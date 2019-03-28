using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class passwordhash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameMasterUsername",
                table: "SINnerGroups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameMasterUsername",
                table: "SINnerGroups",
                nullable: true);
        }
    }
}
