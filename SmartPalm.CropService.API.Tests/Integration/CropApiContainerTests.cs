using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SmartPalm.CropService.API.Tests.Integration;

public class CropApiContainerTests : IAsyncLifetime
{
    private const string JwtKey = "crop-tests-signing-key-with-at-least-32-characters";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("smartpalm_crop")
        .WithUsername("smartpalm_crop")
        .WithPassword("crop-tests")
        .Build();
    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:4-management-alpine").Build();

    public async Task InitializeAsync() =>
        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());

    public async Task DisposeAsync() =>
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbitMq.DisposeAsync().AsTask());

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task CreatePlantation_PersistsAggregateAndPublishesOutboxMessage()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:CropDatabase", _postgres.GetConnectionString());
            builder.UseSetting("Jwt:Issuer", "smartpalm.identity.tests");
            builder.UseSetting("Jwt:Audience", "smartpalm.clients.tests");
            builder.UseSetting("Jwt:Key", JwtKey);
            builder.UseSetting("RabbitMq:Enabled", "true");
            builder.UseSetting("RabbitMq:Uri", _rabbitMq.GetConnectionString());
        });

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdministratorToken());
        var response = await client.PostAsJsonAsync("/api/v1/plantations", new
        {
            palmGrowerId = 7L,
            name = "Integration Plantation",
            hectares = 25.5m,
            address = "Ucayali",
            coordinates = "-8.475,-74.805"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        var published = false;
        for (var attempt = 0; attempt < 10 && !published; attempt++)
        {
            await using var command = new NpgsqlCommand(
                "SELECT COUNT(*) FROM crop.outbox_messages WHERE processed_on_utc IS NOT NULL",
                connection);
            published = Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
            if (!published) await Task.Delay(TimeSpan.FromSeconds(1));
        }
        Assert.True(published, "The Crop outbox message was not published within the retry window.");
    }

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task ReconciliationSnapshot_ReturnsBoundedPlantationsSectorsAndActiveAffiliations()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:CropDatabase", _postgres.GetConnectionString());
            builder.UseSetting("Jwt:Issuer", "smartpalm.identity.tests");
            builder.UseSetting("Jwt:Audience", "smartpalm.clients.tests");
            builder.UseSetting("Jwt:Key", JwtKey);
            builder.UseSetting("RabbitMq:Enabled", "false");
        });

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdministratorToken());
        var plantation = await client.PostAsJsonAsync("/api/v1/plantations", new
        {
            palmGrowerId = 7L,
            name = "Snapshot Plantation",
            hectares = 10m,
            address = "Ucayali"
        });
        var plantationBody = await plantation.Content.ReadFromJsonAsync<JsonElement>();
        var plantationId = plantationBody.GetProperty("id").GetInt64();

        var sector = await client.PostAsJsonAsync($"/api/v1/admin/plantations/{plantationId}/sectors", new { sectorName = "North" });
        Assert.Equal(HttpStatusCode.Created, sector.StatusCode);
        var affiliation = await client.PostAsJsonAsync("/api/v1/agronomists/plantation-affiliations", new
        {
            agronomistId = 7L,
            plantationId
        });
        Assert.Equal(HttpStatusCode.Created, affiliation.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAgronomyServiceToken());
        var response = await client.GetAsync("/api/v1/internal/reconciliation/snapshot?limit=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var snapshot = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(snapshot.GetProperty("plantations").EnumerateArray());
        Assert.Single(snapshot.GetProperty("sectors").EnumerateArray());
        Assert.Single(snapshot.GetProperty("activeAgronomistAffiliations").EnumerateArray());
        Assert.Equal(plantationId, snapshot.GetProperty("plantations")[0].GetProperty("id").GetInt64());
        Assert.Equal(plantationId, snapshot.GetProperty("sectors")[0].GetProperty("plantationId").GetInt64());
    }

    private static string CreateAdministratorToken()
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "smartpalm.identity.tests",
            audience: "smartpalm.clients.tests",
            claims:
            [
                new Claim("sub", "42"),
                new Claim("sid", "42"),
                new Claim("name", "integration-admin"),
                new Claim("role", "Administrator")
            ],
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateAgronomyServiceToken()
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "smartpalm.identity.tests",
            audience: "smartpalm.clients.tests",
            claims:
            [
                new Claim("sub", "agronomy-service"),
                new Claim("name", "agronomy-service"),
                new Claim("role", "AgronomyService")
            ],
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
