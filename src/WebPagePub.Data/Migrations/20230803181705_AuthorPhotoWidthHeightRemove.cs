using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPagePub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuthorPhotoWidthHeightRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoFullScreenUrlHeight",
                table: "Author");

            migrationBuilder.DropColumn(
                name: "PhotoFullScreenUrlWidth",
                table: "Author");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoFullScreenUrlHeight",
                table: "Author",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PhotoFullScreenUrlWidth",
                table: "Author",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
