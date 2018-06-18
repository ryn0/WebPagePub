using Microsoft.EntityFrameworkCore.Migrations;

namespace WebPagePub.Data.Migrations
{
    public partial class ReviewPageData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SitePage",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 255);

            migrationBuilder.AddColumn<int>(
                name: "PageType",
                table: "SitePage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ReviewBestValue",
                table: "SitePage",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ReviewItemName",
                table: "SitePage",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ReviewRatingValue",
                table: "SitePage",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ReviewWorstValue",
                table: "SitePage",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageType",
                table: "SitePage");

            migrationBuilder.DropColumn(
                name: "ReviewBestValue",
                table: "SitePage");

            migrationBuilder.DropColumn(
                name: "ReviewItemName",
                table: "SitePage");

            migrationBuilder.DropColumn(
                name: "ReviewRatingValue",
                table: "SitePage");

            migrationBuilder.DropColumn(
                name: "ReviewWorstValue",
                table: "SitePage");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SitePage",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);
        }
    }
}
