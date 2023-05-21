using Newtonsoft.Json;

namespace SharpBoy.Cpu.Tests
{
    public class AluTests
    {
        [Test, Parallelizable]
        public void AluTest_adc() => RunAluTests("adc", AluOperations.adc);

        [Test, Parallelizable]
        public void AluTest_add() => RunAluTests("add", AluOperations.add);

        [Test, Parallelizable]
        public void AluTest_and() => RunAluTests("and", AluOperations.and);

        //[Test, Parallelizable]
        //public void AluTest_bit() => RunAluTests("bit", AluOperations.bit);

        [Test, Parallelizable]
        public void AluTest_ccf() => RunAluTests("ccf", AluOperations.ccf);

        [Test, Parallelizable]
        public void AluTest_cp() => RunAluTests("cp", AluOperations.cp);

        [Test, Parallelizable]
        public void AluTest_cpl() => RunAluTests("cpl", AluOperations.cpl);

        [Test, Parallelizable]
        public void AluTest_daa() => RunAluTests("daa", AluOperations.daa);

        [Test, Parallelizable]
        public void AluTest_or() => RunAluTests("or", AluOperations.or);

        //[Test, Parallelizable]
        //public void AluTest_res() => RunAluTests("res", AluOperations.res);

        [Test, Parallelizable]
        public void AluTest_rl() => RunAluTests("rl", AluOperations.rl);

        [Test, Parallelizable]
        public void AluTest_rlc() => RunAluTests("rlc", AluOperations.rlc);

        [Test, Parallelizable]
        public void AluTest_rr() => RunAluTests("rr", AluOperations.rr);

        [Test, Parallelizable]
        public void AluTest_rrc() => RunAluTests("rrc", AluOperations.rrc);

        [Test, Parallelizable]
        public void AluTest_sbc() => RunAluTests("sbc", AluOperations.sbc);

        [Test, Parallelizable]
        public void AluTest_scf() => RunAluTests("scf", AluOperations.scf);

        //[Test, Parallelizable]
        //public void AluTest_set() => RunAluTests("set", AluOperations.set);

        //[Test, Parallelizable]
        //public void AluTest_sla() => RunAluTests("sla", AluOperations.sla);

        //[Test, Parallelizable]
        //public void AluTest_sra() => RunAluTests("sra", AluOperations.sra);

        //[Test, Parallelizable]
        //public void AluTest_srl() => RunAluTests("srl", AluOperations.srl);

        [Test, Parallelizable]
        public void AluTest_sub() => RunAluTests("sub", AluOperations.sub);

        [Test, Parallelizable]
        public void AluTest_swap() => RunAluTests("swap", AluOperations.swap);

        [Test, Parallelizable]
        public void AluTest_xor() => RunAluTests("xor", AluOperations.xor);

        private void RunAluTests(string opType, Func<Registers, byte, byte> method) => RunAluTests(opType, (r, a, b) => method(r, a));

        private void RunAluTests(string opType, Action<Registers> method) => RunAluTests(opType, (r, a, b) =>
        {
            method(r);
            return a;
        });

        private void RunAluTests(string opType, Func<Registers, byte, byte, byte> method)
        {
            var serializer = new JsonSerializer();
            var registers = new Registers();

            using (var s = File.Open($"gameboy-test-data/alu_tests/v1/{opType}.json", FileMode.Open))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        int result = 0;
                        var test = serializer.Deserialize<AluTest>(reader);
                        registers.F = Convert.ToByte(test.flags, 16);

                        result = method(registers, Convert.ToByte(test.x, 16), Convert.ToByte(test.y, 16));

                        Assert.That(result, Is.EqualTo(Convert.ToByte(test.result.value, 16)), () => $"Value is incorrect, test {opType}: {JsonConvert.SerializeObject(test)}");
                        Assert.That(registers.F, Is.EqualTo(Convert.ToByte(test.result.flags, 16)), () => $"Flags are incorrect, test {opType}: {JsonConvert.SerializeObject(test)}");
                    }
                }
            }
        }

        private class AluTest
        {
            public string x { get; set; }
            public string y { get; set; }
            public string flags { get; set; }
            public AluTestResult result { get; set; }
        }

        private class AluTestResult
        {
            public string value { get; set; }
            public string flags { get; set; }
        }
    }
}