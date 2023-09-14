using Raylib_cs;
using SharpBoy.Core.Processor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpBoy.Core.Video
{

    internal class Ppu : IPpu
    {
        internal PpuRegisters Registers { get; }
        public  ReadOnlySpan<PixelValue> FrameBuffer => frameBuffer;

        private byte[] vram = new byte[0x2000];
        private byte[] oam = new byte[0xa0];
        private PixelValue[] frameBuffer = new PixelValue[160 * 144];

        private int cycles;
        private readonly IInterruptManager interruptManager;

        public Ppu(IInterruptManager interruptManager)
        {
            Registers = new PpuRegisters();
            this.interruptManager = interruptManager;
        }

        public void Sync(int cpuCyles)
        {
            var previousStatus = Registers.CurrentStatus;
            cycles += cpuCyles;            

            if (Registers.LY <= 143)
            {
                switch (cycles)
                {
                    case < 80:
                        Registers.CurrentStatus = PpuStatus.SearchingOam;                        
                        break;
                    case < 252:
                        // could take from 172 to 289 cycles, defaulting to 172 for now
                        Registers.CurrentStatus = PpuStatus.Drawing;
                        if (Registers.CurrentStatus != previousStatus)
                        {
                            RenderScanline();
                        }
                        break;
                    case < 456:
                        // could take from 87 to 204 cycles, defaulting to 204 for now
                        Registers.CurrentStatus = PpuStatus.HorizontalBlank;
                        break;
                    default:
                        Registers.LY++;
                        cycles -= 456;
                        break;
                }
            }
            else
            {
                Registers.CurrentStatus = PpuStatus.VerticalBlank;
                if (Registers.CurrentStatus != previousStatus)
                {
                    interruptManager.RequestInterrupt(InterruptFlag.VBlank);
                }

                if (cycles >= 456)
                {
                    if (Registers.LY >= 153)
                    {
                        Registers.LY = 0;
                    }
                    else
                    {
                        Registers.LY++;
                    }
                    cycles -= 456;
                }
            }

            HandleStatInterrupt();
        }

        private void HandleStatInterrupt()
        {
            if (Registers.StatInterruptSource != StatInterruptSourceFlag.None)
            {
                StatInterruptSourceFlag? interrupt = null;

                if (Registers.CurrentStatus == PpuStatus.HorizontalBlank && Registers.StatInterruptSource.HasFlag(StatInterruptSourceFlag.HorizontalBlank))
                {
                    interrupt = StatInterruptSourceFlag.HorizontalBlank;
                }
                else if (Registers.CurrentStatus == PpuStatus.VerticalBlank && Registers.StatInterruptSource.HasFlag(StatInterruptSourceFlag.VerticalBlank))
                {
                    interrupt = StatInterruptSourceFlag.VerticalBlank;
                }
                else if (Registers.CurrentStatus == PpuStatus.SearchingOam && Registers.StatInterruptSource.HasFlag(StatInterruptSourceFlag.SearchingOam))
                {
                    interrupt = StatInterruptSourceFlag.SearchingOam;
                }
                else if (Registers.LyCompareFlag && Registers.StatInterruptSource.HasFlag(StatInterruptSourceFlag.LyEqualsLyc))
                {
                    interrupt = StatInterruptSourceFlag.LyEqualsLyc;
                }

                if (interrupt.HasValue)
                {
                    Registers.StatInterruptSource &= ~interrupt.Value;
                    interruptManager.RequestInterrupt(InterruptFlag.LcdStat);
                }
            }
        }

        public byte ReadVram(ushort address)
        {
            return vram[address];
        }

        public byte ReadOam(ushort address)
        {
            return oam[address];
        }

        public void WriteVram(ushort address, byte value)
        {
            vram[address] = value;
        }

        public void WriteOam(ushort address, byte value)
        {
            oam[address] = value;
        }

        public byte ReadRegister(ushort address)
        {
            return address switch
            {
                0xff40 => (byte)Registers.LCDC,
                0xff41 => Registers.STAT,
                0xff42 => Registers.SCY,
                0xff43 => Registers.SCX,
                0xff44 => Registers.LY,
                0xff45 => Registers.LYC,
                0xff46 => Registers.DMA,
                0xff47 => Registers.BGP,
                0xff48 => Registers.OBP0,
                0xff49 => Registers.OBP1,
                0xff4a => Registers.WY,
                0xff4b => Registers.WX,
                _ => throw new NotImplementedException(),
            };
        }

        public void WriteRegister(ushort address, byte value)
        {
            switch (address)
            {
                case 0xff40: Registers.LCDC = (LcdcFlags)value; break;
                case 0xff41: Registers.STAT = BitUtils.SetBits(Registers.STAT, value, 0b01111000); break;
                case 0xff42: Registers.SCY = value; break;
                case 0xff43: Registers.SCX = value; break;
                case 0xff44: break;
                case 0xff45: Registers.LYC = value; break;
                case 0xff46: Registers.DMA = value; break;
                case 0xff47: Registers.BGP = value; break;
                case 0xff48: Registers.OBP0 = value; break;
                case 0xff49: Registers.OBP1 = value; break;
                case 0xff4a: Registers.WY = value; break;
                case 0xff4b: Registers.WX = value; break;
                default: throw new NotImplementedException();
            }
        }

        private void RenderScanline2()
        {
            var y = Registers.LY;
            var tileMapAddress = Registers.LCDC.HasFlag(LcdcFlags.BgTileMapArea) ? 0x9c00 : 0x9800;

            for (var x = 0; x < 160; x++)
            {
                var effectiveY = y + Registers.SCY;
                var effectiveX = x + Registers.SCX;

                var tileIdAddress = tileMapAddress + (effectiveY / 8 * 32) + (effectiveY / 8);
                var tileId = vram[tileIdAddress & 0x1fff];

                int tileDataAddress;
                if(!Registers.LCDC.HasFlag(LcdcFlags.TileDataArea))
                {
                    var tileDataStartAddress = 0x8000;
                    tileDataAddress = tileDataStartAddress + (tileId * 16) + (effectiveY % 8) * 2;
                }
                else
                {
                    var tileDataStartAddress = 0x8800;
                    tileDataAddress = tileDataStartAddress + ((sbyte)tileId * 16) + (effectiveY % 8) * 2;
                }

                var tileValue = BitUtils.Get16BitValue(vram[tileDataAddress & 0x1fff + 1], vram[tileDataAddress & 0x1fff]);

                tileDataAddress &= 0x1fff;
                byte lowByte = vram[tileDataAddress];
                byte highByte = vram[tileDataAddress + 1];

                // Determine the color
                int pixelBitIndex = 7 - (effectiveX % 8);
                int colorNum = ((highByte >> pixelBitIndex) & 1) << 1 | ((lowByte >> pixelBitIndex) & 1);
                var color = (PixelValue)colorNum;

                // Draw pixel to the framebuffer
                frameBuffer[y * 160 + x] = color;
            }
        }

        private void RenderScanline()
        {
            var y = Registers.LY;

            // Assume a 160x144 screen size (the Game Boy's screen size)
            for (int x = 0; x < 160; x++)
            {
                // Calculate the effective coordinates, considering the scroll
                int effectiveY = (y + Registers.SCY) % 256;
                int effectiveX = (x + Registers.SCX) % 256;

                // Calculate tile row and column
                int tileRow = effectiveY / 8;
                int tileCol = effectiveX / 8;

                // Fetch tile ID from tile map
                var tileMapStartAddress = Registers.LCDC.HasFlag(LcdcFlags.BgTileMapArea) ? 0x9C00 : 0x9800;
                var tileAddress = (tileMapStartAddress + tileRow * 32 + tileCol) & 0x1fff;
                byte tileID = vram[tileAddress + tileRow * 32 + tileCol];

                // Fetch pixel data from tile data
                int tileDataAddress;

                if (!Registers.LCDC.HasFlag(LcdcFlags.TileDataArea))
                {
                    var tileDataStartAddress = 0x8000;
                    tileDataAddress = tileDataStartAddress + (tileID * 16) + (effectiveY % 8) * 2;
                }
                else
                {
                    var tileDataStartAddress = 0x8800;
                    tileDataAddress = tileDataStartAddress + ((sbyte)tileID * 16) + (effectiveY % 8) * 2;
                }

                tileDataAddress &= 0x1fff;
                byte lowByte = vram[tileDataAddress];
                byte highByte = vram[tileDataAddress + 1];

                // Determine the color
                int pixelBitIndex = 7 - (effectiveX % 8);
                int colorNum = ((highByte >> pixelBitIndex) & 1) << 1 | ((lowByte >> pixelBitIndex) & 1);
                var color = (PixelValue)colorNum;

                // Draw pixel to the framebuffer
                frameBuffer[y * 160 + x] = color;
            }
        }
    }

    public enum PixelValue : byte
    {
        Zero = 1 << 0,
        One = 1 << 1,
        Two = 1 << 2,
        Three = 1 << 3
    }

    public struct ColorRgb
    {
        public byte Red;
        public byte Green;
        public byte Blue;
    }
}
