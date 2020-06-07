using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'passwordhash'
    public partial class passwordhash : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'passwordhash'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'passwordhash.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'passwordhash.Up(MigrationBuilder)'
        {
            migrationBuilder.DropColumn(
                name: "GameMasterUsername",
                table: "SINnerGroups");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'passwordhash.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'passwordhash.Down(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<string>(
                name: "GameMasterUsername",
                table: "SINnerGroups",
                nullable: true);
        }
    }
}
