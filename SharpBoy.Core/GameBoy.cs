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

        private const float RefreshRateHz = 59.7275f;
        private const int CpuSpeedHz = 4194304;
        private const int ExpectedCyclesPerUpdate = (int)(CpuSpeedHz / RefreshRateHz);
        private const int ExpectedMillisecondsPerUpdate = (int)(RefreshRateHz / 1000);


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
            while (PowerOn)
            {
                timer.Start();

                var cycles = 0;
                while (cycles < ExpectedCyclesPerUpdate)
                {
                    cycles = Cpu.Step();
                    Timer.Step(cycles);
                    InterruptManager.Step();
                }

                var timeElapsed = timer.ElapsedMilliseconds;
                if (cycles < ExpectedCyclesPerUpdate)
                {
                    var sleep = ExpectedMillisecondsPerUpdate - timeElapsed;
                    Thread.Sleep((int)sleep);
                }

                cycles -= ExpectedCyclesPerUpdate;
                timer.Reset();
            }
        }
    }
}
