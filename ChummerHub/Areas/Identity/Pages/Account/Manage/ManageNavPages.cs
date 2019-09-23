using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace ChummerHub.Areas.Identity.Pages.Account.Manage
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages'
    public static class ManageNavPages
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.Index'
        public static string Index => "Index";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.Index'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.ChangePassword'
        public static string ChangePassword => "ChangePassword";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.ChangePassword'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.DownloadPersonalData'
        public static string DownloadPersonalData => "DownloadPersonalData";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.DownloadPersonalData'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.DeletePersonalData'
        public static string DeletePersonalData => "DeletePersonalData";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.DeletePersonalData'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.ExternalLogins'
        public static string ExternalLogins => "ExternalLogins";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.ExternalLogins'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.PersonalData'
        public static string PersonalData => "PersonalData";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.PersonalData'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.TwoFactorAuthentication'
        public static string TwoFactorAuthentication => "TwoFactorAuthentication";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.TwoFactorAuthentication'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.IndexNavClass(ViewContext)'
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.IndexNavClass(ViewContext)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.ChangePasswordNavClass(ViewContext)'
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.ChangePasswordNavClass(ViewContext)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.DownloadPersonalDataNavClass(ViewContext)'
        public static string DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.DownloadPersonalDataNavClass(ViewContext)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.DeletePersonalDataNavClass(ViewContext)'
        public static string DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.DeletePersonalDataNavClass(ViewContext)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.ExternalLoginsNavClass(ViewContext)'
        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.ExternalLoginsNavClass(ViewContext)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.PersonalDataNavClass(ViewContext)'
        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.PersonalDataNavClass(ViewContext)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.TwoFactorAuthenticationNavClass(ViewContext)'
        public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.TwoFactorAuthenticationNavClass(ViewContext)'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.PageNavClass(ViewContext, string)'
        public static string PageNavClass(ViewContext viewContext, string page)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ManageNavPages.PageNavClass(ViewContext, string)'
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
