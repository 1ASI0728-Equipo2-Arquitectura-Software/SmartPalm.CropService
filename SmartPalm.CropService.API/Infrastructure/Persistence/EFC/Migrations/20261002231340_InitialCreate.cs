using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartPalm.CropService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "crop");

            migrationBuilder.CreateTable(
                name: "agronomist_plantation_affiliations",
                schema: "crop",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    agronomist_id = table.Column<long>(type: "bigint", nullable: false),
                    plantation_id = table.Column<long>(type: "bigint", nullable: false),
                    affiliated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    detached_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agronomist_plantation_affiliations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "crop",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plantations",
                schema: "crop",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    palm_grower_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    coordinates = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hectares = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estimated_sensors = table.Column<int>(type: "integer", nullable: false),
                    installation_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plantations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sectors",
                schema: "crop",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plantation_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    iot_device_mac_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sectors", x => x.id);
                    table.ForeignKey(
                        name: "fk_sectors_plantations_plantation_id",
                        column: x => x.plantation_id,
                        principalSchema: "crop",
                        principalTable: "plantations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agronomist_plantation_affiliations_agronomist_id_plantation",
                schema: "crop",
                table: "agronomist_plantation_affiliations",
                columns: new[] { "agronomist_id", "plantation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agronomist_plantation_affiliations_plantation_id",
                schema: "crop",
                table: "agronomist_plantation_affiliations",
                column: "plantation_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_utc_occurred_on_utc",
                schema: "crop",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "occurred_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_plantations_palm_grower_id",
                schema: "crop",
                table: "plantations",
                column: "palm_grower_id");

            migrationBuilder.CreateIndex(
                name: "ix_sectors_iot_device_mac_address",
                schema: "crop",
                table: "sectors",
                column: "iot_device_mac_address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sectors_plantation_id",
                schema: "crop",
                table: "sectors",
                column: "plantation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agronomist_plantation_affiliations",
                schema: "crop");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "crop");

            migrationBuilder.DropTable(
                name: "sectors",
                schema: "crop");

            migrationBuilder.DropTable(
                name: "plantations",
                schema: "crop");
        }
    }
}
