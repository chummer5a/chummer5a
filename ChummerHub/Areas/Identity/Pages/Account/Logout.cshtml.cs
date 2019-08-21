using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel'
    public class LogoutModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel'
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.LogoutModel(SignInManager<ApplicationUser>, ILogger<LogoutModel>)'
        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.LogoutModel(SignInManager<ApplicationUser>, ILogger<LogoutModel>)'
        {
            _signInManager = signInManager;
            _logger = logger;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.OnGet()'
        public void OnGet()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.OnGet()'
        {
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.OnPost(string)'
        public async Task<IActionResult> OnPost(string returnUrl = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LogoutModel.OnPost(string)'
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return Page();
            }
        }
    }
}