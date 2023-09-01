using System.ComponentModel;

namespace ChummerDBRazorLibrary.Backend.Bases;

public interface IViewModelBase : INotifyPropertyChanged
{
    Task OnInitializedAsync();
    Task Loaded();
    Task OnParametersSetAsync();

    void OnParametersSet();
}