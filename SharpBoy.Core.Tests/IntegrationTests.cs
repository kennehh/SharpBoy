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

namespace SharpBoy.Core.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class IntegrationTests
    {
        private static IEnumerable<string> CpuInstrRoms => Directory.GetFiles("TestRoms/blargg/cpu_instrs/individual");
        private static IEnumerable<string> MemTimingRoms => Directory.GetFiles("TestRoms/blargg/mem_timing/individual")
            .Concat(Directory.GetFiles("TestRoms/blargg/mem_timing-2/rom_singles"));

        [Test, TestCaseSource(nameof(CpuInstrRoms))]
        public void BlarggCpuInstrTest(string path) => TestBlarggRom(path);

        [Test]
        public void BlarggInstrTimingTest() => TestBlarggRom("TestRoms/blargg/instr_timing/instr_timing.gb");

        [Test, TestCaseSource(nameof(MemTimingRoms))]
        public void BlarggMemTimingTest(string path) => TestBlarggRom(path);

        [Test]
        public void BlarggHaltBugTest() => TestBlarggRom("TestRoms/blargg/halt_bug.gb");

        private void TestBlarggRom(string path)
        {
            var gb = new GameBoy();
            gb.LoadCartridge(path);

            var lastPC = -1;
            //gb.Cpu.Registers.PC = 0x101;
            gb.LoadBootRom("Z:\\games\\bios\\gb\\dmg0_rom.bin");
            var characters = new List<byte>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var isHalted = false;
            while (!(lastPC == gb.Cpu.Registers.PC && !isHalted))
            {
                isHalted = gb.Cpu.State == CpuState.Halted;
                lastPC = gb.Cpu.Registers.PC;

                gb.Step();

                if (gb.Cpu.Mmu.ReadValue(0xff02) == 0x81)
                {
                    characters.Add(gb.Cpu.Mmu.ReadValue(0xff01));
                    gb.Cpu.Mmu.WriteValue(0xff02, 0x01);
                }

                //Assert.That(stopwatch.Elapsed.TotalSeconds, Is.LessThan(30), "Test took too long");
            }

            stopwatch.Reset();

            if (!characters.Any())
            {
                // test message should be stored at 0xa004
                Assert.That(gb.Mmu.ReadValue(0xa001), Is.EqualTo(0xde));
                Assert.That(gb.Mmu.ReadValue(0xa002), Is.EqualTo(0xb0));
                Assert.That(gb.Mmu.ReadValue(0xa003), Is.EqualTo(0x61));

                ushort address = 0xa004;
                while (true)
                {
                    var character = gb.Mmu.ReadValue(address++);
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
    }
}
