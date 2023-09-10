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

namespace SharpBoy.Core.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class IntegrationTests
    {
        private static IEnumerable<string> CpuInstrRoms => Directory.GetFiles("TestRoms/blargg/cpu_instrs/individual");
        private static IEnumerable<string> MemTimingRoms => Directory.GetFiles("TestRoms/blargg/mem_timing/individual")
            .Concat(Directory.GetFiles("TestRoms/blargg/mem_timing-2/rom_singles"));

        [Test, TestCaseSource(nameof(CpuInstrRoms))]
        public void BlargCpuInstrTest(string path) => TestRom(path);

        [Test]
        public void BlargInstrTimingTest() => TestRom("TestRoms/blargg/instr_timing/instr_timing.gb");

        [Test, TestCaseSource(nameof(MemTimingRoms))]
        public void BlargMemTimingTest(string path) => TestRom(path);

        private void TestRom(string path)
        {
            var gb = new GameBoy();
            gb.LoadCartridge(path);

            var lastPC = 0;
            gb.Cpu.Registers.PC = 0x101;
            var characters = new List<byte>();

            var isHalted = false;
            while (!(lastPC == gb.Cpu.Registers.PC && !isHalted))
            {
                isHalted = gb.Cpu.Halted;
                lastPC = gb.Cpu.Registers.PC;

                gb.Cpu.Step();

                if (gb.Cpu.Mmu.Read8Bit(0xff02) == 0x81)
                {
                    characters.Add(gb.Cpu.Mmu.Read8Bit(0xff01));
                    gb.Cpu.Mmu.Write8Bit(0xff02, 0x01);
                }
            }
            
            if (!characters.Any())
            {
                // test message should be stored at 0xa004
                Assert.That(gb.Mmu.Read8Bit(0xa001), Is.EqualTo(0xde));
                Assert.That(gb.Mmu.Read8Bit(0xa002), Is.EqualTo(0xb0));
                Assert.That(gb.Mmu.Read8Bit(0xa003), Is.EqualTo(0x61));

                ushort address = 0xa004;
                while (true)
                {
                    var character = gb.Mmu.Read8Bit(address++);
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
