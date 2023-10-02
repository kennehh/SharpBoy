using Microsoft.Extensions.DependencyInjection;
using SharpBoy.Core.Cartridges;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.InputHandling;
using SharpBoy.Core.Interrupts;
using SharpBoy.Core.Memory;
using SharpBoy.Core.Processor;
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
                .AddSingleton<ICartridgeFactory, CartridgeFactory>()
                .AddSingleton<IFrameBufferManager, DoubleBufferManager>()
                .AddSingleton<IInterruptManager, InterruptManager>()
                .AddSingleton<IInputController, InputController>()
                .AddSingleton<IPpuMemory, PpuMemory>()
                .AddSingleton<IRenderer, Renderer>()
                .AddSingleton<ITimer, Timer>();
        }
    }
}
