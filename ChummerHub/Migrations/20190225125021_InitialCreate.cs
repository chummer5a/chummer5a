using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InitialCreate'
    public partial class InitialCreate : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InitialCreate'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InitialCreate.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InitialCreate.Up(MigrationBuilder)'
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    MyRole = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Groupname = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SINnerComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SINnerId = table.Column<Guid>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Downloads = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SINnerGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    IsGroupVisible = table.Column<bool>(nullable: false),
                    Groupname = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SINnerVisibility",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    IsGroupVisible = table.Column<bool>(nullable: false),
                    Groupname = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerVisibility", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UploadClients",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ChummerVersion = table.Column<string>(nullable: true),
                    ClientSecret = table.Column<string>(nullable: true),
                    UserEmail = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SINnerMetaData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    VisibilityId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerMetaData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SINnerMetaData_SINnerVisibility_VisibilityId",
                        column: x => x.VisibilityId,
                        principalTable: "SINnerVisibility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRights",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SINnerId = table.Column<Guid>(nullable: true),
                    EMail = table.Column<string>(nullable: true),
                    CanEdit = table.Column<bool>(nullable: false),
                    SINnerVisibilityId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRights_SINnerVisibility_SINnerVisibilityId",
                        column: x => x.SINnerVisibilityId,
                        principalTable: "SINnerVisibility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SINners",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DownloadUrl = table.Column<string>(nullable: true),
                    UploadDateTime = table.Column<DateTime>(nullable: true),
                    LastChange = table.Column<DateTime>(nullable: false),
                    UploadClientId = table.Column<Guid>(nullable: false),
                    SINnerMetaDataId = table.Column<Guid>(nullable: true),
                    JsonSummary = table.Column<string>(nullable: true),
                    MyGroupId = table.Column<Guid>(nullable: true),
                    GoogleDriveFileId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SINners_SINnerGroups_MyGroupId",
                        column: x => x.MyGroupId,
                        principalTable: "SINnerGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SINners_SINnerMetaData_SINnerMetaDataId",
                        column: x => x.SINnerMetaDataId,
                        principalTable: "SINnerMetaData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TagName = table.Column<string>(nullable: true),
                    TagValue = table.Column<string>(nullable: true),
                    TagComment = table.Column<string>(nullable: true),
                    ParentTagId = table.Column<Guid>(nullable: true),
                    SINnerId = table.Column<Guid>(nullable: true),
                    IsUserGenerated = table.Column<bool>(nullable: false),
                    TagType = table.Column<int>(nullable: false),
                    SINnerMetaDataId = table.Column<Guid>(nullable: true),
                    TagId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_SINnerMetaData_SINnerMetaDataId",
                        column: x => x.SINnerMetaDataId,
                        principalTable: "SINnerMetaData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerMetaData_VisibilityId",
                table: "SINnerMetaData",
                column: "VisibilityId");

            migrationBuilder.CreateIndex(
                name: "IX_SINners_MyGroupId",
                table: "SINners",
                column: "MyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SINners_SINnerMetaDataId",
                table: "SINners",
                column: "SINnerMetaDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_SINnerMetaDataId",
                table: "Tags",
                column: "SINnerMetaDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagId",
                table: "Tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagName_TagValue",
                table: "Tags",
                columns: new[] { "TagName", "TagValue" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRights_SINnerId",
                table: "UserRights",
                column: "SINnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRights_SINnerVisibilityId",
                table: "UserRights",
                column: "SINnerVisibilityId");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InitialCreate.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InitialCreate.Down(MigrationBuilder)'
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "SINnerComments");

            migrationBuilder.DropTable(
                name: "SINners");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "UploadClients");

            migrationBuilder.DropTable(
                name: "UserRights");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "SINnerGroups");

            migrationBuilder.DropTable(
                name: "SINnerMetaData");

            migrationBuilder.DropTable(
                name: "SINnerVisibility");
        }
    }
}
