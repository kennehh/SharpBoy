using SharpBoy.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Processor
{
    internal class InterruptManager : IInterruptManager
    {
        public InterruptFlag IE { get; set; }
        public InterruptFlag IF { get; set; }
        public bool IME { get; set; }


        private readonly static Interrupt[] interrupts = Enum
            .GetValues(typeof(InterruptFlag))
            .Cast<InterruptFlag>()
            .Select((x, i) => new Interrupt(x, (ushort)(0x40 + (i * 0x08))))
            .ToArray();

        public bool AnyInterruptRequested => (IE & IF) != 0;

        public void RequestInterrupt(InterruptFlag flag) => SetInterruptFlag(flag, true);
        public void ClearInterrupt(InterruptFlag flag) => SetInterruptFlag(flag, false);
        public Interrupt GetRequestedInterrupt() => interrupts.FirstOrDefault(x => IsInterruptRequested(x.Flag));

        private void SetInterruptFlag(InterruptFlag flag, bool val) => IF = (val ? IF | flag : IF & ~flag);
        private bool IsInterruptRequested(InterruptFlag flag) => IE.HasFlag(flag) && IF.HasFlag(flag);
    }
}
