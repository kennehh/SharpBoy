using SharpBoy.Core.Cartridges;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.Memory;
using SharpBoy.Core.Processor;
using System.Diagnostics;
using System.IO.Compression;

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
        private bool runUncapped = false;

        private readonly object syncRoot = new object();

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

        public void LoadCartridge(string pathToRom, string pathToRam = null)
        {
            byte[] rom, ram = null;

            if (Path.GetExtension(pathToRom) == ".zip")
            {
                rom = ReadFromZipFile(pathToRom);
            }
            else
            {
                rom = File.ReadAllBytes(pathToRom);
            }

            if (!string.IsNullOrWhiteSpace(pathToRam))
            {
                ram = File.ReadAllBytes(pathToRam);
            }

            var cart = cartridgeFactory.CreateCartridge(rom, ram);
            Mmu.LoadCartridge(cart);
        }

        public void InitialiseGameBoyState()
        {
            stopped = false;
            Cpu.ResetState(Mmu.BootRomLoaded);
            Ppu.ResetState(Mmu.BootRomLoaded);
        }

        private long cyclesEmulated = 0;
        private long lastCyclesTime = 0;
        private long cyclesCounter = 0;
        private Stopwatch stopwatch = new Stopwatch();

        public void Run()
        {
            InitialiseGameBoyState();

            stopwatch = Stopwatch.StartNew();
            var targetCyclesPerSecond = CpuSpeedHz;

            while (!stopped)
            {
                var elapsedTime = stopwatch.ElapsedMilliseconds;
                var expectedCycles = targetCyclesPerSecond * elapsedTime / 1000;
                var cyclesToEmulate = expectedCycles - cyclesEmulated;

                while (cyclesToEmulate > 0 || runUncapped)
                {
                    try
                    {
                        var cycles = Step();

                        if (stopped)
                        {
                            break;
                        }

                        cyclesToEmulate -= cycles;
                        cyclesEmulated += cycles;

                        // Update the cycles counter.
                        cyclesCounter += cycles;

                        // If a full second has passed, display the speed and reset counters.
                        if (stopwatch.ElapsedMilliseconds - lastCyclesTime >= 1000)
                        {
                            double speedPercentage = (double)cyclesCounter / CpuSpeedHz * 100;
                            Console.WriteLine($"Running at {speedPercentage:0.00}% of real Gameboy speed.");
                            lastCyclesTime = stopwatch.ElapsedMilliseconds;
                            cyclesCounter = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                if (!runUncapped && cyclesToEmulate <= 0)
                {
                    Thread.Sleep(1);
                }
            }
        }

        public void Stop()
        {
            stopped = true;
        }

        public void UncapSpeed(bool value)
        {
            lock (syncRoot)
            {
                if (runUncapped != value)
                {
                    runUncapped = value;
                    cyclesEmulated = 0;
                    lastCyclesTime = 0;
                    cyclesCounter = 0;
                    stopwatch.Restart();
                }
            }
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
