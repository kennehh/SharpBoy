using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Cartridges
{
    internal class Mbc3RtcController
    {
        private Dictionary<RtcRegister, byte> rtcRegisters = Enum.GetValues<RtcRegister>().ToDictionary(key => key, value => (byte)0);
        private Dictionary<RtcRegister, byte> latchedRtcRegisters = null;

        private Stopwatch rtcStopwatch = Stopwatch.StartNew();
        private TimeSpan rtcElapsedOffset = TimeSpan.Zero;
        
        public void Latch()
        {
            Update();
            latchedRtcRegisters = new(rtcRegisters);
        }

        public void WriteToRegister(RtcRegister register, byte value)
        {
            switch (register)
            {
                case RtcRegister.Seconds:
                    value &= 0b0011_1111;
                    rtcElapsedOffset = rtcElapsedOffset
                        .Subtract(TimeSpan.FromSeconds(rtcElapsedOffset.Seconds))
                        .Add(TimeSpan.FromSeconds(value));
                    rtcStopwatch.Restart();
                    break;
                case RtcRegister.Minutes:
                    value &= 0b0011_1111;
                    rtcElapsedOffset = rtcElapsedOffset
                        .Subtract(TimeSpan.FromMinutes(rtcElapsedOffset.Minutes))
                        .Add(TimeSpan.FromMinutes(value));
                    rtcStopwatch.Restart();
                    break;
                case RtcRegister.Hours:
                    value &= 0b0001_1111;
                    rtcElapsedOffset = rtcElapsedOffset
                        .Subtract(TimeSpan.FromHours(rtcElapsedOffset.Hours))
                        .Add(TimeSpan.FromHours(value));
                    rtcStopwatch.Restart();
                    break;
                case RtcRegister.Days:
                    rtcElapsedOffset = rtcElapsedOffset
                        .Subtract(TimeSpan.FromDays((int)rtcElapsedOffset.TotalDays))
                        .Add(TimeSpan.FromDays(value));
                    rtcStopwatch.Restart();
                    break;
                case RtcRegister.Control:
                    value &= 0b1100_0001;
                    if ((value & 0x40) != 0) // bit 6 for halt
                    {
                        Update();
                        rtcStopwatch.Stop();
                    }
                    else if (!rtcStopwatch.IsRunning)
                    {
                        rtcStopwatch.Start();
                    }
                    break;
            }

            rtcRegisters[register] = value;
        }

        public byte ReadFromRegister(RtcRegister register)
        {
            Update();
            var value = latchedRtcRegisters?[register] ?? rtcRegisters[register];
            switch (register)
            {
                case RtcRegister.Seconds:
                case RtcRegister.Minutes:
                    return (byte)(value & 0b0011_1111);
                case RtcRegister.Hours:
                    return (byte)(value & 0b0001_1111);
                case RtcRegister.Days:
                    return value;
                case RtcRegister.Control:
                    return (byte)(value & 0b1100_0001);
                default:
                    return value;
            }
        }

        private void Update()
        {
            if (!rtcStopwatch.IsRunning)
            {
                return;
            }

            // Using Stopwatch's Elapsed properties
            var elapsed = rtcStopwatch.Elapsed.Add(rtcElapsedOffset);
            rtcRegisters[RtcRegister.Seconds] = (byte)elapsed.Seconds;
            rtcRegisters[RtcRegister.Minutes] = (byte)elapsed.Minutes;
            rtcRegisters[RtcRegister.Hours] = (byte)elapsed.Hours;
            int totalDays = (int)elapsed.TotalDays;
            int days = totalDays;

            if (totalDays >= 512)
            {
                days = totalDays % 512;
                rtcRegisters[RtcRegister.Control] |= 0x80; // Sets the overflow bit in the control register
            }
            if (days >= 256)
            {
                days %= 256;
                rtcRegisters[RtcRegister.Control] |= 0x01; // Sets the 9th bit in the control register
            }
            rtcRegisters[RtcRegister.Days] = (byte)days;
        }
    }

    public enum RtcRegister
    {
        None = 0,
        Seconds = 0x08,
        Minutes = 0x09,
        Hours = 0x0a,
        Days = 0x0b,
        Control = 0x0c
    }
}
