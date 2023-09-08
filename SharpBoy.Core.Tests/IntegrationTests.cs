using SharpBoy.Core.Cpu;
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
        private static IEnumerable<string> GetCpuInstrRoms()
        {
            // (hl) breaks the test explorer
            return Directory.GetFiles("TestRoms/blargg/cpu_instrs/individual");
        }

        [Test, TestCaseSource(nameof(GetCpuInstrRoms))]
        public void BlargCpuInstrTests(string path)
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

                gb.Cpu.Tick();

                if (gb.Cpu.Mmu.Read8Bit(0xff02) == 0x81)
                {
                    characters.Add(gb.Cpu.Mmu.Read8Bit(0xff01));
                    gb.Cpu.Mmu.Write8Bit(0xff02, 0x01);
                }
            }

            var message = Encoding.Default.GetString(characters.ToArray());
            var passed = message.Contains("Passed") && !message.Contains("Failed");
            Assert.That(passed, "Message: " + message);
        }

        //[Test]
        //public void BlargCpuInstrTests()
        //{
        //}
    }
}
