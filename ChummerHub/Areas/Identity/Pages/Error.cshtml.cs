using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace ChummerHub.Areas.Identity.Pages
{
    [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel'
    public class ErrorModel : PageModel
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.RequestId'
        public string RequestId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.RequestId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.ShowRequestId'
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.ShowRequestId'

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.OnGet()'
        public void OnGet()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorModel.OnGet()'
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}