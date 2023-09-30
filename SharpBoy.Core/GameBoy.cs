using SharpBoy.Core.Processor;
using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.Cartridges;
using System.IO.Compression;
using System.Runtime.Intrinsics.Arm;

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
        private readonly ICartridgeFactory cartridgeFactory;
        private readonly GameBoyState state;
        private bool stopped = false;

        public GameBoy(ICpu cpu, IMmu mmu, IPpu ppu, ICartridgeFactory cartridgeFactory, GameBoyState state)
        {
            Cpu = cpu;
            Mmu = mmu;
            Ppu = ppu;
            this.cartridgeFactory = cartridgeFactory;
            this.state = state;
        }

        public void LoadBootRom(string path)
        {
            var rom = File.ReadAllBytes(path);
            Mmu.LoadBootRom(rom);
        }

        public void LoadCartridge(string path)
        {
            byte[] rom;

            if (Path.GetExtension(path) == ".zip")
            {
                rom = ReadFromZipFile(path);
            }
            else
            {
                rom = File.ReadAllBytes(path);
            }

            Mmu.LoadCartridge(cartridgeFactory.CreateCartridge(rom));
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
                    try
                    {
                        var cycles = Step();

                        cyclesToEmulate -= cycles;
                        cyclesEmulated += cycles;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                if (cyclesToEmulate <= 0)
                {
                    if (StateUpdated != null)
                    {
                        state.Registers.Clear();
                        state.Registers.Add(new RegisterState("A", Cpu.Registers.A.ToString("X")));
                        state.Registers.Add(new RegisterState("F", Cpu.Registers.F.ToString("X")));
                        state.Registers.Add(new RegisterState("AF", Cpu.Registers.AF.ToString("X")));
                        state.Registers.Add(new RegisterState("B", Cpu.Registers.B.ToString("X")));
                        state.Registers.Add(new RegisterState("C", Cpu.Registers.C.ToString("X")));
                        state.Registers.Add(new RegisterState("BC", Cpu.Registers.BC.ToString("X")));
                        state.Registers.Add(new RegisterState("D", Cpu.Registers.D.ToString("X")));
                        state.Registers.Add(new RegisterState("E", Cpu.Registers.E.ToString("X")));
                        state.Registers.Add(new RegisterState("DE", Cpu.Registers.DE.ToString("X")));
                        state.Registers.Add(new RegisterState("H", Cpu.Registers.H.ToString("X")));
                        state.Registers.Add(new RegisterState("L", Cpu.Registers.L.ToString("X")));
                        state.Registers.Add(new RegisterState("HL", Cpu.Registers.HL.ToString("X")));
                        state.Registers.Add(new RegisterState("PC", Cpu.Registers.PC.ToString("X")));
                        state.Registers.Add(new RegisterState("STAT", Ppu.Registers.STAT.ToString("X")));
                        state.Registers.Add(new RegisterState("LCDC", Ppu.Registers.LCDC.ToString("X")));

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

        private byte[] ReadFromZipFile(string path)
        {
            byte[] rom;

            using (var zip = ZipFile.OpenRead(path))
            {
                var gbEntry = zip.Entries.FirstOrDefault(x => Path.GetExtension(x.Name) == ".gb");
                if (gbEntry == null)
                {
                    return null;
                }

                using (var stream = gbEntry.Open())
                {
                    var bytesRead = 0;
                    rom = new byte[gbEntry.Length];

                    while (bytesRead < gbEntry.Length)
                    {
                        bytesRead += stream.Read(rom, bytesRead, rom.Length - bytesRead);
                    }

                    return rom;
                }
            }
        }
    }
}
