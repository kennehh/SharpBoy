namespace SharpBoy.Core
{
    public class GameBoyState
    {
        public IList<RegisterState> Registers { get; } = new List<RegisterState>();
        public string LastInstruction { get; set; }
    }

    public struct RegisterState
    {
        public RegisterState(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}
