using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class userrights2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINerUserRight_SINnerVisibility_SINnerVisibilityId",
                table: "SINerUserRight");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_SINnerMetaData_SINnerMetaDataId",
                table: "Tag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Tag_TagId",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SINerUserRight",
                table: "SINerUserRight");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "SINerUserRight",
                newName: "UserRights");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_TagName_TagValue",
                table: "Tags",
                newName: "IX_Tags_TagName_TagValue");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_TagId",
                table: "Tags",
                newName: "IX_Tags_TagId");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_SINnerMetaDataId",
                table: "Tags",
                newName: "IX_Tags_SINnerMetaDataId");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_Id",
                table: "Tags",
                newName: "IX_Tags_Id");

            migrationBuilder.RenameIndex(
                name: "IX_SINerUserRight_SINnerVisibilityId",
                table: "UserRights",
                newName: "IX_UserRights_SINnerVisibilityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRights",
                table: "UserRights",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_SINnerMetaData_SINnerMetaDataId",
                table: "Tags",
                column: "SINnerMetaDataId",
                principalTable: "SINnerMetaData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_TagId",
                table: "Tags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRights_SINnerVisibility_SINnerVisibilityId",
                table: "UserRights",
                column: "SINnerVisibilityId",
                principalTable: "SINnerVisibility",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_SINnerMetaData_SINnerMetaDataId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_TagId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRights_SINnerVisibility_SINnerVisibilityId",
                table: "UserRights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRights",
                table: "UserRights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.RenameTable(
                name: "UserRights",
                newName: "SINerUserRight");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.RenameIndex(
                name: "IX_UserRights_SINnerVisibilityId",
                table: "SINerUserRight",
                newName: "IX_SINerUserRight_SINnerVisibilityId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_TagName_TagValue",
                table: "Tag",
                newName: "IX_Tag_TagName_TagValue");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_TagId",
                table: "Tag",
                newName: "IX_Tag_TagId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_SINnerMetaDataId",
                table: "Tag",
                newName: "IX_Tag_SINnerMetaDataId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_Id",
                table: "Tag",
                newName: "IX_Tag_Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SINerUserRight",
                table: "SINerUserRight",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SINerUserRight_SINnerVisibility_SINnerVisibilityId",
                table: "SINerUserRight",
                column: "SINnerVisibilityId",
                principalTable: "SINnerVisibility",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_SINnerMetaData_SINnerMetaDataId",
                table: "Tag",
                column: "SINnerMetaDataId",
                principalTable: "SINnerMetaData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Tag_TagId",
                table: "Tag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
