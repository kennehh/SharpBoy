using SharpBoy.Core.Interrupts;
using SharpBoy.Core.Memory;
using SharpBoy.Core.Rendering;
using SharpBoy.Core.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
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

        private static readonly ColorRgb TransparentColor = Colors[0];
        private static readonly ColorRgb[] BgpColorMap = (ColorRgb[])Colors.Clone();
        private static readonly ColorRgb[] Obp0ColorMap = (ColorRgb[])Colors.Clone();
        private static readonly ColorRgb[] Obp1ColorMap = (ColorRgb[])Colors.Clone();

        private IReadWriteMemory vram = new Ram(0x2000);
        private IReadWriteMemory oam = new Ram(0xa0);
        private byte[] frameBuffer = new byte[LcdWidth * LcdHeight * 3];

        private int cycles;
        private readonly IInterruptManager interruptManager;
        private readonly IRenderQueue renderQueue;
        private readonly TileMaps tileMaps;

        public Ppu(IInterruptManager interruptManager, IRenderQueue renderQueue)
        {
            this.interruptManager = interruptManager;
            this.renderQueue = renderQueue;
            tileMaps = new TileMaps(vram);
        }

        public void Tick()
        {
            var previousStatus = Registers.CurrentStatus;
            cycles += 4;

            if (Registers.LY <= 143)
            {
                HandleModeSwitching(previousStatus);
            }
            else
            {
                HandleVBlank(previousStatus);
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
                case 0xff40: UpdateLcdc(value); break;
                case 0xff41: Registers.STAT = value; break;
                case 0xff42: Registers.SCY = value; break;
                case 0xff43: Registers.SCX = value; break;
                case 0xff44: break;
                case 0xff45: Registers.LYC = value; break;
                case 0xff46: Registers.DMA = value; break;
                case 0xff47:
                    Registers.BGP = value;
                    UpdateColorMap(BgpColorMap, value); 
                    break;
                case 0xff48:
                    Registers.OBP0 = value;
                    UpdateColorMap(Obp0ColorMap, value); 
                    break;
                case 0xff49:
                    Registers.OBP1 = value;
                    UpdateColorMap(Obp1ColorMap, value); 
                    break;
                case 0xff4a: Registers.WY = value; break;
                case 0xff4b: Registers.WX = value; break;
                default: throw new NotImplementedException();
            }
        }

        private void UpdateLcdc(byte newValue)
        {
            Registers.LCDC = (LcdcFlags)newValue;
            tileMaps.SetActiveTileMap(Registers.LCDC.HasFlag(LcdcFlags.BgTileMapArea));
            tileMaps.ActiveTileMap.SetActiveTileData(Registers.LCDC.HasFlag(LcdcFlags.TileDataArea));
        }

        private void HandleModeSwitching(PpuStatus previousStatus)
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
                        RenderBgScanline();
                        RenderSprites();
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

        private void HandleVBlank(PpuStatus previousStatus)
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

        private void RenderBgScanline()
        {
            // Get the current line being rendered
            var line = Registers.LY;

            // Calculate the base position in the framebuffer for this scanline
            int pixelPositionBase = line * LcdWidth * 3;

            // If the LCD is disabled, fill the framebuffer with the transparent color for this scanline
            if (!Registers.LCDC.HasFlag(LcdcFlags.LcdEnable))
            {
                for (int pixel = 0; pixel < LcdWidth; pixel++)
                {
                    var pixelPosition = pixelPositionBase + pixel * 3;

                    frameBuffer[pixelPosition] = TransparentColor.Red;
                    frameBuffer[pixelPosition + 1] = TransparentColor.Green;
                    frameBuffer[pixelPosition + 2] = TransparentColor.Blue;
                }
                return; // Exit early since LCD is disabled
            }

            // Calculate the adjusted Y position accounting for vertical scroll
            int yPos = (line + Registers.SCY) & 0xFF;

            // Iterate through each pixel in the current scanline
            for (int pixel = 0; pixel < LcdWidth; pixel++)
            {
                var pixelPosition = pixelPositionBase + pixel * 3;

                // Calculate the adjusted X position accounting for horizontal scroll
                int xPos = (pixel + Registers.SCX) & 0xFF;

                // Fetch the color index for the given pixel from the active tilemap
                var colorIndex = tileMaps.ActiveTileMap.GetColorIndex(xPos, yPos);

                // Get the actual RGB color using the fetched color index
                var color = BgpColorMap[colorIndex];

                // Fill the framebuffer with the RGB values
                frameBuffer[pixelPosition] = color.Red;
                frameBuffer[pixelPosition + 1] = color.Green;
                frameBuffer[pixelPosition + 2] = color.Blue;
            }
        }

        private void RenderSprites()
        {
            if (!Registers.LCDC.HasFlag(LcdcFlags.ObjEnable))
            {
                return;
            }

            int spriteHeight = Registers.LCDC.HasFlag(LcdcFlags.ObjSize) ? 16 : 8;

            for (int i = 0; i < 40; i++) // Game Boy has 40 sprites in OAM
            {
                int index = i * 4; // Each sprite has 4 bytes of data in OAM
                byte yPos = oam.Read(index);
                byte xPos = oam.Read(index + 1);
                byte tileNumber = oam.Read(index + 2);
                byte attributes = oam.Read(index + 3);

                bool yFlip = (attributes & 0x40) != 0;
                bool xFlip = (attributes & 0x20) != 0;
                // ... other flags can be read from attributes as needed

                int yPositionCorrected = yPos - 16;
                int xPositionCorrected = xPos - 8;

                // Check if this sprite intersects with the current scanline
                if (Registers.LY >= yPositionCorrected && Registers.LY < (yPositionCorrected + spriteHeight))
                {
                    int line = yFlip ? (yPositionCorrected + spriteHeight - 1 - Registers.LY) : (Registers.LY - yPositionCorrected);
                    RenderSpriteLine(xPositionCorrected, line, tileNumber, xFlip, attributes);
                }
            }
        }

        private void RenderSpriteLine(int x, int line, byte tileNumber, bool xFlip, byte attributes)
        {
            // Get the tile pattern data for this line of the sprite
            int tileDataAddress = 0x8000 + (tileNumber * 16) + (line * 2);
            byte data1 = vram.Read(tileDataAddress);
            byte data2 = vram.Read(tileDataAddress + 1);

            for (int tilePixel = 0; tilePixel < 8; tilePixel++)
            {
                int colorBit = xFlip ? tilePixel : 7 - tilePixel;
                int colorIndex = ((data2 >> colorBit) & 1) << 1;
                colorIndex |= (data1 >> colorBit) & 1;

                // Convert color index to actual color and merge with background
                var color = GetSpriteColor(colorIndex, attributes);

                if (color != TransparentColor)
                {
                    int bufferPosition = ((Registers.LY * LcdWidth) + x + tilePixel) * 3;

                    frameBuffer[bufferPosition] = color.Red;
                    frameBuffer[bufferPosition + 1] = color.Green;
                    frameBuffer[bufferPosition + 2] = color.Blue;
                }
            }
        }

        private ColorRgb GetSpriteColor(int colorIndex, byte attributes)
        {
            bool useObp1 = (attributes & 0x10) != 0;  // Check if OBP1 palette is used.
            return useObp1 ? Obp1ColorMap[colorIndex] : Obp0ColorMap[colorIndex];
        }

        private void UpdateColorMap(ColorRgb[] colorMap, byte registerValue)
        {
            for (int i = 0; i < 4; i++)
            {
                var colorValue = registerValue >>> (i * 2) & 3;
                colorMap[i] = Colors[colorValue];
            }
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
            public byte Red { get; }
            public byte Green { get; }
            public byte Blue { get; }

            public ColorRgb(byte red, byte green, byte blue)
            {
                Red = red;
                Green = green;
                Blue = blue;
            }

            public override bool Equals(object obj)
            {
                return obj is ColorRgb other && this == other;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Red, Green, Blue);
            }

            public static bool operator ==(ColorRgb x, ColorRgb y)
            {
                return x.Red == y.Red && x.Blue == y.Blue && x.Green == y.Green;
            }

            public static bool operator !=(ColorRgb x, ColorRgb y)
            {
                return !(x == y);
            }
        }
    }
}
