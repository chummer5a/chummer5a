using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class sinnergroup_hash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "SINnerGroups",
                maxLength: 8,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_Hash",
                table: "SINnerGroups",
                column: "Hash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_Hash",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "SINnerGroups");
        }
    }
}
