using SharpBoy.Core.Cpu;
using SharpBoy.Core.Tests.Mocks;
using System;
using System.Collections.Generic;
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
            return Directory.GetFiles("TestRoms/blargg/cpu_instrs/individual").Select(x => Path.GetFileName(x));
        }

        [Test, TestCaseSource(nameof(GetCpuInstrRoms))]
        public void BlargCpuInstrTests(string filename)
        {
            var rom = File.ReadAllBytes($"TestRoms/blargg/cpu_instrs/individual/{filename}");
            var memory = new byte[0x10000];
            rom.CopyTo(memory, 0);

            var cpu = new SharpSM83(new MmuMock(memory));
            var lastPC = 0;
            cpu.Registers.PC = 101;
            var characters = new List<byte>();            

            while (lastPC != cpu.Registers.PC)
            {
                lastPC = cpu.Registers.PC;

                cpu.Tick();
                
                if (memory[0xff02] == 0x81)
                {
                    characters.Add(memory[0xff01]);
                    memory[0xff02] = 0x01;
                }
            }

            var message = Encoding.Default.GetString(characters.ToArray());
            Assert.That(message.Contains("Passed") && !message.Contains("Failed"), "message: " + message);
        }
    }
}
