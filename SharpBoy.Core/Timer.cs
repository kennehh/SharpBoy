using SharpBoy.Core.Processor;

namespace SharpBoy.Core
{
    internal class Timer : ITimer
    {
        public byte DIV { get; private set; }
        public byte TIMA { get; private set; }
        public byte TMA { get; private set; }

        private byte tac = 0;
        public byte TAC
        {
            get => tac;
            set
            {
                tac = value;
                isTimerEnabled = BitUtils.IsBitSet(value, 2);
                currentTacClock = GetTacClock(value);
            }
        }

        private bool isTimerEnabled = false;
        private int currentTacClock = GetTacClock(0);

        private const int DivClock = 256;
        private readonly IInterruptManager interruptManager;
        private int divCycles = 0;
        private int timaCycles = 0;

        public Timer(IInterruptManager interruptManager)
        {
            this.interruptManager = interruptManager;
        }

        public void Update(int cycles)
        {
            UpdateDivider(cycles);
            UpdateTimer(cycles);
        }

        public byte ReadRegister(ushort address)
        {
            return address switch
            {
                0xff04 => DIV,
                0xff05 => TIMA,
                0xff06 => TMA,
                0xff07 => TAC,
                _ => throw new NotImplementedException()
            };
        }

        public void WriteRegister(ushort address, byte value)
        {
            switch (address)
            {
                case 0xff04: DIV = 0; break;
                case 0xff05: TIMA = value; break;
                case 0xff06: TMA = value; break;
                case 0xff07: TAC = value; break;
                default: throw new NotImplementedException();
            }
        }

        private void UpdateDivider(int cycles)
        {
            divCycles += cycles;
            while (divCycles >= DivClock)
            {
                if (DIV >= 0xff)
                {
                    DIV = 0;
                }
                else
                {
                    DIV++;
                }
                divCycles -= DivClock;
            }
        }

        private void UpdateTimer(int cycles)
        {
            if (isTimerEnabled)
            {
                timaCycles += cycles;

                while (timaCycles >= currentTacClock)
                {
                    if (TIMA >= 0xff)
                    {
                        TIMA = TMA;
                        interruptManager.RequestInterrupt(InterruptFlag.Timer);
                    }
                    else
                    {
                        TIMA++;
                    }
                    timaCycles -= currentTacClock;
                }
            }
        }

        private static int GetTacClock(byte tac)
        {
            var val = tac & 0x3;
            return val switch
            {
                0b00 => 1024, // 00: CPU Clock / 1024 (DMG, SGB2, CGB Single Speed Mode:   4096 Hz, SGB1:   ~4194 Hz, CGB Double Speed Mode:   8192 Hz)
                0b01 => 16,   // 01: CPU Clock / 16   (DMG, SGB2, CGB Single Speed Mode: 262144 Hz, SGB1: ~268400 Hz, CGB Double Speed Mode: 524288 Hz)
                0b10 => 64,   // 10: CPU Clock / 64   (DMG, SGB2, CGB Single Speed Mode:  65536 Hz, SGB1:  ~67110 Hz, CGB Double Speed Mode: 131072 Hz)
                0b11 => 256,  // 11: CPU Clock / 256  (DMG, SGB2, CGB Single Speed Mode:  16384 Hz, SGB1:  ~16780 Hz, CGB Double Speed Mode:  32768 Hz)
                _ => throw new NotImplementedException()
            };
        }
    }
}
