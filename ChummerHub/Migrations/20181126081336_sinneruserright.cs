using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class sinneruserright : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerMetaData_SINnerVisibility_VisibilitySINnerVisibilityId",
                table: "SINnerMetaData");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Tag_TagId1",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_SINnerId",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_TagId",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_TagId1",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_SINners_UploadClientId",
                table: "SINners");

            migrationBuilder.DropIndex(
                name: "IX_SINnerComments_SINnerId",
                table: "SINnerComments");

            migrationBuilder.DropColumn(
                name: "TagId1",
                table: "Tag");

            migrationBuilder.RenameColumn(
                name: "UploadClientId",
                table: "UploadClients",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SINnerVisibilityId",
                table: "SINnerVisibility",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SINnerId",
                table: "SINners",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_SINners_SINnerId",
                table: "SINners",
                newName: "IX_SINners_Id");

            migrationBuilder.RenameColumn(
                name: "VisibilitySINnerVisibilityId",
                table: "SINnerMetaData",
                newName: "VisibilityId");

            migrationBuilder.RenameColumn(
                name: "SINnerMetaDataId",
                table: "SINnerMetaData",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_SINnerMetaData_VisibilitySINnerVisibilityId",
                table: "SINnerMetaData",
                newName: "IX_SINnerMetaData_VisibilityId");

            migrationBuilder.RenameColumn(
                name: "SINnerCommentId",
                table: "SINnerComments",
                newName: "Id");

            migrationBuilder.AlterColumn<Guid>(
                name: "TagId",
                table: "Tag",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Tag",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SINerUserRight",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EMail = table.Column<string>(nullable: true),
                    CanEdit = table.Column<bool>(nullable: false),
                    SINnerVisibilityId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINerUserRight", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SINerUserRight_SINnerVisibility_SINnerVisibilityId",
                        column: x => x.SINnerVisibilityId,
                        principalTable: "SINnerVisibility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Id",
                table: "Tag",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TagId",
                table: "Tag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerComments_Id",
                table: "SINnerComments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SINerUserRight_SINnerVisibilityId",
                table: "SINerUserRight",
                column: "SINnerVisibilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerMetaData_SINnerVisibility_VisibilityId",
                table: "SINnerMetaData",
                column: "VisibilityId",
                principalTable: "SINnerVisibility",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerMetaData_SINnerVisibility_VisibilityId",
                table: "SINnerMetaData");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Tag_TagId",
                table: "Tag");

            migrationBuilder.DropTable(
                name: "SINerUserRight");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_Id",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_TagId",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_SINnerComments_Id",
                table: "SINnerComments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Tag");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UploadClients",
                newName: "UploadClientId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SINnerVisibility",
                newName: "SINnerVisibilityId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SINners",
                newName: "SINnerId");

            migrationBuilder.RenameIndex(
                name: "IX_SINners_Id",
                table: "SINners",
                newName: "IX_SINners_SINnerId");

            migrationBuilder.RenameColumn(
                name: "VisibilityId",
                table: "SINnerMetaData",
                newName: "VisibilitySINnerVisibilityId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SINnerMetaData",
                newName: "SINnerMetaDataId");

            migrationBuilder.RenameIndex(
                name: "IX_SINnerMetaData_VisibilityId",
                table: "SINnerMetaData",
                newName: "IX_SINnerMetaData_VisibilitySINnerVisibilityId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SINnerComments",
                newName: "SINnerCommentId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TagId",
                table: "Tag",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TagId1",
                table: "Tag",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "TagId");

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
                name: "IX_Tag_TagId1",
                table: "Tag",
                column: "TagId1");

            migrationBuilder.CreateIndex(
                name: "IX_SINners_UploadClientId",
                table: "SINners",
                column: "UploadClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerComments_SINnerId",
                table: "SINnerComments",
                column: "SINnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerMetaData_SINnerVisibility_VisibilitySINnerVisibilityId",
                table: "SINnerMetaData",
                column: "VisibilitySINnerVisibilityId",
                principalTable: "SINnerVisibility",
                principalColumn: "SINnerVisibilityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Tag_TagId1",
                table: "Tag",
                column: "TagId1",
                principalTable: "Tag",
                principalColumn: "TagId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
