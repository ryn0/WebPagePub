using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPagePub.Data.Migrations
{
    /// <inheritdoc />
    public partial class SearchLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSearchLogs",
                columns: table => new
                {
                    SiteSearchLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Term = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    PageSize = table.Column<int>(type: "int", nullable: false),
                    ResultsCount = table.Column<int>(type: "int", nullable: false),
                    ClientIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Referer = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSearchLogs", x => x.SiteSearchLogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteSearchLogs_CreateDate",
                table: "SiteSearchLogs",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSearchLogs_Term",
                table: "SiteSearchLogs",
                column: "Term");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSearchLogs");
        }
    }
}
