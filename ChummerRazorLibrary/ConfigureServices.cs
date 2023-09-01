using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace ChummerRazorLibrary;

public static class ConfigureServices
{
    public static IServiceCollection AddChummerRazorLibraryServices(this IServiceCollection services)
    {
        services.AddMudServices();
        return services;
    }
}
