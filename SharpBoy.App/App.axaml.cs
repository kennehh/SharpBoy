using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SharpBoy.App.ViewModels;
using SharpBoy.App.Views;
using SharpBoy.Core.Rendering;
using SharpBoy.Core.Rendering.Silk;
using SharpBoy.Core.Utilities;
using System;

namespace SharpBoy.App;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceProvider = new ServiceCollection()
            .RegisterCoreServices()
            .AddSingleton<IRenderer, SilkRenderer>()
            .AddSingleton<DebugViewModel>()
            .BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
