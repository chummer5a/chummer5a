using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class Group1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Groupname",
                table: "SINnerVisibility");

            migrationBuilder.DropColumn(
                name: "IsGroupVisible",
                table: "SINnerGroups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Groupname",
                table: "SINnerVisibility",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGroupVisible",
                table: "SINnerGroups",
                nullable: false,
                defaultValue: false);
        }
    }
}
