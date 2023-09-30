using Microsoft.Extensions.DependencyInjection;
using SDL2;
using SharpBoy.App.ImGuiCore;
using SharpBoy.App.SdlCore;
using SharpBoy.Core;
using SharpBoy.Core.InputHandling;
using SharpBoy.Core.Utilities;

namespace SharpBoy.App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .RegisterCoreServices()
                .AddSingleton<SdlManager>()
                //.AddSingleton<ImGuiManager>()
                //.AddSingleton<ImGuiRenderer>()
                .AddSingleton<MainWindow>()
                .AddSingleton<GameBoyFramebuffer>()
                .AddSingleton<IInputHandler, SdlInputHandler>()
                //.AddSingleton<DebugViewModel>()
                .BuildServiceProvider();


            var gameboy = serviceCollection.GetRequiredService<GameBoy>();

            const string romPath = "Z:\\Games\\Roms\\gb\\Legend of Zelda, The - Link's Awakening (USA, Europe) (Rev B).zip";
            //const string romPath = "C:\\Projects\\Dr. Mario (World) (Rev A).gb";
            //const string romPath = "D:\\Emulators\\bgb\\bgbtest.gb";
            //const string bootPath = "Z:\\games\\bios\\gb\\gb_bios.bin";
            //gameboy.LoadBootRom(bootPath);
            gameboy.LoadCartridge(romPath);

            Task.Run(gameboy.Run);

            var window = serviceCollection.GetRequiredService<MainWindow>();
            window.Initialise();
            window.Run();            
        }
    }
}