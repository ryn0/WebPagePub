using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPagePub.Data.Migrations
{
    /// <inheritdoc />
    public partial class SitePageAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Author",
                columns: table => new
                {
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    AuthorBio = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhotoOriginalUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoThumbUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoPreviewUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoFullScreenUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoFullScreenUrlHeight = table.Column<int>(type: "int", nullable: false),
                    PhotoFullScreenUrlWidth = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Author", x => x.AuthorId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Author_AuthorId",
                table: "Author",
                column: "AuthorId",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "SitePage",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddForeignKey(
                name: "FK_SitePage_Author_AuthorId",
                table: "SitePage",
                column: "AuthorId",
                principalTable: "Author",
                principalColumn: "AuthorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SitePage_Author_AuthorId",
                table: "SitePage");

            migrationBuilder.DropTable(
                name: "Author");

            migrationBuilder.DropColumn(
                name: "SitePage",
                table: "AuthorId");
        }
    }
}
