using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Migrations
{
    public partial class SiteSection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SitePage_SitePageSection_SitePageSectionId",
                table: "SitePage");

            migrationBuilder.DropIndex(
                name: "IX_SitePage_Key",
                table: "SitePage");

            migrationBuilder.DropIndex(
                name: "IX_SitePage_SitePageSectionId",
                table: "SitePage");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SitePageSection");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SitePageSection",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "SitePageSection",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SitePageSectionId",
                table: "SitePage",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePageSection_Key",
                table: "SitePageSection",
                column: "Key",
                unique: true,
                filter: "[Key] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SitePage_SitePageSectionId_Key",
                table: "SitePage",
                columns: new[] { "SitePageSectionId", "Key" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SitePage_SitePageSection_SitePageSectionId",
                table: "SitePage",
                column: "SitePageSectionId",
                principalTable: "SitePageSection",
                principalColumn: "SitePageSectionId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SitePage_SitePageSection_SitePageSectionId",
                table: "SitePage");

            migrationBuilder.DropIndex(
                name: "IX_SitePageSection_Key",
                table: "SitePageSection");

            migrationBuilder.DropIndex(
                name: "IX_SitePage_SitePageSectionId_Key",
                table: "SitePage");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "SitePageSection");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "SitePageSection",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SitePageSection",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SitePageSectionId",
                table: "SitePage",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_SitePage_Key",
                table: "SitePage",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePage_SitePageSectionId",
                table: "SitePage",
                column: "SitePageSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SitePage_SitePageSection_SitePageSectionId",
                table: "SitePage",
                column: "SitePageSectionId",
                principalTable: "SitePageSection",
                principalColumn: "SitePageSectionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
