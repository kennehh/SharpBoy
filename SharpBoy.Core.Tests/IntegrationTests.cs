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
    public class IntegrationTests
    {
        private static IEnumerable<string> GetCpuInstrRoms()
        {
            return Directory.GetFiles("TestRoms/blargg/cpu_instrs/individual").Select(x => Path.GetFileName(x).Replace("(hl)", "[hl]"));
        }

        [Test, TestCaseSource(nameof(GetCpuInstrRoms)), Parallelizable(ParallelScope.All)]
        public void BlargCpuInstrTests(string filename)
        {
            var gb = new GameBoy();
            gb.LoadCartridge($"TestRoms/blargg/cpu_instrs/individual/{filename.Replace("[hl]", "(hl)")}");

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
    }
}
