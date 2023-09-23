using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core.Memory
{
    public interface IMmu : IReadWriteMemory
    {
        bool BootRomLoaded { get; }
        void LoadBootRom(byte[] rom);
    }
}
