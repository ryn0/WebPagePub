using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Migrations
{
    public partial class FixCaseOnUpdatedByUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedByuserId",
                table: "SitePage",
                newName: "UpdatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedByUserId",
                table: "SitePage",
                newName: "UpdatedByuserId");
        }
    }
}
