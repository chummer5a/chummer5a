using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChummerRazorLibrary.Bases;

public abstract partial class ViewModelBase: ObservableObject, IViewModelBase
{
    public virtual async Task OnInitializedAsync()
    {
        await Loaded().ConfigureAwait(false);
    }

    [RelayCommand]
    public virtual async Task Loaded()
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public virtual async Task OnParametersSetAsync()
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    protected virtual void NotifyStateChanged() => OnPropertyChanged((string?) null);

    public virtual void OnParametersSet()
    {

    }
}
