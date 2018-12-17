using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ChummerHub.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using ChummerHub.API;

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
                //context.Users.AddRange(Config.GetAdminUsers());
                foreach (var user in Config.GetAdminUsers())
                {
                    var userID = await EnsureUser(serviceProvider, user, testUserPw);
                    await EnsureRole(serviceProvider, user.Id, Authorizarion.Constants.AdministratorsRole);
                    await EnsureRole(serviceProvider, user.Id, Authorizarion.Constants.RegisteredUserRole);
                }

                
                // For sample purposes we are seeding 2 users both with the same password.
                // The password is set with the following command:
                // dotnet user-secrets set SeedUserPW <pw>
                // The admin user can do anything


                //var adminID = await EnsureUser(serviceProvider, "archon.megalon", "archon.megalon@gmail.com");
                //await EnsureRole(serviceProvider, adminID, Authorizarion.Constants.AdministratorsRole);

                // allowed user can create and edit contacts that they create
                //var uid = await EnsureUser(serviceProvider, testUserPw, "manager@contoso.com");
                //await EnsureRole(serviceProvider, uid, Constants.ContactManagersRole);

                context.SaveChanges();
            }
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
