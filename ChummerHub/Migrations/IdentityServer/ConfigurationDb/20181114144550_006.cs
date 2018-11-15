using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations.IdentityServer.ConfigurationDb
{
    public partial class _006 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "IdentityResources",
                schema: "Config",
                newName: "IdentityResources");

            migrationBuilder.RenameTable(
                name: "IdentityClaims",
                schema: "Config",
                newName: "IdentityClaims");

            migrationBuilder.RenameTable(
                name: "ClientSecrets",
                schema: "Config",
                newName: "ClientSecrets");

            migrationBuilder.RenameTable(
                name: "ClientScopes",
                schema: "Config",
                newName: "ClientScopes");

            migrationBuilder.RenameTable(
                name: "Clients",
                schema: "Config",
                newName: "Clients");

            migrationBuilder.RenameTable(
                name: "ClientRedirectUris",
                schema: "Config",
                newName: "ClientRedirectUris");

            migrationBuilder.RenameTable(
                name: "ClientProperties",
                schema: "Config",
                newName: "ClientProperties");

            migrationBuilder.RenameTable(
                name: "ClientPostLogoutRedirectUris",
                schema: "Config",
                newName: "ClientPostLogoutRedirectUris");

            migrationBuilder.RenameTable(
                name: "ClientIdPRestrictions",
                schema: "Config",
                newName: "ClientIdPRestrictions");

            migrationBuilder.RenameTable(
                name: "ClientGrantTypes",
                schema: "Config",
                newName: "ClientGrantTypes");

            migrationBuilder.RenameTable(
                name: "ClientCorsOrigins",
                schema: "Config",
                newName: "ClientCorsOrigins");

            migrationBuilder.RenameTable(
                name: "ClientClaims",
                schema: "Config",
                newName: "ClientClaims");

            migrationBuilder.RenameTable(
                name: "ApiSecrets",
                schema: "Config",
                newName: "ApiSecrets");

            migrationBuilder.RenameTable(
                name: "ApiScopes",
                schema: "Config",
                newName: "ApiScopes");

            migrationBuilder.RenameTable(
                name: "ApiScopeClaims",
                schema: "Config",
                newName: "ApiScopeClaims");

            migrationBuilder.RenameTable(
                name: "ApiResources",
                schema: "Config",
                newName: "ApiResources");

            migrationBuilder.RenameTable(
                name: "ApiClaims",
                schema: "Config",
                newName: "ApiClaims");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Config");

            migrationBuilder.RenameTable(
                name: "IdentityResources",
                newName: "IdentityResources",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "IdentityClaims",
                newName: "IdentityClaims",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientSecrets",
                newName: "ClientSecrets",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientScopes",
                newName: "ClientScopes",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "Clients",
                newName: "Clients",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientRedirectUris",
                newName: "ClientRedirectUris",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientProperties",
                newName: "ClientProperties",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientPostLogoutRedirectUris",
                newName: "ClientPostLogoutRedirectUris",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientIdPRestrictions",
                newName: "ClientIdPRestrictions",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientGrantTypes",
                newName: "ClientGrantTypes",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientCorsOrigins",
                newName: "ClientCorsOrigins",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ClientClaims",
                newName: "ClientClaims",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ApiSecrets",
                newName: "ApiSecrets",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ApiScopes",
                newName: "ApiScopes",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ApiScopeClaims",
                newName: "ApiScopeClaims",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ApiResources",
                newName: "ApiResources",
                newSchema: "Config");

            migrationBuilder.RenameTable(
                name: "ApiClaims",
                newName: "ApiClaims",
                newSchema: "Config");
        }
    }
}
