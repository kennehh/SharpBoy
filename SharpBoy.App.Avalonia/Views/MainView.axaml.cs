using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SharpBoy.App.Avalonia.Controls;
using SharpBoy.Core;
using SharpBoy.Core.Rendering;
using System.Threading.Tasks;

namespace SharpBoy.App.Avalonia.Views;

public partial class MainView : UserControl
{
    private readonly GameBoy gameboy;

    public MainView()
    {
        InitializeComponent();

        var rendererControl = this.FindControl<EmulatorRenderer>("EmulatorRenderer");
        rendererControl.Renderer = App.ServiceProvider.GetRequiredService<IRenderer>();

        gameboy = App.ServiceProvider.GetRequiredService<GameBoy>();

        //const string romPath = "C:\\Projects\\SharpBoy\\SharpBoy.Core.Tests\\TestRoms\\blargg\\cpu_instrs\\cpu_instrs.gb";
        const string romPath = "Z:\\Games\\Roms\\gb\\Dr. Mario (World) (Rev A).zip";
        //const string romPath = "C:\\Projects\\Dr. Mario (World) (Rev A).gb";
        //const string bootPath = "Z:\\games\\bios\\gb\\gb_bios.bin";
        //gameboy.LoadBootRom(bootPath);
        gameboy.LoadCartridge(romPath);

        Task.Run(gameboy.Run);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        gameboy.Stop();
    }
}
