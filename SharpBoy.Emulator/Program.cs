using SharpBoy.Core.Processor;
using SharpBoy.Core;
using System.Diagnostics;
using SharpBoy.Core.Rendering;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SharpBoy.Core.Memory;
using SharpBoy.Core.Graphics;
using SharpBoy.Rendering.Silk;

internal class Program
{
    private static async Task Main(string[] args)
    {
        const string romPath = "C:\\Projects\\Tetris (World) (Rev A).gb";
        const string bootPath = "Z:\\games\\bios\\gb\\gb_bios.bin";

        var serviceProvider = new ServiceCollection()
            .RegisterCoreServices()
            .AddSingleton<IRenderer, SilkRenderer>()
            .BuildServiceProvider();

        var gb = serviceProvider.GetService<GameBoy>();
        gb.LoadBootRom(bootPath);
        gb.LoadCartridge(romPath);
        gb.Run();
    }
}