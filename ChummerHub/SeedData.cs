using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ChummerHub.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChummerHub.API;
using Microsoft.Extensions.Configuration;

namespace ChummerHub
{
    public class SeedData
    {

        #region snippet_Initialize
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                
                if (context.Users.Any())
                {
                    return;   // DB has been seeded
                }
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                //currently AzureLogins need to be created on the master-DB
                //Todo: implement this with a seperate connection to the masters-DB
                //var sqlMasterUser = GetSqlCommandMasterUser(config["SqlSinnerUserName"], config["SqlSinnerUserPW"]);
                //context.Database.ExecuteSqlCommandAsync(sqlMasterUser);
                foreach (var user in Config.GetAdminUsers())
                {
                    var userID = await EnsureUser(serviceProvider, user, testUserPw);
                    await EnsureRole(serviceProvider, user.Id, Authorizarion.Constants.AdministratorsRole);
                    await EnsureRole(serviceProvider, user.Id, Authorizarion.Constants.RegisteredUserRole);
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// TODO: This statement needs to be executed in a SEPERATE connection to the Azure-Master-DB
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userpwd"></param>
        /// <returns></returns>
        private static String GetSqlCommandMasterUser(string username, string userpwd)
        {
            string sqltext =                       @"IF NOT EXISTS (SELECT name FROM sys.sql_logins WHERE name='" + username + "') ";
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

                var u = await userManager.FindByNameAsync(user.UserName);
                if (u == null)
                {
                    user.PasswordHash = userManager.PasswordHasher.HashPassword(user, userPW);
                    await userManager.CreateAsync(user);
                }
                return u.Id;
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
            }
            return Guid.Empty;
            
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                                      Guid uid, string role)
        {
            IdentityResult IR = null;

            try
            {

            
                var roleManager = serviceProvider.GetService<RoleManager<ApplicationRole>>();

                if (!await roleManager.RoleExistsAsync(role))
                {
                    IR = await roleManager.CreateAsync(new ApplicationRole(role));
                }

                var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

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
