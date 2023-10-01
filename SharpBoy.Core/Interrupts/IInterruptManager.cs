namespace SharpBoy.Core.Interrupts
{
    public interface IInterruptManager
    {
        InterruptFlags IE { get; set; }
        InterruptFlags IF { get; set; }
        bool IME { get; set; }

        bool AnyInterruptRequested { get; }
        void RequestInterrupt(InterruptFlags flag);
        void ClearInterrupt(InterruptFlags flag);
        Interrupt GetRequestedInterrupt();
    }

    [Flags]
    public enum InterruptFlags : byte
    {
        VBlank = 1 << 0,
        LcdStat = 1 << 1,
        Timer = 1 << 2,
        Serial = 1 << 3,
        Joypad = 1 << 4,
    }

    public class Interrupt
    {
        public InterruptFlags Flag { get; }
        public ushort Vector { get; }

        internal Interrupt(InterruptFlags flag, ushort vector)
        {
            Flag = flag;
            Vector = vector;
        }
    }
}
