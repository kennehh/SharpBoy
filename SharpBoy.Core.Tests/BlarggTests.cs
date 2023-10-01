using SharpBoy.Core.Processor;
using SharpBoy.Core.Memory;
using SharpBoy.Core.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SharpBoy.Core.Utilities;
using SharpBoy.Core.InputHandling;

namespace SharpBoy.Core.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class BlarggTests
    {
        private static IEnumerable<string> CpuInstrRoms => Directory.GetFiles("TestRoms/blargg/cpu_instrs/individual");
        private static IEnumerable<string> MemTimingRoms => Directory.GetFiles("TestRoms/blargg/mem_timing/individual")
            .Concat(Directory.GetFiles("TestRoms/blargg/mem_timing-2/rom_singles"));

        [Test, TestCaseSource(nameof(CpuInstrRoms))]
        public void CpuInstrTest(string path) => TestBlarggRom(path);

        [Test]
        public void InstrTimingTest() => TestBlarggRom("TestRoms/blargg/instr_timing/instr_timing.gb");

        [Test, TestCaseSource(nameof(MemTimingRoms))]
        public void MemTimingTest(string path) => TestBlarggRom(path);


        [Test]
        public void HaltBugTest() => TestBlarggRom("TestRoms/blargg/halt_bug.gb");

        private void TestBlarggRom(string path)
        {
            var gb = CreateGameBoy();
            gb.LoadCartridge(path);
            gb.InitialiseGameBoyState();
            var cpu = (Cpu)gb.Cpu;

            var lastPC = -1;
            var characters = new List<byte>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var isHalted = false;
            while (!(lastPC == cpu.Registers.PC && !isHalted))
            {
                isHalted = cpu.State == CpuState.Halted;
                lastPC = cpu.Registers.PC;

                gb.Step();

                if (gb.Mmu.Read(0xff02) == 0x81)
                {
                    characters.Add(gb.Mmu.Read(0xff01));
                    gb.Mmu.Write(0xff02, 0x01);
                }

                Assert.That(stopwatch.Elapsed.TotalSeconds, Is.LessThan(10), "Test took too long");
            }

            stopwatch.Reset();

            if (!characters.Any())
            {
                // test message should be stored at 0xa004
                Assert.That(gb.Mmu.Read(0xa001), Is.EqualTo(0xde));
                Assert.That(gb.Mmu.Read(0xa002), Is.EqualTo(0xb0));
                Assert.That(gb.Mmu.Read(0xa003), Is.EqualTo(0x61));

                ushort address = 0xa004;
                while (true)
                {
                    var character = gb.Mmu.Read(address++);
                    if (character != 0)
                    {
                        characters.Add(character);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var message = Encoding.Default.GetString(characters.ToArray());
            var passed = message.Contains("Passed") && !message.Contains("Failed");
            Assert.That(passed, "Message: " + message);
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
