using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SharpBoy.App.ViewModels;

namespace SharpBoy.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var debugWindow = new DebugWindow
        {
            DataContext = App.ServiceProvider.GetRequiredService<DebugViewModel>()
        };
        debugWindow.Show();

        Closing += (source, args) =>
        {
            debugWindow.Close();
        };
    }
}
