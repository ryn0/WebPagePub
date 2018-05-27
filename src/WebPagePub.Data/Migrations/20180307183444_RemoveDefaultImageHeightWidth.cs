using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Migrations
{
    public partial class RemoveDefaultImageHeightWidth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultImageHeight",
                table: "SitePagePhoto");

            migrationBuilder.DropColumn(
                name: "DefaultImageWidth",
                table: "SitePagePhoto");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultImageHeight",
                table: "SitePagePhoto",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultImageWidth",
                table: "SitePagePhoto",
                nullable: false,
                defaultValue: 0);
        }
    }
}
