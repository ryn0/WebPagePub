using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Migrations
{
    public partial class PhotoFullScreenUrlHeightWidth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoFullScreenUrlHeight",
                table: "SitePagePhoto",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PhotoFullScreenUrlWidth",
                table: "SitePagePhoto",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoFullScreenUrlHeight",
                table: "SitePagePhoto");

            migrationBuilder.DropColumn(
                name: "PhotoFullScreenUrlWidth",
                table: "SitePagePhoto");
        }
    }
}
