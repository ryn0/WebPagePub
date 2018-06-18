using Microsoft.EntityFrameworkCore.Migrations;

namespace WebPagePub.Data.Migrations
{
    public partial class HeightWidth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "SitePagePhoto",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "SitePagePhoto",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "SitePagePhoto");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "SitePagePhoto");
        }
    }
}
