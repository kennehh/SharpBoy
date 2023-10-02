using SharpBoy.Core.Graphics.Interfaces;
using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    public class PpuMemory : IPpuMemory
    {
        public IReadWriteMemory Vram { get; } = new Ram(0x2000);
        public IReadWriteMemory Oam { get; } = new Ram(0xa0);
    }
}
