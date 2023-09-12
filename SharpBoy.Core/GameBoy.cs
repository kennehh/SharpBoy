using SharpBoy.Core.Processor;
using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SharpBoy.Core.Video;

namespace SharpBoy.Core
{
    public class GameBoy
    {
        public bool PowerOn { get; set; }

        public const double RefreshRateHz = 59.7275;
        public const int CpuSpeedHz = 4194304;
        public const double CpuCyclesPerMillisecond = CpuSpeedHz / 1000;
        public const double ExpectedMillisecondsPerUpdate = 1000 / RefreshRateHz;
        public const double ExpectedCpuCyclesPerUpdate = ExpectedMillisecondsPerUpdate * CpuCyclesPerMillisecond;

        internal Cpu Cpu { get; }
        internal Ppu Ppu { get; }
        internal IMmu Mmu { get; }
        internal IInterruptManager InterruptManager { get; }
        internal ITimer Timer { get; }
        internal Cartridge Cartridge { get; private set; }

        public GameBoy()
        {
            Mmu = new Mmu(this);
            InterruptManager = new InterruptManager();
            Timer = new Timer(InterruptManager);
            Cpu = new Cpu(Mmu, InterruptManager, Timer);
            Ppu = new Ppu(InterruptManager);
        }

        public void LoadCartridge(string path)
        {
            var rom = File.ReadAllBytes(path);
            Cartridge = new Cartridge(rom);
        }

        public int Step()
        {
            var cycles = Cpu.Step();
            Ppu.Sync(cycles);
            return cycles;
        }
    }
}
