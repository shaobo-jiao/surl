using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UrlShortener.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "url_shortener");

            migrationBuilder.CreateTable(
                name: "short_urls",
                schema: "url_shortener",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    original_url = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_urls", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "short_url_clicks",
                schema: "url_shortener",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    short_url_id = table.Column<long>(type: "bigint", nullable: false),
                    clicked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_url_clicks", x => x.id);
                    table.ForeignKey(
                        name: "fk_short_url_clicks_short_urls_short_url_id",
                        column: x => x.short_url_id,
                        principalSchema: "url_shortener",
                        principalTable: "short_urls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_short_url_clicks_short_url_id",
                schema: "url_shortener",
                table: "short_url_clicks",
                column: "short_url_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "short_url_clicks",
                schema: "url_shortener");

            migrationBuilder.DropTable(
                name: "short_urls",
                schema: "url_shortener");
        }
    }
}
