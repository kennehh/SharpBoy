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

        internal Cpu Cpu { get; }
        internal IPpu Ppu { get; }
        internal IMmu Mmu { get; }
        internal IInterruptManager InterruptManager { get; }
        internal ITimer Timer { get; }
        internal IRenderer Renderer { get; }
        internal Cartridge Cartridge { get; private set; } = new Cartridge();

        private const double RefreshRateHz = 59.7275;
        private const int CpuSpeedHz = 4194304;
        private const double CpuCyclesPerMillisecond = CpuSpeedHz / 1000;
        private const double ExpectedMillisecondsPerUpdate = 1000 / RefreshRateHz;
        private const int ExpectedCpuCyclesPerUpdate = (int)(ExpectedMillisecondsPerUpdate * CpuCyclesPerMillisecond);

        public GameBoy()
        {
            Mmu = new Mmu(this);
            InterruptManager = new InterruptManager();
            Timer = new Timer(InterruptManager);
            Ppu = new Ppu(InterruptManager);
            Cpu = new Cpu(Mmu, InterruptManager, Timer, Ppu);
            Renderer = new SilkRenderer();
        }

        public void LoadBootRom(string path)
        {
            var rom = File.ReadAllBytes(path);
            Mmu.LoadBootRom(rom);
        }

        public void LoadCartridge(string path)
        {
            var rom = File.ReadAllBytes(path);
            Cartridge = new Cartridge(rom);
        }

        //public void Run()
        //{
        //    Renderer.OpenWindow();
        //    var cycles = 0;
        //    var stopwatch = Stopwatch.StartNew();

        //    while (Renderer.IsWindowOpen())
        //    {
        //        stopwatch.Restart();
        //        while (cycles < ExpectedCpuCyclesPerUpdate)
        //        {
        //            cycles += Step();
        //        }
        //        cycles -= ExpectedCpuCyclesPerUpdate;

        //        Renderer.Render();

        //        var elapsedMilliseconds = stopwatch.ElapsedTicks / 10000d;
        //        if (elapsedMilliseconds < ExpectedMillisecondsPerUpdate)
        //        {
        //            var sleep = (int)(ExpectedMillisecondsPerUpdate - elapsedMilliseconds);
        //            Thread.Sleep(sleep);
        //        }
        //    }
        //}

        public void Run()
        {
            var cyclesEmulated = 0L;
            var targetCyclesPerSecond = CpuSpeedHz;

            Renderer.Initialise();
            var stopwatch = Stopwatch.StartNew();

            Renderer.Run(() =>
            {
                Renderer.Render(Ppu.FrameBuffer);

                var elapsedTime = stopwatch.ElapsedMilliseconds;
                var expectedCycles = targetCyclesPerSecond * elapsedTime / 1000;
                var cyclesToEmulate = expectedCycles - cyclesEmulated;

                while (cyclesToEmulate > 0)
                {
                    var cycles = Step();

                    cyclesToEmulate -= cycles;
                    cyclesEmulated += cycles;
                }

                if (cyclesToEmulate <= 0)
                {
                    Thread.Sleep(1);
                }
            });
        }

        internal int Step()
        {
            var cycles = Cpu.Step();
            return cycles;
        }
    }
}
