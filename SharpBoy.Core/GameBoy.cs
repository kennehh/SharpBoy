using SharpBoy.Core.Cpu;
using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core
{
    public class GameBoy
    {
        internal SharpSM83 Cpu { get; }
        internal Ppu Ppu { get; }
        internal IMmu Mmu { get; }
        internal Cartridge Cartridge { get; private set; }

        private const int ClockSpeed = 4194304;
        private const int RefreshRate = 60;

        public GameBoy()
        {
            Mmu = new Mmu(this);
            Cpu = new SharpSM83(Mmu);
            Ppu = new Ppu();
        }

        public void LoadCartridge(string path)
        {
            var rom = File.ReadAllBytes(path);
            Cartridge = new Cartridge(rom);
        }

        public void Run()
        {
            var cycles = 0;
            while (cycles < RefreshRate)
            {
                Cpu.Tick();
            }
        }
    }
}
