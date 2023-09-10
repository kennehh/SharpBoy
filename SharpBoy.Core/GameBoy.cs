using SharpBoy.Core.Processor;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SharpBoy.Core
{
    public class GameBoy
    {
        public bool PowerOn { get; set; }

        internal Cpu Cpu { get; }
        internal Ppu Ppu { get; }
        internal IMmu Mmu { get; }
        internal InterruptManager InterruptManager { get; }
        internal Processor.Timer Timer { get; }
        internal Cartridge Cartridge { get; private set; }

        private const double RefreshRateHz = 59.7275;
        private const int CpuSpeedHz = 4194304;
        private const int CpuCyclesPerMillisecond = CpuSpeedHz / 1000;
        private const int ExpectedMillisecondsPerUpdate = (int)(1000 / RefreshRateHz);
        private const int ExpectedCpuCyclesPerUpdate = ExpectedMillisecondsPerUpdate * CpuCyclesPerMillisecond;

        private readonly Stopwatch timer = new Stopwatch();

        public GameBoy()
        {
            Mmu = new Mmu(this);
            Cpu = new Cpu(Mmu);
            Ppu = new Ppu();
            InterruptManager = new InterruptManager(Cpu);
            Timer = new Processor.Timer(InterruptManager);
        }

        public void LoadCartridge(string path)
        {
            var rom = File.ReadAllBytes(path);
            Cartridge = new Cartridge(rom);
        }

        public void Run()
        {
            var totalCycles = 0;
            while (PowerOn)
            {
                timer.Start();

                while (totalCycles < ExpectedCpuCyclesPerUpdate)
                {
                    var cycles = Cpu.Step();
                    Timer.Step(cycles);
                    InterruptManager.Step();
                    totalCycles += cycles;
                }

                var timeElapsed = timer.ElapsedMilliseconds;
                if (totalCycles < ExpectedMillisecondsPerUpdate)
                {
                    var sleep = ExpectedMillisecondsPerUpdate - timeElapsed;
                    Thread.Sleep((int)sleep);
                }

                totalCycles -= ExpectedCpuCyclesPerUpdate;
                timer.Reset();
            }
        }
    }
}
