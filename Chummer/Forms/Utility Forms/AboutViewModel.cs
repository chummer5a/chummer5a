using System.Collections.Generic;
using System.Threading.Tasks;
using ChummerRazorLibrary.Bases;
using ChummerRazorLibrary.Pages;

namespace Chummer;

public class AboutViewModel : ViewModelBase, IAboutViewModel
{

    public string ProductName { get; } = About.AssemblyProduct;

    public string Version { get; private set; }
    public string Copyright { get; private set; }
    public string CompanyName { get; private set; }
    public string Description { get; private set; }
    public string Disclaimer { get; private set; }
    public string ImagePath => "images/troll.png";

    public IReadOnlyCollection<string> Contributors { get; } = Properties.Contributors.Usernames;
    public bool Initialized { get; set; }


    public override async Task OnInitializedAsync()
    {
        var strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);

        var productName = await LanguageManager.GetStringAsync("String_Version", false).ConfigureAwait(false);
        Version = string.IsNullOrEmpty(productName) ? "Version" : productName + strSpace + About.AssemblyVersion;

        var copyright = await LanguageManager.GetStringAsync("About_Copyright_Text", false).ConfigureAwait(false);
        Copyright = string.IsNullOrEmpty(copyright) ? About.AssemblyCopyright : copyright;

        var company = await LanguageManager.GetStringAsync("About_Company_Text", false).ConfigureAwait(false);
        CompanyName = string.IsNullOrEmpty(company) ? About.AssemblyCompany : company;

        var description = await LanguageManager.GetStringAsync("About_Description_Text", false).ConfigureAwait(false);
        Description = string.IsNullOrEmpty(description) ? About.AssemblyDescription : description;

        Disclaimer = await LanguageManager.GetStringAsync("About_Label_Disclaimer_Text").ConfigureAwait(false);

        Initialized = true;
    }
}
