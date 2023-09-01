using System.Diagnostics.CodeAnalysis;
using ChummerDBRazorLibrary.Backend.Bases;

namespace ChummerRazorLibrary.Pages;

public interface IAboutViewModel : IViewModelBase
{
    public string? ProductName { get; }
    public string? Version { get; }
    public string? Copyright { get; }
    public string? CompanyName { get; }
    public string? Description { get; }
    public string? Disclaimer { get; }
    public string? ImagePath { get; }

    public IReadOnlyCollection<string>? Contributors { get; }

    [MemberNotNullWhen(returnValue: true, new []{nameof(ProductName), nameof(Version), nameof(Copyright), nameof(CompanyName), nameof(Description), nameof(Disclaimer), nameof(ImagePath), nameof(Contributors)})]
    public bool Initialized { get; set; }
}
