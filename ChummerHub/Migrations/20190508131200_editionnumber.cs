using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class editionnumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
