using SharpBoy.Core.Memory;
using SharpBoy.Core.Rendering;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpBoy.Core.Graphics
{

    public class Ppu : IPpu
    {
        public PpuRegisters Registers { get; } = new PpuRegisters();

        private const int LcdWidth = 160;
        private const int LcdHeight = 144;
        private static readonly ColorRgb[] Colors =
        {
            new ColorRgb(154, 158, 63),
            new ColorRgb(73, 107, 34),
            new ColorRgb(14, 69, 11),
            new ColorRgb(27, 42, 9)
        };

        private IReadWriteMemory vram = new Ram(0x2000);
        private IReadWriteMemory oam = new Ram(0xa0);
        private byte[] frameBuffer = new byte[LcdWidth * LcdHeight * 3];

        private int cycles;
        private readonly IInterruptManager interruptManager;
        private readonly IRenderQueue renderQueue;

        public Ppu(IInterruptManager interruptManager, IRenderQueue renderQueue)
        {
            this.interruptManager = interruptManager;
            this.renderQueue = renderQueue;
        }

        public void Tick()
        {
            var previousStatus = Registers.CurrentStatus;
            cycles += 4;

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
                    renderQueue.Enqueue(frameBuffer);
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

        public byte ReadVram(ushort address) => vram.Read(address);

        public byte ReadOam(ushort address) => oam.Read(address);

        public void WriteVram(ushort address, byte value) => vram.Write(address, value);

        public void WriteOam(ushort address, byte value) => oam.Write(address, value);

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

        private void RenderScanline()
        {
            if (!Registers.LCDC.HasFlag(LcdcFlags.LcdEnable))
            {
                return;
            }

            var line = Registers.LY;

            int tileDataAddress = Registers.LCDC.HasFlag(LcdcFlags.TileDataArea) ? 0x8000 : 0x8800;
            int bgMapAddress = Registers.LCDC.HasFlag(LcdcFlags.BgTileMapArea) ? 0x9C00 : 0x9800;

            int yPos = line + Registers.SCY; // y position of the current scanline
            int tileRow = (yPos / 8) * 32; // Each tile is 8x8 pixels, and each row has 32 tiles

            for (int pixel = 0; pixel < LcdWidth; pixel++)
            {
                int xPos = pixel + Registers.SCX;
                int tileCol = xPos / 8;

                int tileNum;
                int tileAddress = bgMapAddress + tileRow + tileCol;

                if (tileDataAddress == 0x8000)
                {
                    tileNum = vram.Read(tileAddress);
                }
                else
                {
                    tileNum = (sbyte)vram.Read(tileAddress); // signed
                }

                int tileLocation = tileDataAddress + (tileNum * 16);
                int lineOffset = (yPos % 8) * 2; // Each line in a tile takes up 2 bytes

                byte data1 = vram.Read(tileLocation + lineOffset);
                byte data2 = vram.Read(tileLocation + lineOffset + 1);

                // Find the correct pixel within the tile
                int colorBitIndex = 7 - (xPos % 8);

                // Combine data from two bytes to get the color index
                int colorIndex = ((data2 >> colorBitIndex) & 1) << 1;
                colorIndex |= (data1 >> colorBitIndex) & 1;

                // Set pixel color in the screen buffer
                var color = GetColor(colorIndex);
                var pixelPosition = ((line * LcdWidth) + pixel) * 3;
                frameBuffer[pixelPosition] = color.Red;
                frameBuffer[pixelPosition + 1] = color.Green;
                frameBuffer[pixelPosition + 2] = color.Blue;
            }
        }

        private ColorRgb GetColor(int pixelIndex)
        {
            var colorValue = Registers.BGP >>> (pixelIndex * 2) & 3;
            return Colors[colorValue];
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

        private struct ColorRgb
        {
            public byte Red;
            public byte Green;
            public byte Blue;

            public ColorRgb(byte red, byte green, byte blue)
            {
                Red = red;
                Green = green;
                Blue = blue;
            }
        }
    }
}
