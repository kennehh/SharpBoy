using SharpBoy.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Processor
{
    internal class InterruptManager
    {
        public InterruptFlag IE { get; set; }
        public InterruptFlag IF { get; set; }


        private readonly Cpu cpu;
        private readonly static Interrupt[] interrupts = Enum
            .GetValues(typeof(InterruptFlag))
            .Cast<InterruptFlag>()
            .Select((x, i) => new Interrupt(x, (ushort)(0x40 + (i * 0x08))))
            .ToArray();

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
                    foreach (var interrupt in interrupts)
                    {
                        if (InterruptRequested(interrupt.Flag))
                        {
                            cpu.Registers.IME = false;
                            SetInterruptFlag(interrupt.Flag, false);
                            cpu.HandleInterrupt(interrupt.Address);

                            break;
                        }
                        index++;
                    }
                }
            }
        }

        public void RequestInterrupt(InterruptFlag flag) => SetInterruptFlag(flag, true);

        private void SetInterruptFlag(InterruptFlag flag, bool val) => IF = (val ? IF | flag : IF & ~flag);
        private bool InterruptRequested(InterruptFlag flag) => IE.HasFlag(flag) && IF.HasFlag(flag);

        private class Interrupt
        {
            public InterruptFlag Flag { get; }
            public ushort Address { get; }

            public Interrupt(InterruptFlag flag, ushort address) 
            {
                Flag = flag;
                Address = address;
            }
        }
    }

    [Flags]
    internal enum InterruptFlag : byte
    {
        VBlank = 1 << 0,
        LcdStat = 1 << 1,
        Timer = 1 << 2,
        Serial = 1 << 3,
        Joypad = 1 << 4,
    }
}
