﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPagePub.Data.Migrations
{
    /// <inheritdoc />
    public partial class IsBotOnClick : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBot",
                table: "ClickLog",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBot",
                table: "ClickLog");
        }
    }
}
