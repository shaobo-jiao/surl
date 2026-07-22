using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Surl.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ShortUrlUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "short_urls",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_urls_user_id",
                table: "short_urls",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_short_urls_users_user_id",
                table: "short_urls",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_short_urls_users_user_id",
                table: "short_urls");

            migrationBuilder.DropIndex(
                name: "ix_short_urls_user_id",
                table: "short_urls");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "short_urls");
        }
    }
}
