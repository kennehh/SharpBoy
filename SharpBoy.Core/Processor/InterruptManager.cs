using SharpBoy.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Processor
{
    internal class InterruptManager
    {
        public Interrupt IE { get; set; }
        public Interrupt IF { get; set; }


        private readonly Cpu cpu;
        private bool AnyInterruptRequested => (IE & IF) != 0;

        public InterruptManager(Cpu cpu)
        {
            this.cpu = cpu;
        }

        public void Step()
        {
            if (AnyInterruptRequested)
            {
                if (cpu.Halted)
                {
                    cpu.Halted = false;
                }

                if (cpu.Registers.IME)
                {
                    var index = 0;
                    foreach (Interrupt interrupt in Enum.GetValues(typeof(Interrupt)))
                    {
                        if (InterruptRequested(interrupt))
                        {
                            cpu.Registers.IME = false;
                            SetInterruptFlag(interrupt, false);

                            var address = (ushort)(0x40 + (index * 0x08));
                            cpu.HandleInterrupt(address);

                            break;
                        }
                        index++;
                    }
                }
            }
        }

        public void SetInterruptFlag(Interrupt flag, bool val) => IF = (val ? IF | flag : IF & ~flag);

        private bool InterruptRequested(Interrupt flag) => IE.HasFlag(flag) && IF.HasFlag(flag);
    }

    [Flags]
    internal enum Interrupt : byte
    {
        VBlank = 1 << 0,
        LcdStat = 1 << 1,
        Timer = 1 << 2,
        Serial = 1 << 3,
        Joypad = 1 << 4,
    }
}
