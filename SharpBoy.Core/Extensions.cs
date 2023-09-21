using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.Memory;
using SharpBoy.Core.Processor;
using SharpBoy.Core.Rendering;

namespace SharpBoy.Core
{
    public static class Extensions
    {
        public static IServiceCollection RegisterCoreServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<GameBoy>()
                .AddSingleton<GameBoyState>()
                .AddSingleton<ICpu, Cpu>()
                .AddSingleton<IMmu, Mmu>()
                .AddSingleton<IPpu, Ppu>()
                .AddSingleton<ICartridgeReader, CartridgeReader>()
                .AddSingleton<IRenderQueue, RenderQueue>()
                .AddSingleton<IInterruptManager, InterruptManager>()
                .AddSingleton<ITimer, Timer>();
        }
    }
}
