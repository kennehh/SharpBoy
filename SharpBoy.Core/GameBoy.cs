﻿using SharpBoy.Core.Processor;
using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.Rendering;
using SharpBoy.Core.Cartridge;

namespace SharpBoy.Core
{

    public class GameBoy
    {
        public event Action<GameBoyState> StateUpdated;

        internal ICpu Cpu { get; }
        internal IMmu Mmu { get; }
        internal IPpu Ppu { get; }

        private const double RefreshRateHz = 59.7275;
        private const int CpuSpeedHz = 4194304;
        private const double CpuCyclesPerMillisecond = CpuSpeedHz / 1000;
        private const double ExpectedMillisecondsPerUpdate = 1000 / RefreshRateHz;
        private const int ExpectedCpuCyclesPerUpdate = (int)(ExpectedMillisecondsPerUpdate * CpuCyclesPerMillisecond);
        private readonly ICartridgeReader cartridgeReader;
        private readonly GameBoyState state;
        private bool stopped = false;

        public GameBoy(ICpu cpu, IMmu mmu, IPpu ppu, ICartridgeReader cartridgeReader, GameBoyState state)
        {
            Cpu = cpu;
            Mmu = mmu;
            Ppu = ppu;
            this.cartridgeReader = cartridgeReader;
            this.state = state;
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
            if (!Mmu.BootRomLoaded)
            {
                Cpu.Registers.AF = 0x01b0;
                Cpu.Registers.BC = 0x0013;
                Cpu.Registers.DE = 0x00d8;
                Cpu.Registers.HL = 0x014d;
                Cpu.Registers.PC = 0x0100;
                Cpu.Registers.SP = 0xfffe;

                Ppu.Registers.LCDC = (LcdcFlags)0x91;
                Ppu.Registers.BGP = 0xfc;
                Ppu.Registers.OBP0 = 0xff;
                Ppu.Registers.OBP1 = 0xff;
            }

            var stopwatch = Stopwatch.StartNew();
            var cyclesEmulated = 0L;
            var targetCyclesPerSecond = CpuSpeedHz;

            while (!stopped)
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
                    if (StateUpdated != null)
                    {
                        state.Registers["A"] = Cpu.Registers.A.ToString("X");
                        state.Registers["F"] = Cpu.Registers.F.ToString("X");
                        state.Registers["AF"] = Cpu.Registers.AF.ToString("X");
                        state.Registers["B"] = Cpu.Registers.B.ToString("X");
                        state.Registers["C"] = Cpu.Registers.C.ToString("X");
                        state.Registers["BC"] = Cpu.Registers.BC.ToString("X");
                        state.Registers["D"] = Cpu.Registers.D.ToString("X");
                        state.Registers["E"] = Cpu.Registers.E.ToString("X");
                        state.Registers["DE"] = Cpu.Registers.DE.ToString("X");
                        state.Registers["H"] = Cpu.Registers.H.ToString("X");
                        state.Registers["L"] = Cpu.Registers.L.ToString("X");
                        state.Registers["HL"] = Cpu.Registers.HL.ToString("X");
                        state.Registers["PC"] = Cpu.Registers.PC.ToString("X");

                        state.Registers["STAT"] = Ppu.Registers.STAT.ToString("X");
                        state.Registers["LCDC"] = Ppu.Registers.LCDC.ToString();

                        StateUpdated(state);
                    }

                    Thread.Sleep(1);
                }
            }
        }

        public void Stop()
        {
            stopped = true;
        }

        internal int Step()
        {
            var cycles = Cpu.Step();
            return cycles;
        }
    }
}
