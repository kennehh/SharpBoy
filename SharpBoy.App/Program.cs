using Microsoft.Extensions.DependencyInjection;
using SharpBoy.App.SdlCore;
using SharpBoy.Core.InputHandling;
using SharpBoy.Core.Utilities;

namespace SharpBoy.App
{
    internal class Program
    {
        [STAThread]
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


            var window = serviceCollection.GetRequiredService<MainWindow>();
            window.Initialise();
            window.Run();
        }
    }
}