using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GroupPW'
    public partial class GroupPW : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GroupPW'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GroupPW.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GroupPW.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SINnerGroups",
                nullable: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GroupPW.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GroupPW.Down(MigrationBuilder)'
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SINnerGroups");
        }
    }
}
