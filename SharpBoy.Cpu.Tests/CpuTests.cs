using Newtonsoft.Json;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpBoy.Cpu.Tests.AluTests;

namespace SharpBoy.Cpu.Tests
{
    public class CpuTests
    {
        private readonly static string[] opcodes = new string[]
        {
            "0x00", "0x01", "0x02", "0x03", "0x04", "0x05", "0x06", "0x07", "0x08", "0x09", "0x0a", "0x0b", "0x0c", "0x0d", "0x0e", "0x0f",
            "0x10", "0x11", "0x12", "0x13", "0x14", "0x15", "0x16", "0x17", "0x18", "0x19", "0x1a", "0x1b", "0x1c", "0x1d", "0x1e", "0x1f",
            "0x20", "0x21", "0x22", "0x23", "0x24", "0x25", "0x26", "0x27", "0x28", "0x29", "0x2a", "0x2b", "0x2c", "0x2d", "0x2e", "0x2f",
            "0x30", "0x31", "0x32", "0x33", "0x34", "0x35", "0x36", "0x37", "0x38", "0x39", "0x3a", "0x3b", "0x3c", "0x3d", "0x3e", "0x3f",
            "0x40", "0x41", "0x42", "0x43", "0x44", "0x45", "0x46", "0x47", "0x48", "0x49", "0x4a", "0x4b", "0x4c", "0x4d", "0x4e", "0x4f",
            "0x50", "0x51", "0x52", "0x53", "0x54", "0x55", "0x56", "0x57", "0x58", "0x59", "0x5a", "0x5b", "0x5c", "0x5d", "0x5e", "0x5f",
            "0x60", "0x61", "0x62", "0x63", "0x64", "0x65", "0x66", "0x67", "0x68", "0x69", "0x6a", "0x6b", "0x6c", "0x6d", "0x6e", "0x6f",
            "0x70", "0x71", "0x72", "0x73", "0x74", "0x75", "0x76", "0x77", "0x78", "0x79", "0x7a", "0x7b", "0x7c", "0x7d", "0x7e", "0x7f",
            "0x80", "0x81", "0x82", "0x83", "0x84", "0x85", "0x86", "0x87", "0x88", "0x89", "0x8a", "0x8b", "0x8c", "0x8d", "0x8e", "0x8f",
            "0x90", "0x91", "0x92", "0x93", "0x94", "0x95", "0x96", "0x97", "0x98", "0x99", "0x9a", "0x9b", "0x9c", "0x9d", "0x9e", "0x9f",
            "0xa0", "0xa1", "0xa2", "0xa3", "0xa4", "0xa5", "0xa6", "0xa7", "0xa8", "0xa9", "0xaa", "0xab", "0xac", "0xad", "0xae", "0xaf",
            "0xb0", "0xb1", "0xb2", "0xb3", "0xb4", "0xb5", "0xb6", "0xb7", "0xb8", "0xb9", "0xba", "0xbb", "0xbc", "0xbd", "0xbe", "0xbf",
            "0xc0", "0xc1", "0xc2", "0xc3", "0xc4", "0xc5", "0xc6", "0xc7", "0xc8", "0xc9", "0xca", "0xcb", "0xcc", "0xcd", "0xce", "0xcf",
            "0xd0", "0xd1", "0xd2",         "0xd4", "0xd5", "0xd6", "0xd7", "0xd8", "0xd9", "0xda",         "0xdc",         "0xde", "0xdf",
            "0xe0", "0xe1", "0xe2",                 "0xe5", "0xe6", "0xe7", "0xe8", "0xe9", "0xea",                         "0xee", "0xef",
            "0xf0", "0xf1", "0xf2", "0xf3",         "0xf5", "0xf6", "0xf7", "0xf8", "0xf9", "0xfa", "0xfb",                 "0xfe", //"0xff"
        };

        //private readonly static byte[] opcodes = new byte[] { 0 };

        [Test, TestCaseSource(nameof(opcodes)), Parallelizable(ParallelScope.All)]
        public void OpcodeTest(string opcodeString)
        {
            var opcode = Convert.ToByte(opcodeString, 16);
            var serializer = new JsonSerializer();
            var cpu = new CpuCore(0x10000);

            using (var s = File.Open($"gameboy-test-data/cpu_tests/v1/{opcode:x2}.json", FileMode.Open))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        var test = serializer.Deserialize<CpuTest>(reader);
                        SetupInitialValues(cpu, test.initial);

                        var cycles = cpu.Tick();
                        AssertCpuState(cpu, test.final);

                        var expectedCycles = test.cycles.Where(x => x?.Any() ?? false).Count() * 4;
                        Assert.That(cycles, Is.EqualTo(expectedCycles), "Cycles incorrect");
                    }
                }
            }
        }

        private static void SetupInitialValues(CpuCore cpu, CpuTestData data)
        {
            cpu.registers.A = Convert.ToByte(data.cpu.a, 16);
            cpu.registers.B = Convert.ToByte(data.cpu.b, 16);
            cpu.registers.C = Convert.ToByte(data.cpu.c, 16);
            cpu.registers.D = Convert.ToByte(data.cpu.d, 16);
            cpu.registers.E = Convert.ToByte(data.cpu.e, 16);
            cpu.registers.F = Convert.ToByte(data.cpu.f, 16);
            cpu.registers.H = Convert.ToByte(data.cpu.h, 16);
            cpu.registers.L = Convert.ToByte(data.cpu.l, 16);
            cpu.registers.PC = Convert.ToUInt16(data.cpu.pc, 16);
            cpu.registers.SP = Convert.ToUInt16(data.cpu.sp, 16);

            foreach (var addressValue in data.ram)
            {
                var address = Convert.ToUInt16(addressValue[0], 16);
                var value = Convert.ToByte(addressValue[1], 16);
                cpu.memory.Write8Bit(address, value);
            }
        }

        private void AssertCpuState(CpuCore cpu, CpuTestData data)
        {
            var a = Convert.ToByte(data.cpu.a, 16);
            var b = Convert.ToByte(data.cpu.b, 16);
            var c = Convert.ToByte(data.cpu.c, 16);
            var d = Convert.ToByte(data.cpu.d, 16);
            var e = Convert.ToByte(data.cpu.e, 16);
            var f = Convert.ToByte(data.cpu.f, 16);
            var h = Convert.ToByte(data.cpu.h, 16);
            var l = Convert.ToByte(data.cpu.l, 16);
            var pc =  Convert.ToUInt16(data.cpu.pc, 16);
            var sp =  Convert.ToUInt16(data.cpu.sp, 16);

            Assert.That(cpu.registers.A, Is.EqualTo(a), "A is incorrect");
            Assert.That(cpu.registers.B, Is.EqualTo(b), "B is incorrect");
            Assert.That(cpu.registers.C, Is.EqualTo(c), "C is incorrect");
            Assert.That(cpu.registers.D, Is.EqualTo(d), "D is incorrect");
            Assert.That(cpu.registers.E, Is.EqualTo(e), "E is incorrect");
            Assert.That(cpu.registers.F, Is.EqualTo(f), "F is incorrect");
            Assert.That(cpu.registers.H, Is.EqualTo(h), "H is incorrect");
            Assert.That(cpu.registers.L, Is.EqualTo(l), "L is incorrect");
            Assert.That(cpu.registers.PC, Is.EqualTo(pc), "PC is incorrect");
            Assert.That(cpu.registers.SP, Is.EqualTo(sp), "SP is incorrect");

            foreach (var addressValue in data.ram)
            {
                var address = Convert.ToUInt16(addressValue[0], 16);
                var expected = Convert.ToByte(addressValue[1], 16);
                var actual = cpu.memory.Read8Bit(address);
                Assert.That(actual, Is.EqualTo(expected), $"Value at memory address {address:x4} is incorrect");
            }
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
            public CpuTestDataCpu cpu { get; set; }
            public string[][] ram { get; set; }
        }
        
        private class CpuTestDataCpu
        {
            public string a { get; set; }
            public string b { get; set; }
            public string c { get; set; }
            public string d { get; set; }
            public string e { get; set; }
            public string f { get; set; }
            public string h { get; set; }
            public string l { get; set; }
            public string pc { get; set; }
            public string sp { get; set; }
        }
    }
}
