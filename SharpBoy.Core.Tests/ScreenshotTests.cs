using Microsoft.Extensions.DependencyInjection;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.InputHandling;
using SharpBoy.Core.Processor;
using SharpBoy.Core.Tests.Mocks;
using SharpBoy.Core.Utilities;

namespace SharpBoy.Core.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class ScreenshotTests
    {
        private static IEnumerable<(string, string)> MealybugTearoomPpuTestRoms => GetMealybugTearoomTests();

        [Test, TestCaseSource(nameof(MealybugTearoomPpuTestRoms))]
        public void MealybugTearoomPpuTest((string rom, string screenshot) test) => TestRom(test.rom, test.screenshot, 0x40);

        [Test]
        public void DmgAcid2Test() => TestRom("TestRoms/dmg-acid2/dmg-acid2.gb", "TestRoms/dmg-acid2/dmg-acid2-dmg.png", 0x40);

        private static void TestRom(string pathToRom, string pathToScreenshot, byte opcodeToStopAt)
        {
            var serviceProvider = GetServiceProvider();
            var gb = serviceProvider.GetService<GameBoy>();

            gb.LoadCartridge(pathToRom);
            var cpu = (Cpu)gb.Cpu;
            gb.InitialiseGameBoyState();

            while (cpu.Opcode != opcodeToStopAt)
            {
                gb.Step();
            }

            var renderQueue = serviceProvider.GetService<IFrameBufferManager>();
            renderQueue.TryGetNextFrame(out var fb);
            CompareToScreenshot(fb.ToArray(), pathToScreenshot);
        }

        private static void CompareToScreenshot(byte[] framebuffer, string pathToScreenshot)
        {
            using (var expectedImage = Image.Load<Rgba32>(pathToScreenshot))
            {
                for (int y = 0; y < expectedImage.Height; y++)
                {
                    for (int x = 0; x < expectedImage.Width; x++)
                    {
                        var expectedPixel = expectedImage[x, y];
                        var offset = (y * expectedImage.Width + x) * 4;
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
                .AddSingleton<IFrameBufferManager, RenderQueueMock>()
                .BuildServiceProvider();
        }

        private static IEnumerable<(string, string)> GetMealybugTearoomTests()
        {
            var files = Directory.GetFiles("TestRoms/mealybug-tearoom-tests/ppu");
            var roms = files.Where(x => Path.GetExtension(x) == ".gb");

            var tests = roms.Select(rom =>
            {
                var screenshot = files.FirstOrDefault(x =>
                    x.Contains(Path.GetFileNameWithoutExtension(rom)) &&
                    Path.GetExtension(x) == ".png" &&
                    x.Contains("dmg_blob"));

                return (rom, screenshot);
            });

            return tests.Where(x => !string.IsNullOrEmpty(x.rom) && !string.IsNullOrEmpty(x.screenshot));
        }
    }
}
