using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations.IdentityServer.PersistedGrantDb
{
    public partial class _003 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PersistedGrants",
                schema: "Grant",
                newName: "PersistedGrants");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Grant");

            migrationBuilder.RenameTable(
                name: "PersistedGrants",
                newName: "PersistedGrants",
                newSchema: "Grant");
        }
    }
}
