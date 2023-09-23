using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
