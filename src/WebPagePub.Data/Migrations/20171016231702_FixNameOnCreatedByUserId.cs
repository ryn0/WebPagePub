using Microsoft.EntityFrameworkCore.Migrations;

namespace WebPagePub.Data.Migrations
{
    public partial class FixNameOnCreatedByUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateByUserId",
                table: "SitePage");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "SitePage",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SitePage");

            migrationBuilder.AddColumn<string>(
                name: "CreateByUserId",
                table: "SitePage",
                maxLength: 36,
                nullable: true);
        }
    }
}
