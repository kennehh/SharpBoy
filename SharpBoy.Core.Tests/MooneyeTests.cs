using Microsoft.Extensions.DependencyInjection;
using SharpBoy.Core.InputHandling;
using SharpBoy.Core.Processor;
using SharpBoy.Core.Tests.Mocks;
using SharpBoy.Core.Utilities;
using System.Diagnostics;

namespace SharpBoy.Core.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class MooneyeTests
    {
        private static IEnumerable<string> AcceptanceRoms => Directory.GetFiles("TestRoms/mooneye-test-suite/acceptance");
        private static IEnumerable<string> AcceptanceBitsRoms => Directory.GetFiles("TestRoms/mooneye-test-suite/acceptance/bits");
        private static IEnumerable<string> AcceptanceOamDmaRoms => Directory.GetFiles("TestRoms/mooneye-test-suite/acceptance/oam_dma");
        private static IEnumerable<string> AcceptancePpuRoms => Directory.GetFiles("TestRoms/mooneye-test-suite/acceptance/ppu");
        private static IEnumerable<string> AcceptanceTimerRoms => Directory.GetFiles("TestRoms/mooneye-test-suite/acceptance/timer");
        private static IEnumerable<string> Mbc1Roms => Directory.GetFiles("TestRoms/mooneye-test-suite/emulator-only/mbc1");
        private static IEnumerable<string> Mbc2Roms => Directory.GetFiles("TestRoms/mooneye-test-suite/emulator-only/mbc2");
        private static IEnumerable<string> Mbc5Roms => Directory.GetFiles("TestRoms/mooneye-test-suite/emulator-only/mbc5");

        [Test, TestCaseSource(nameof(AcceptanceRoms))]
        public void AcceptanceTest(string path) => TestMooneyeRom(path);
        [Test, TestCaseSource(nameof(AcceptanceBitsRoms))]
        public void AcceptanceBitsTest(string path) => TestMooneyeRom(path);
        [Test]
        public void AcceptanceInstrDaaTest() => TestMooneyeRom("TestRoms/mooneye-test-suite/acceptance/instr/daa.gb");
        [Test]
        public void AcceptanceInterruptsTest() => TestMooneyeRom("TestRoms/mooneye-test-suite/acceptance/interrupts/ie_push.gb");
        [Test, TestCaseSource(nameof(AcceptanceOamDmaRoms))]
        public void AcceptanceOamDmaTest(string path) => TestMooneyeRom(path);
        [Test, TestCaseSource(nameof(AcceptancePpuRoms))]
        public void AcceptancePpuTest(string path) => TestMooneyeRom(path);
        [Test]
        public void AcceptanceSerialTest() => TestMooneyeRom("TestRoms/mooneye-test-suite/acceptance/serial/boot_sclk_align-dmgABCmgb.gb");
        [Test, TestCaseSource(nameof(AcceptanceTimerRoms))]
        public void AcceptanceTimerTest(string path) => TestMooneyeRom(path);

        [Test, TestCaseSource(nameof(Mbc1Roms))]
        public void Mbc1Test(string path) => TestMooneyeRom(path);

        [Test, TestCaseSource(nameof(Mbc2Roms))]
        public void Mbc2Test(string path) => TestMooneyeRom(path);

        [Test, TestCaseSource(nameof(Mbc5Roms))]
        public void Mbc5Test(string path) => TestMooneyeRom(path);

        private void TestMooneyeRom(string path)
        {
            var gb = CreateGameBoy();
            gb.LoadCartridge(path);
            gb.InitialiseGameBoyState();
            var cpu = (Cpu)gb.Cpu;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (cpu.Opcode != 0x40)
            {
                gb.Step();
                //Assert.That(stopwatch.Elapsed.TotalSeconds, Is.LessThan(10), "Test took too long");
            }

            stopwatch.Reset();

            Assert.That(cpu.Registers.B, Is.EqualTo(3));
            Assert.That(cpu.Registers.C, Is.EqualTo(5));
            Assert.That(cpu.Registers.D, Is.EqualTo(8));
            Assert.That(cpu.Registers.E, Is.EqualTo(13));
            Assert.That(cpu.Registers.H, Is.EqualTo(21));
            Assert.That(cpu.Registers.L, Is.EqualTo(34));
        }

        private static GameBoy CreateGameBoy()
        {
            var serviceProvider = new ServiceCollection()
                .RegisterCoreServices()
                .AddSingleton<IInputHandler, InputHandlerMock>()
                .BuildServiceProvider();

            return serviceProvider.GetService<GameBoy>();
        }
    }
}
