namespace SharpBoy.Core
{
    public class Timer : ITimer
    {
        private byte tac = 0;
        private byte TAC
        {
            get => tac;
            set
            {
                tac = value;
                isTimerEnabled = BitUtils.IsBitSet(value, 2);
                currentTacClock = GetTacClock(value);
            }
        }

        private byte div;
        private byte tima;
        private byte tma;

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

        public void Tick()
        {
            UpdateDivider(4);
            UpdateTimer(4);
        }

        public byte ReadRegister(ushort address)
        {
            return address switch
            {
                0xff04 => div,
                0xff05 => tima,
                0xff06 => tma,
                0xff07 => TAC,
                _ => throw new NotImplementedException()
            };
        }

        public void WriteRegister(ushort address, byte value)
        {
            switch (address)
            {
                case 0xff04: div = 0; break;
                case 0xff05: tima = value; break;
                case 0xff06: tma = value; break;
                case 0xff07: TAC = value; break;
                default: throw new NotImplementedException();
            }
        }

        private void UpdateDivider(int cycles)
        {
            divCycles += cycles;
            while (divCycles >= DivClock)
            {
                if (div >= 0xff)
                {
                    div = 0;
                }
                else
                {
                    div++;
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
                    if (tima >= 0xff)
                    {
                        tima = tma;
                        interruptManager.RequestInterrupt(InterruptFlag.Timer);
                    }
                    else
                    {
                        tima++;
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
