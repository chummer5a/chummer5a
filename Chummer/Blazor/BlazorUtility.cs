using System;
using System.Collections.Generic;
using System.IO;
using Chummer.Annotations;
using ChummerRazorLibrary;
using ChummerRazorLibrary.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Chummer.Blazor;

public static class BlazorUtility
{
    [CanBeNull] private static IServiceProvider _defaultServiceProvider;

    /// <summary>
    /// A <see cref="IServiceProvider"/> that contains a default list of services needed to spawn a <see cref="BlazorWebView"/>.
    /// It is cached and thus will only be build once and then reused among all other following components.
    /// </summary>
    public static IServiceProvider DefaultServiceProvider
    {
        get
        {
            if (_defaultServiceProvider is not null)
            {
                return _defaultServiceProvider;
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(config);
            });

            services.AddChummerServices();
            services.AddChummerRazorLibraryServices();
            services.AddWindowsFormsBlazorWebView();
#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif
            _defaultServiceProvider = services.BuildServiceProvider();
            return _defaultServiceProvider;
        }
    }

    /// <summary>
    /// Define all services that may be needed for Blazor components here.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    private static IServiceCollection AddChummerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IAboutViewModel, AboutViewModel>();

        return serviceCollection;
    }


    /// <summary>
    /// Convenience Method to configure a <see cref="BlazorWebView"/> with all the needed services.
    /// </summary>
    /// <param name="blazorWebView" />
    /// <param name="parameters" />
    public static BlazorWebView ConfigureBlazorWebView<TComponent>(this BlazorWebView blazorWebView, [CanBeNull] IDictionary<string, object> parameters = null)
        where TComponent : IComponent
    {
        parameters ??= new Dictionary<string, object>();
        blazorWebView.HostPage = "wwwroot\\index.html";
        blazorWebView.Services = DefaultServiceProvider;

        var wrapperParams = new Dictionary<string, object>()
        {
            {nameof(SharedWrapper.IsLightMode), ColorManager.IsLightMode}
        };

        blazorWebView.RootComponents.Add<SharedWrapper>("#app-wrapper", wrapperParams);
        blazorWebView.RootComponents.Add<TComponent>("#app", parameters);

        return blazorWebView;
    }
}
