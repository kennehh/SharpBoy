using Microsoft.Extensions.DependencyInjection;
using SDL2;
using SharpBoy.App.ImGuiCore;
using SharpBoy.App.SdlCore;
using SharpBoy.Core;
using SharpBoy.Core.InputHandling;
using SharpBoy.Core.Utilities;
using System.Windows.Forms;

namespace SharpBoy.App
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Game Boy Rom (*.gb)|*.gb;*.zip";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
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
                    gameboy.LoadCartridge(openFileDialog.FileName);

                    Task.Run(gameboy.Run);

                    var window = serviceCollection.GetRequiredService<MainWindow>();
                    window.Initialise();
                    window.Run();
                }
            }
        }
    }
}