using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPagePub.Data.Migrations
{
    /// <inheritdoc />
    public partial class SitePageWordCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WordCount",
                table: "SitePage",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WordCount",
                table: "SitePage");
        }
    }
}
