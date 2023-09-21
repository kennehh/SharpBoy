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
        public ConcurrentDictionary<string, string> Registers { get; } = new ConcurrentDictionary<string, string>();
        public string LastInstruction { get; set; }
    }
}
