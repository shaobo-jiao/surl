using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surl.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDefaultSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "short_urls",
                schema: "url_shortener",
                newName: "short_urls");

            migrationBuilder.RenameTable(
                name: "short_url_clicks",
                schema: "url_shortener",
                newName: "short_url_clicks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "url_shortener");

            migrationBuilder.RenameTable(
                name: "short_urls",
                newName: "short_urls",
                newSchema: "url_shortener");

            migrationBuilder.RenameTable(
                name: "short_url_clicks",
                newName: "short_url_clicks",
                newSchema: "url_shortener");
        }
    }
}
