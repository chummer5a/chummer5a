using System.ComponentModel;

namespace ChummerRazorLibrary.Bases;

public interface IViewModelBase : INotifyPropertyChanged
{
    Task OnInitializedAsync();
    Task Loaded();
    Task OnParametersSetAsync();

    void OnParametersSet();
}
