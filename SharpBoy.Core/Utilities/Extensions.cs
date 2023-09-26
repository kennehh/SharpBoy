using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpBoy.Core.CartridgeHandling;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.Interrupts;
using SharpBoy.Core.Memory;
using SharpBoy.Core.Processor;
using SharpBoy.Core.Rendering;
using SharpBoy.Core.Timing;
using Timer = SharpBoy.Core.Timing.Timer;

namespace SharpBoy.Core.Utilities
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
                .AddSingleton<ICartridge, Cartridge>()
                .AddSingleton<IRenderQueue, RenderQueue>()
                .AddSingleton<IInterruptManager, InterruptManager>()
                .AddSingleton<ITimer, Timer>();
        }
    }
}
