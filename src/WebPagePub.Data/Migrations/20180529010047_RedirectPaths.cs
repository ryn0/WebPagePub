using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Migrations
{
    public partial class RedirectPaths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RedirectPath",
                columns: table => new
                {
                    RedirectPathId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Path = table.Column<string>(nullable: true),
                    PathDestination = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectPath", x => x.RedirectPathId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RedirectPath_Path",
                table: "RedirectPath",
                column: "Path",
                unique: true,
                filter: "[Path] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedirectPath");
        }
    }
}
