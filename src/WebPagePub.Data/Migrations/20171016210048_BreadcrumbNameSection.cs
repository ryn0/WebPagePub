using Microsoft.EntityFrameworkCore.Migrations;

namespace WebPagePub.Data.Migrations
{
    public partial class BreadcrumbNameSection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SitePageSection_Key",
                table: "SitePageSection");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SitePageSection",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SitePageSection",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BreadcrumbName",
                table: "SitePageSection",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_SitePageSection_Key",
                table: "SitePageSection",
                column: "Key",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SitePageSection_Key",
                table: "SitePageSection");

            migrationBuilder.DropColumn(
                name: "BreadcrumbName",
                table: "SitePageSection");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SitePageSection",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SitePageSection",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_SitePageSection_Key",
                table: "SitePageSection",
                column: "Key",
                unique: true,
                filter: "[Key] IS NOT NULL");
        }
    }
}
