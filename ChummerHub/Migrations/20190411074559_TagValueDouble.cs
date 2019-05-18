using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class TagValueDouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TagValueDouble",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags",
                column: "TagValueDouble");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagValueDouble",
                table: "Tags");
        }
    }
}
