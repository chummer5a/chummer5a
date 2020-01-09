using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'editionnumber'
    public partial class editionnumber : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'editionnumber'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'editionnumber.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'editionnumber.Up(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINners_SINnerExtended_MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SINnerExtended",
                table: "SINnerExtended");

            migrationBuilder.RenameTable(
                name: "SINnerExtended",
                newName: "SINnerExtendedMetaData");

            migrationBuilder.AddColumn<string>(
                name: "EditionNumber",
                table: "SINners",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SINnerExtendedMetaData",
                table: "SINnerExtendedMetaData",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SINners_SINnerExtendedMetaData_MyExtendedAttributesId",
                table: "SINners",
                column: "MyExtendedAttributesId",
                principalTable: "SINnerExtendedMetaData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'editionnumber.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'editionnumber.Down(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINners_SINnerExtendedMetaData_MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SINnerExtendedMetaData",
                table: "SINnerExtendedMetaData");

            migrationBuilder.DropColumn(
                name: "EditionNumber",
                table: "SINners");

            migrationBuilder.RenameTable(
                name: "SINnerExtendedMetaData",
                newName: "SINnerExtended");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SINnerExtended",
                table: "SINnerExtended",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SINners_SINnerExtended_MyExtendedAttributesId",
                table: "SINners",
                column: "MyExtendedAttributesId",
                principalTable: "SINnerExtended",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
