using Microsoft.Extensions.DependencyInjection;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.InputHandling;
using SharpBoy.Core.Processor;
using SharpBoy.Core.Tests.Mocks;
using SharpBoy.Core.Utilities;
using System.Drawing;

namespace SharpBoy.Core.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class IntegrationTests
    {
        [Test]
        public void DmgAcid2Test() => TestRom("TestRoms/dmg-acid2/dmg-acid2.gb", "TestRoms/dmg-acid2/dmg-acid2-dmg.png", 0x40);

        private void TestRom(string pathToRom, string pathToScreenshot, byte opcodeToStopAt)
        {
            var serviceProvider = GetServiceProvider();
            var gb = serviceProvider.GetService<GameBoy>();

            gb.LoadCartridge(pathToRom);
            var cpu = (Cpu)gb.Cpu;

            gb.Cpu.Registers.PC = 0x100;
            gb.InitialiseGameBoyState();

            while (cpu.Opcode != opcodeToStopAt)
            {
                gb.Step();
            }

            var renderQueue = serviceProvider.GetService<IRenderQueue>();
            renderQueue.TryDequeue(out var fb);
            CompareToScreenshot(fb.ToArray(), pathToScreenshot);
        }

        private void CompareToScreenshot(byte[] framebuffer, string pathToScreenshot)
        {
            using (var expectedImage = Image.Load<Rgba32>(pathToScreenshot))
            {
                for (int y = 0; y < expectedImage.Height; y++)
                {
                    for (int x = 0; x < expectedImage.Width; x++)
                    {
                        var expectedPixel = expectedImage[x, y];
                        var offset = (y * 160 + x) * 4;
                        var framebufferPixel = new Rgba32(framebuffer[offset], framebuffer[offset + 1], framebuffer[offset + 2], framebuffer[offset + 3]);
                        Assert.That(framebufferPixel, Is.EqualTo(expectedPixel), $"Incorrect at {x},{y}");
                    }
                }
            }
        }

        private static ServiceProvider GetServiceProvider()
        {
            return new ServiceCollection()
                .RegisterCoreServices()
                .AddSingleton<IInputHandler, InputHandlerMock>()
                .AddSingleton<IRenderQueue, RenderQueueMock>()
                .BuildServiceProvider();
        }
    }
}
