using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class _01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Base64EncodedXmlFile",
                table: "SINners",
                newName: "DownloadUrl");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DownloadUrl",
                table: "SINners",
                newName: "Base64EncodedXmlFile");
        }
    }
}
