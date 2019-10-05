using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DownloadPersonalDataModel'
    public class DownloadPersonalDataModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DownloadPersonalDataModel'
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DownloadPersonalDataModel.DownloadPersonalDataModel(UserManager<ApplicationUser>, ILogger<DownloadPersonalDataModel>)'
        public DownloadPersonalDataModel(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DownloadPersonalDataModel.DownloadPersonalDataModel(UserManager<ApplicationUser>, ILogger<DownloadPersonalDataModel>)'
            UserManager<ApplicationUser> userManager,
            ILogger<DownloadPersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DownloadPersonalDataModel.OnPostAsync()'
        public async Task<IActionResult> OnPostAsync()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DownloadPersonalDataModel.OnPostAsync()'
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            // Only include personal data for download
            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(personalData)), "text/json");
        }
    }
}
