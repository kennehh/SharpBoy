using Avalonia.Controls;

namespace SharpBoy.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var debugWindow = new DebugWindow();
        debugWindow.Show();

        Closing += (source, args) =>
        {
            debugWindow.Close();
        };
    }
}
