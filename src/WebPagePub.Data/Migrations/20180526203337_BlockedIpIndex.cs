using Microsoft.EntityFrameworkCore.Migrations;

namespace WebPagePub.Data.Migrations
{
    public partial class BlockedIpIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BlockedIP_IpAddress",
                table: "BlockedIP",
                column: "IpAddress",
                unique: true,
                filter: "[IpAddress] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BlockedIP_IpAddress",
                table: "BlockedIP");
        }
    }
}
