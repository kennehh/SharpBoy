using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Video
{
    internal class PpuRegisters
    {
        public byte STAT
        {
            get => (byte)((byte)StatInterruptSource | (byte)CurrentStatus | (LyCompareFlag.ToBit() << 2));
            set
            {
                StatInterruptSource = (StatInterruptSourceFlag)(value & 0b01111000);
                // LYEqualsLYC = BitUtils.IsBitSet(value, 2);
                CurrentStatus = (PpuStatus)(value & 0b00000011);
            }
        }

        public StatInterruptSourceFlag StatInterruptSource = StatInterruptSourceFlag.None;
        public bool LyCompareFlag => LY == LYC;
        public PpuStatus CurrentStatus = PpuStatus.HorizontalBlank;

        public byte LCDC = 0;
        public byte LY = 0;
        public byte LYC = 0;

        public byte SCY = 0;
        public byte SCX = 0;

        public byte WY = 0;
        public byte WX = 0;

        public byte BGP = 0;

        public byte OBP0 = 0;
        public byte OBP1 = 0;

        public byte DMA = 0;
    }
}
