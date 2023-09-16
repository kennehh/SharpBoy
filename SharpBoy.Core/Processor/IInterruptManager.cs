using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Processor
{
    internal interface IInterruptManager
    {
        InterruptFlag IE { get; set; }
        InterruptFlag IF { get; set; }
        bool IME { get; set; }

        bool AnyInterruptRequested => (IE & IF) != 0;
        void RequestInterrupt(InterruptFlag flag);
        void ClearInterrupt(InterruptFlag flag);
        Interrupt GetRequestedInterrupt();
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

    internal class Interrupt
    {
        public InterruptFlag Flag { get; }
        public ushort Vector { get; }

        public Interrupt(InterruptFlag flag, ushort vector)
        {
            Flag = flag;
            Vector = vector;
        }
    }
}
