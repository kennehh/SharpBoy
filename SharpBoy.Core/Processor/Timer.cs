using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Processor
{
    internal class Timer
    {
        public byte DIV { get; private set; }
        public byte TIMA { get; private set; }
        public byte TMA { get; private set; }
        public byte TAC { get; private set; }

        private const int DivClock = 256;
        private static readonly int[] TacInputClocks = new[]
        {
            1024, // 00: CPU Clock / 1024 (DMG, SGB2, CGB Single Speed Mode:   4096 Hz, SGB1:   ~4194 Hz, CGB Double Speed Mode:   8192 Hz)
            16,   // 01: CPU Clock / 16   (DMG, SGB2, CGB Single Speed Mode: 262144 Hz, SGB1: ~268400 Hz, CGB Double Speed Mode: 524288 Hz)
            64,   // 10: CPU Clock / 64   (DMG, SGB2, CGB Single Speed Mode:  65536 Hz, SGB1:  ~67110 Hz, CGB Double Speed Mode: 131072 Hz)
            256,  // 11: CPU Clock / 256  (DMG, SGB2, CGB Single Speed Mode:  16384 Hz, SGB1:  ~16780 Hz, CGB Double Speed Mode:  32768 Hz)
        };
        private readonly InterruptManager interruptManager;
        private int divCycles = 0;
        private int timaCycles = 0;

        private bool isTimerEnabled => Utils.IsBitSet(TAC, 2);

        public Timer(InterruptManager interruptManager)
        {
            this.interruptManager = interruptManager;
        }

        public void Step(int cycles)
        {
            HandleDivider(cycles);
            HandleTimer(cycles);
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

        private void HandleDivider(int cycles)
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

        private void HandleTimer(int cycles)
        {
            if (isTimerEnabled)
            {
                var tacClock = GetSelectedTacInputClock();
                timaCycles += cycles;
                while (timaCycles >= tacClock)
                {
                    if (TIMA >= 0xff)
                    {
                        TIMA = TMA;
                        interruptManager.SetInterruptFlag(Interrupt.Timer, true);
                    }
                    else
                    {
                        TIMA++;
                    }
                    timaCycles -= tacClock;
                }
            }
        }

        private int GetSelectedTacInputClock() => TacInputClocks[TAC & 0x3];
    }
}
