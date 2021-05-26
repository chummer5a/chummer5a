using ChummerHub.API;
using ChummerHub.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChummerHub
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SeedData'
    public class SeedData
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SeedData'
    {

        #region snippet_Initialize
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SeedData.Initialize(IServiceProvider, string, IHostingEnvironment)'
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw, IHostingEnvironment env)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SeedData.Initialize(IServiceProvider, string, IHostingEnvironment)'
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>(), env))
            {

                if (context.Users.Any())
                {
                    return;   // DB has been seeded
                }
                //var config = serviceProvider.GetRequiredService<IConfiguration>();
                //currently AzureLogins need to be created on the master-DB
                //Todo: implement this with a seperate connection to the masters-DB
                //var sqlMasterUser = GetSqlCommandMasterUser(config["SqlSinnerUserName"], config["SqlSinnerUserPW"]);
                //context.Database.ExecuteSqlCommandAsync(sqlMasterUser);
                foreach (var user in Config.GetAdminUsers())
                {
                    var userID = await EnsureUser(serviceProvider, user, testUserPw);
                    await EnsureRole(serviceProvider, user.Id, Authorizarion.Constants.AdministratorsRole, null, null);
                    await EnsureRole(serviceProvider, user.Id, Authorizarion.Constants.UserRoleRegistered, null, null);
                    await EnsureRole(serviceProvider, user.Id, Authorizarion.Constants.UserRoleArchetype, null, null);
                }

                CreateViews(context);
                context.SaveChanges();
            }
        }

        private static void CreateViews(ApplicationDbContext context)
        {
            /*
              DROP VIEW dbo.ViewSINners
              CREATE VIEW dbo.ViewSINners as
                    SELECT dbo.SINners.Id, dbo.SINners.MyGroupId, dbo.SINners.GoogleDriveFileId, dbo.SINners.Alias, dbo.SINners.JsonSummary, dbo.SINners.UploadClientId, dbo.SINners.LastChange, dbo.SINners.UploadDateTime, 
                          dbo.SINners.DownloadUrl, dbo.SINners.SINnerMetaDataId, dbo.SINnerMetaData.VisibilityId, dbo.UserRights.CanEdit, dbo.UserRights.EMail, dbo.UserRights.SINnerId, dbo.SINnerVisibility.IsPublic AS SINnerIsPublic, 
                          dbo.SINnerVisibility.IsGroupVisible, dbo.SINnerGroups.IsPublic AS GroupIsPublic, dbo.SINnerGroups.Groupname, dbo.SINnerGroups.MyAdminIdentityRole, dbo.SINnerGroups.MyParentGroupId
                        FROM            dbo.SINners LEFT OUTER JOIN
                          dbo.SINnerMetaData ON dbo.SINners.SINnerMetaDataId = dbo.SINnerMetaData.Id INNER JOIN
                          dbo.SINnerVisibility ON dbo.SINnerMetaData.VisibilityId = dbo.SINnerVisibility.Id INNER JOIN
                          dbo.SINnerGroups ON dbo.SINners.MyGroupId = dbo.SINnerGroups.Id LEFT OUTER JOIN
                          dbo.UserRights ON dbo.SINnerVisibility.Id = dbo.UserRights.SINnerVisibilityId
             */
        }

        /// <summary>
        /// TODO: This statement needs to be executed in a SEPERATE connection to the Azure-Master-DB
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userpwd"></param>
        /// <returns></returns>
        private static string GetSqlCommandMasterUser(string username, string userpwd)
        {
            string sqltext = @"IF NOT EXISTS (SELECT name FROM sys.sql_logins WHERE name='" + username + "') ";
            sqltext += " " + Environment.NewLine + "   BEGIN";
            sqltext += " " + Environment.NewLine + "       CREATE LOGIN " + username + " WITH PASSWORD = '" + userpwd + "';";
            sqltext += " " + Environment.NewLine + "       CREATE USER[" + username + "] FROM LOGIN[" + username + "];";
            sqltext += " " + Environment.NewLine + "       ALTER ROLE db_owner ADD MEMBER " + username + ";";
            sqltext += " " + Environment.NewLine + "    END";
            sqltext += " " + Environment.NewLine + "ELSE";
            sqltext += " " + Environment.NewLine + "    BEGIN";
            sqltext += " " + Environment.NewLine + "        ALTER LOGIN " + username + " WITH PASSWORD = '" + userpwd + "';";
            //sqltext += " " + Environment.NewLine + "        CREATE USER[" + username + "] FROM LOGIN[" + username + "];";
            sqltext += " " + Environment.NewLine + "    END; ";
            return sqltext;
        }


        private static async Task<Guid> EnsureUser(IServiceProvider serviceProvider,
                                                   ApplicationUser user, string userPW)
        {
            try
            {
                var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
                if (userManager == null)
                    return Guid.Empty;
                var u = await userManager.FindByNameAsync(user.UserName);
                if (u == null)
                {
                    user.PasswordHash = userManager.PasswordHasher.HashPassword(user, userPW);
                    await userManager.CreateAsync(user);
                }
                return u?.Id ?? Guid.Empty;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
            }
            return Guid.Empty;

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SeedData.EnsureRole(IServiceProvider, Guid, string, RoleManager<ApplicationRole>, UserManager<ApplicationUser>)'
        public static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SeedData.EnsureRole(IServiceProvider, Guid, string, RoleManager<ApplicationRole>, UserManager<ApplicationUser>)'
                                                                      Guid uid, string role, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            IdentityResult IR = null;

            try
            {
                if (roleManager == null)
                {
                    roleManager = serviceProvider.GetService<RoleManager<ApplicationRole>>();
                    if (roleManager == null)
                        return null;
                }
                if (!await roleManager.RoleExistsAsync(role))
                {
                    IR = await roleManager.CreateAsync(new ApplicationRole(role));
                }

                if (userManager == null)
                {
                    userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
                    if (userManager == null)
                        return null;
                }

                var user = await userManager.FindByIdAsync(uid.ToString());

                IR = await userManager.AddToRoleAsync(user, role);

            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
            }
            return IR;

        }
        #endregion


    }
}
