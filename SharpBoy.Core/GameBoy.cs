using SharpBoy.Core.Processor;
using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.Rendering;

namespace SharpBoy.Core
{

    public class GameBoy
    {
        public event Action Stopped;

        internal ICpu Cpu { get; }
        internal IMmu Mmu { get; }

        private const double RefreshRateHz = 59.7275;
        private const int CpuSpeedHz = 4194304;
        private const double CpuCyclesPerMillisecond = CpuSpeedHz / 1000;
        private const double ExpectedMillisecondsPerUpdate = 1000 / RefreshRateHz;
        private const int ExpectedCpuCyclesPerUpdate = (int)(ExpectedMillisecondsPerUpdate * CpuCyclesPerMillisecond);

        private readonly IRenderer renderer;
        private readonly ICartridgeReader cartridgeReader;

        public GameBoy(ICpu cpu, IMmu mmu, IRenderer renderer, ICartridgeReader cartridgeReader)
        {
            Cpu = cpu;
            Mmu = mmu;
            this.renderer = renderer;
            this.cartridgeReader = cartridgeReader;
        }

        public void LoadBootRom(string path)
        {
            var rom = File.ReadAllBytes(path);
            Mmu.LoadBootRom(rom);
        }

        public void LoadCartridge(string path)
        {
            var rom = File.ReadAllBytes(path);
            cartridgeReader.LoadCartridge(rom);
        }

        public void Run()
        {
            renderer.Initialise();

            var emulationThread = new Thread(RunEmulator);
            var renderThread = new Thread(renderer.Run);

            emulationThread.Start();
            renderThread.Start();
        }

        internal int Step()
        {
            var cycles = Cpu.Step();
            return cycles;
        }

        private void RunEmulator()
        {
            var stopwatch = Stopwatch.StartNew();
            var cyclesEmulated = 0L;
            var targetCyclesPerSecond = CpuSpeedHz;
            var windowClosing = false;

            renderer.Closing += () => windowClosing = true;

            while (!windowClosing)
            {
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
            }

            Stopped?.Invoke();
        }
    }
}
