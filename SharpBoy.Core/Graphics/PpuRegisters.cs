﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    internal class PpuRegisters
    {
        public byte STAT
        {
            get => (byte)((byte)StatInterruptSource | (LyCompareFlag.ToBit() << 2) | (byte)CurrentStatus);
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

        public LcdcFlags LCDC = 0;

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

    [Flags]
    internal enum LcdcFlags
    {
        LcdEnable = 1 << 7,
        WindowTileMapArea = 1 << 6,
        WindowEnable = 1 << 5,
        TileDataArea = 1 << 4,
        BgTileMapArea = 1 << 3,
        ObjSize = 1 << 2,
        ObjEnable = 1 << 1,
        BgWindowPriority = 1 << 0,
    }

    internal enum PpuStatus : byte
    {
        HorizontalBlank = 0,
        VerticalBlank = 1,
        SearchingOam = 2,
        Drawing = 3,
    }

    [Flags]
    internal enum StatInterruptSourceFlag : byte
    {
        None = 0,
        HorizontalBlank = 1 << 3,
        VerticalBlank = 1 << 4,
        SearchingOam = 1 << 5,
        LyEqualsLyc = 1 << 6
    }
}