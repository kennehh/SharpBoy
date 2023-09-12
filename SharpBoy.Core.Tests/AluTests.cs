using Newtonsoft.Json;
using SharpBoy.Core.Processor;
using System;
using System.Reflection;

namespace SharpBoy.Core.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class AluTests
    {
        private const string DirectoryPath = "GeneratedTests/AluTests";
        private static IEnumerable<string> Tests => Directory.GetFiles(DirectoryPath).Select(x => Path.GetFileNameWithoutExtension(x));

        [Test, TestCaseSource(nameof(Tests))]
        public void AluOperationTest(string opType)
        {
            var method = typeof(AluOperations).GetMethod(opType);
            var parameters = method.GetParameters();

            if (method.ReturnType == typeof(void))
            {
                RunAluTests(opType, (r, a, b) =>
                {
                    method.Invoke(null, new[] { r });
                    return a;
                });
            }
            else if (parameters[0].ParameterType != typeof(Registers))
            {
                RunAluTests(opType, (r, a, b) => (byte)method.Invoke(null, new object[] { a, b }));
            }
            else if (parameters.Count() == 3)
            {
                RunAluTests(opType, (r, a, b) => (byte)method.Invoke(null, new object[] { r, a, b }));
            }
            else
            {
                RunAluTests(opType, (r, a, b) => (byte)method.Invoke(null, new object[] { r, a }));
            }
        }

        private void RunAluTests(string opType, Func<Registers, byte, byte, byte> method)
        {
            var serializer = new JsonSerializer();
            var registers = new Registers();

            using (var s = File.Open(Path.Combine(DirectoryPath, $"{opType}.json"), FileMode.Open))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        int result = 0;
                        var test = serializer.Deserialize<AluTest>(reader);
                        registers.F = (Flag)Convert.ToByte(test.flags, 16);

                        result = method(registers, Convert.ToByte(test.x, 16), Convert.ToByte(test.y, 16));

                        Assert.That(result, Is.EqualTo(Convert.ToByte(test.result.value, 16)), () => $"Value is incorrect, test {opType}: {JsonConvert.SerializeObject(test)}");
                        Assert.That((byte)registers.F, Is.EqualTo(Convert.ToByte(test.result.flags, 16)), () => $"Flags are incorrect, test {opType}: {JsonConvert.SerializeObject(test)}");
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