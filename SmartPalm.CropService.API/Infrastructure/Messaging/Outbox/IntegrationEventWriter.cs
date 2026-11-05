using System.Text.Json;
using SmartPalm.CropService.Application.Abstractions;
using SmartPalm.CropService.Infrastructure.Persistence;

namespace SmartPalm.CropService.Infrastructure.Messaging;

internal class IntegrationEventWriter(CropDbContext context) : IIntegrationEventWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Enqueue<T>(string eventType, T payload)
    {
        var eventId = Guid.NewGuid();
        var occurredAtUtc = DateTime.UtcNow;
        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = eventId,
            EventType = eventType,
            OccurredOnUtc = occurredAtUtc,
            Content = JsonSerializer.Serialize(new
            {
                eventId,
                eventType,
                schemaVersion = 1,
                occurredAtUtc,
                correlationId = Guid.NewGuid(),
                producer = "smartpalm.crop-service",
                payload
            }, SerializerOptions)
        });
    }
}
