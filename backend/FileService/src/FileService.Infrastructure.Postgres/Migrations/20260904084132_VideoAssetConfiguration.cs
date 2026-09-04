using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class VideoAssetConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preview_keys",
                table: "media_assets",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sprite_preview_key",
                table: "media_assets",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_metadata",
                table: "media_assets",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preview_keys",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "sprite_preview_key",
                table: "media_assets");

            migrationBuilder.DropColumn(
                name: "video_metadata",
                table: "media_assets");
        }
    }
}
