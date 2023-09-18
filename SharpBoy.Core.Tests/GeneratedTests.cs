using Newtonsoft.Json;
using NUnit.Framework.Internal;
using SharpBoy.Core.Processor;
using SharpBoy.Core.Tests.Mocks;
using SharpBoy.Core.Graphics;
using Microsoft.Extensions.DependencyInjection;
using SharpBoy.Core.Rendering;
using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class InstructionTests
    {
        private const string DirectoryPath = "GeneratedTests/InstructionTests";
        private static IEnumerable<string> Tests => Directory.GetFiles(DirectoryPath).Select(Path.GetFileNameWithoutExtension);

        [Test, TestCaseSource(nameof(Tests))]
        public void InstructionTest(string opcode)
        {
            var serializer = new JsonSerializer();
            var cpu = CreateCpu();

            using (var s = File.Open(Path.Combine(DirectoryPath, $"{opcode}.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        var test = serializer.Deserialize<CpuTest>(reader);
                        SetupInitialValues(cpu, test.initial);

                        var cycles = cpu.Step();
                        AssertCpuState(cpu, cycles, test);
                    }
                }
            }
        }

        private static Cpu CreateCpu()
        {
            var serviceProvider = new ServiceCollection()
                .RegisterCoreServices()
                .AddSingleton<IMmu, MmuMock>()
                .BuildServiceProvider();

            return (Cpu)serviceProvider.GetService<ICpu>();
        }

        private static void SetupInitialValues(Cpu cpu, CpuTestData data)
        {
            cpu.Registers.A = data.a;
            cpu.Registers.B = data.b;
            cpu.Registers.C = data.c;
            cpu.Registers.D = data.d;
            cpu.Registers.E = data.e;
            cpu.Registers.F = (Flag)data.f;
            cpu.Registers.H = data.h;
            cpu.Registers.L = data.l;
            cpu.Registers.PC = data.pc;
            cpu.Registers.SP = data.sp;
            cpu.InterruptManager.IME = data.ime == 1;
            cpu.State = CpuState.Running;

            foreach (var addressValue in data.ram)
            {
                var address = addressValue[0];
                var value = addressValue[1];
                cpu.Mmu.Write(address, (byte)value);
            }
        }

        private void AssertCpuState(Cpu cpu, int cycles, CpuTest test)
        {
            var data = test.final;

            Assert.That(cpu.Registers.A, Is.EqualTo(data.a), $"A is incorrect: {test.name}");
            Assert.That(cpu.Registers.B, Is.EqualTo(data.b), $"B is incorrect: {test.name}");
            Assert.That(cpu.Registers.C, Is.EqualTo(data.c), $"C is incorrect: {test.name}");
            Assert.That(cpu.Registers.D, Is.EqualTo(data.d), $"D is incorrect: {test.name}");
            Assert.That(cpu.Registers.E, Is.EqualTo(data.e), $"E is incorrect: {test.name}");
            Assert.That((byte)cpu.Registers.F, Is.EqualTo(data.f), $"F is incorrect: {test.name}");
            Assert.That(cpu.Registers.H, Is.EqualTo(data.h), $"H is incorrect: {test.name}");
            Assert.That(cpu.Registers.L, Is.EqualTo(data.l), $"L is incorrect: {test.name}");
            Assert.That(cpu.Registers.PC, Is.EqualTo(data.pc), $"PC is incorrect: {test.name}");
            Assert.That(cpu.Registers.SP, Is.EqualTo(data.sp), $"SP is incorrect: {test.name}");
            //Assert.That(cpu.InterruptManager.IME, Is.EqualTo(data.ime == 1), $"IME is incorrect: {test.name}");

            foreach (var addressValue in data.ram)
            {
                var address = addressValue[0];
                var expected = addressValue[1];
                var actual = cpu.Mmu.Read(address);
                Assert.That(actual, Is.EqualTo(expected), $"Value at memory address {address:x4} is incorrect: {test.name}");
            }

            Assert.That(cycles, Is.EqualTo(test.cycles.Length * 4), $"Cycles is incorrect: {test.name}");
        }

        private class CpuTest
        {
            public string name { get; set; }
            public CpuTestData initial { get; set; }
            public CpuTestData final { get; set; }
            public string[][] cycles { get; set; }
        }

        private class CpuTestData
        {
            public byte a { get; set; }
            public byte b { get; set; }
            public byte c { get; set; }
            public byte d { get; set; }
            public byte e { get; set; }
            public byte f { get; set; }
            public byte h { get; set; }
            public byte l { get; set; }
            public ushort pc { get; set; }
            public ushort sp { get; set; }
            public byte ime { get; set; }
            public ushort[][] ram { get; set; }
        }
    }
}
