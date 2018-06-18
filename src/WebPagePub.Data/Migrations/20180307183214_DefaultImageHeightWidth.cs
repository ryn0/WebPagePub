using Microsoft.EntityFrameworkCore.Migrations;

namespace WebPagePub.Data.Migrations
{
    public partial class DefaultImageHeightWidth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Width",
                table: "SitePagePhoto",
                newName: "DefaultImageWidth");

            migrationBuilder.RenameColumn(
                name: "Height",
                table: "SitePagePhoto",
                newName: "DefaultImageHeight");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DefaultImageWidth",
                table: "SitePagePhoto",
                newName: "Width");

            migrationBuilder.RenameColumn(
                name: "DefaultImageHeight",
                table: "SitePagePhoto",
                newName: "Height");
        }
    }
}
