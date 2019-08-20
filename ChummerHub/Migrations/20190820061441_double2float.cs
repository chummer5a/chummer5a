using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class double2float : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagValueDouble",
                table: "Tags");

            migrationBuilder.AddColumn<float>(
                name: "TagValueFloat",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagValueFloat",
                table: "Tags",
                column: "TagValueFloat");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_TagValueFloat",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagValueFloat",
                table: "Tags");

            migrationBuilder.AddColumn<double>(
                name: "TagValueDouble",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags",
                column: "TagValueDouble");
        }
    }
}
