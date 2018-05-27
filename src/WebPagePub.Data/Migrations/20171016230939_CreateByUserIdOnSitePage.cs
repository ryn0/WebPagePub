using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Migrations
{
    public partial class CreateByUserIdOnSitePage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreateByUserId",
                table: "SitePage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByuserId",
                table: "SitePage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorUrl",
                table: "AspNetUsers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateByUserId",
                table: "SitePage");

            migrationBuilder.DropColumn(
                name: "UpdatedByuserId",
                table: "SitePage");

            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AuthorUrl",
                table: "AspNetUsers");
        }
    }
}
