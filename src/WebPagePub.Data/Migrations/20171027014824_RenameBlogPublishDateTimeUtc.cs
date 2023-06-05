using Microsoft.EntityFrameworkCore.Migrations;

namespace WebPagePub.Data.Migrations
{
    public partial class RenameBlogPublishDateTimeUtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("EXEC sp_rename N'dbo.SitePage.BlogPublishDateTimeUtc', N'PublishDateTimeUtc', N'COLUMN';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql("EXEC sp_rename N'dbo.SitePage.PublishDateTimeUtc', N'BlogPublishDateTimeUtc', N'COLUMN';");
        }
    }
}
