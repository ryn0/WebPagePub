using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebPagePub.Data.Migrations
{
    public partial class PageComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowsComments",
                table: "SitePage",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SitePageComment",
                columns: table => new
                {
                    SitePageCommentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Comment = table.Column<string>(nullable: true),
                    CommentStatus = table.Column<byte>(nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(maxLength: 75, nullable: true),
                    IpAddress = table.Column<string>(maxLength: 75, nullable: true),
                    Name = table.Column<string>(maxLength: 75, nullable: true),
                    RequestId = table.Column<Guid>(nullable: false),
                    SitePageId = table.Column<int>(nullable: false),
                    WebSite = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePageComment", x => x.SitePageCommentId);
                    table.ForeignKey(
                        name: "FK_SitePageComment_SitePage_SitePageId",
                        column: x => x.SitePageId,
                        principalTable: "SitePage",
                        principalColumn: "SitePageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SitePageComment_SitePageId",
                table: "SitePageComment",
                column: "SitePageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SitePageComment");

            migrationBuilder.DropColumn(
                name: "AllowsComments",
                table: "SitePage");
        }
    }
}
