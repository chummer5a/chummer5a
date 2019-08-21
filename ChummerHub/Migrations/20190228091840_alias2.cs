using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias2'
    public partial class alias2 : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias2'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias2.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias2.Up(MigrationBuilder)'
        {
            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "UserRights",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alias",
                table: "SINners",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Groupname",
                table: "SINnerGroups",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRights_EMail",
                table: "UserRights",
                column: "EMail");

            migrationBuilder.CreateIndex(
                name: "IX_SINners_Alias",
                table: "SINners",
                column: "Alias");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_Groupname",
                table: "SINnerGroups",
                column: "Groupname");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias2.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias2.Down(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRights_EMail",
                table: "UserRights");

            migrationBuilder.DropIndex(
                name: "IX_SINners_Alias",
                table: "SINners");

            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_Groupname",
                table: "SINnerGroups");

            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "UserRights",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alias",
                table: "SINners",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Groupname",
                table: "SINnerGroups",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
