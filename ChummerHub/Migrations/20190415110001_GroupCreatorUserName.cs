using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GroupCreatorUserName'
    public partial class GroupCreatorUserName : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GroupCreatorUserName'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GroupCreatorUserName.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GroupCreatorUserName.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupCreatorUserName",
                table: "SINnerGroups",
                nullable: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GroupCreatorUserName.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GroupCreatorUserName.Down(MigrationBuilder)'
        {
            migrationBuilder.DropColumn(
                name: "GroupCreatorUserName",
                table: "SINnerGroups");
        }
    }
}
