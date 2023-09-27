using SharpBoy.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Interrupts
{
    public class InterruptManager : IInterruptManager
    {
        public InterruptFlags IE { get; set; }
        public InterruptFlags IF { get; set; }
        public bool IME { get; set; }

        private readonly static Interrupt[] interrupts = Enum
            .GetValues(typeof(InterruptFlags))
            .Cast<InterruptFlags>()
            .Select((x, i) => new Interrupt(x, (ushort)(0x40 + i * 0x08)))
            .ToArray();

        public bool AnyInterruptRequested => ((byte)IE & (byte)IF & 0x1f) != 0;

        public void RequestInterrupt(InterruptFlags flag) => SetInterruptFlag(flag, true);
        public void ClearInterrupt(InterruptFlags flag) => SetInterruptFlag(flag, false);
        public Interrupt GetRequestedInterrupt() => interrupts.FirstOrDefault(x => IsInterruptRequested(x.Flag));

        private void SetInterruptFlag(InterruptFlags flag, bool val) => IF = val ? IF | flag : IF & ~flag;
        private bool IsInterruptRequested(InterruptFlags flag) => IE.HasFlag(flag) && IF.HasFlag(flag);
    }
}
