using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class _05 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerComment_SINnerMetaData_SINnerMetaDataId",
                table: "SINnerComment");

            migrationBuilder.RenameColumn(
                name: "SINnerMetaDataId",
                table: "SINnerComment",
                newName: "SINnerId");

            migrationBuilder.RenameIndex(
                name: "IX_SINnerComment_SINnerMetaDataId",
                table: "SINnerComment",
                newName: "IX_SINnerComment_SINnerId");

            migrationBuilder.AlterColumn<string>(
                name: "TagValue",
                table: "Tag",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "Tag",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tag_SINnerId",
                table: "Tag",
                column: "SINnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TagId",
                table: "Tag",
                column: "TagId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TagName_TagValue",
                table: "Tag",
                columns: new[] { "TagName", "TagValue" });

            migrationBuilder.CreateIndex(
                name: "IX_SINners_SINnerId",
                table: "SINners",
                column: "SINnerId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tag_SINnerId",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_TagId",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_TagName_TagValue",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_SINners_SINnerId",
                table: "SINners");

            migrationBuilder.RenameColumn(
                name: "SINnerId",
                table: "SINnerComment",
                newName: "SINnerMetaDataId");

            migrationBuilder.RenameIndex(
                name: "IX_SINnerComment_SINnerId",
                table: "SINnerComment",
                newName: "IX_SINnerComment_SINnerMetaDataId");

            migrationBuilder.AlterColumn<string>(
                name: "TagValue",
                table: "Tag",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "Tag",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerComment_SINnerMetaData_SINnerMetaDataId",
                table: "SINnerComment",
                column: "SINnerMetaDataId",
                principalTable: "SINnerMetaData",
                principalColumn: "SINnerMetaDataId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
