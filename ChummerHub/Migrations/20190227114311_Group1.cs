using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Group1'
    public partial class Group1 : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Group1'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Group1.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Group1.Up(MigrationBuilder)'
        {
            migrationBuilder.DropColumn(
                name: "Groupname",
                table: "SINnerVisibility");

            migrationBuilder.DropColumn(
                name: "IsGroupVisible",
                table: "SINnerGroups");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Group1.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Group1.Down(MigrationBuilder)'
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
