using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Migrations
{
    public partial class RenameBlogPublishDateTimeUtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn( "BlogPublishDateTimeUtc", "dbo.SitePage", "PublishDateTimeUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("PublishDateTimeUtc", "dbo.SitePage", "BlogPublishDateTimeUtc");
        }
    }
}
