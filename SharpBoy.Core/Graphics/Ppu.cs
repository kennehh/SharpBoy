using SharpBoy.Core.Interrupts;
using SharpBoy.Core.Memory;
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
        private byte[] frameBuffer = new byte[LcdWidth * LcdHeight * 4];
        private byte[] blankFrameBuffer = new byte[LcdWidth * LcdHeight * 4];

        private int cycles;
        private readonly IInterruptManager interruptManager;
        private readonly IRenderQueue renderQueue;
        private readonly TileMapManager tileMapManager;
        private readonly SpriteManager spriteManager;
        private bool LastLcdEnabledStatus = false;

        public Ppu(IInterruptManager interruptManager, IRenderQueue renderQueue)
        {
            this.interruptManager = interruptManager;
            this.renderQueue = renderQueue;
            tileMapManager = new TileMapManager(vram);
            spriteManager = new SpriteManager(oam, vram);

            for (int y = 0; y < LcdHeight; y ++)
            {
                for (int x = 0; x < LcdWidth; x++)
                {
                    int bufferPosition = ((y * LcdWidth) + x) * 4;

                    blankFrameBuffer[bufferPosition] = TransparentColor.Red;
                    blankFrameBuffer[bufferPosition + 1] = TransparentColor.Green;
                    blankFrameBuffer[bufferPosition + 2] = TransparentColor.Blue;
                    blankFrameBuffer[bufferPosition + 3] = 0xff;
                }
            }
        }

        

        public void Tick()
        {
            if (!Registers.LCDC.HasFlag(LcdcFlags.LcdEnable))
            {
                if (LastLcdEnabledStatus)
                {
                    LastLcdEnabledStatus = false;
                    ResetLcdState();
                }
                return;
            }

            LastLcdEnabledStatus = true;
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
                case 0xff45: UpdateLyc(value); break;
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

        public void DoOamDmaTransfer(byte[] sourceData)
        {
            oam.Copy(sourceData);
        }

        private void ResetLcdState()
        {
            Registers.LY = 0;
            cycles = 0;
            Registers.CurrentStatus = PpuStatus.HorizontalBlank;
            renderQueue.Enqueue(blankFrameBuffer);
        }

        private void UpdateLcdc(byte newValue)
        {
            Registers.LCDC = (LcdcFlags)newValue;
            tileMapManager.SetActiveBgTileMap(Registers.LCDC.HasFlag(LcdcFlags.BgTileMapArea));
            tileMapManager.SetActiveWindowTileMap(Registers.LCDC.HasFlag(LcdcFlags.WindowTileMapArea));
            tileMapManager.SetActiveTileData(Registers.LCDC.HasFlag(LcdcFlags.TileDataArea));
        }

        private void UpdateLyc(byte newValue)
        {
            Registers.LYC = newValue;
            CheckLyEqualsLycInterrupt();
        }

        private void IncrementLy()
        {
            if (Registers.LY >= 153)
            {
                Registers.LY = 0;
            }
            else
            {
                Registers.LY++;
            }
            CheckLyEqualsLycInterrupt();
        }

        private void CheckLyEqualsLycInterrupt()
        {
            if (Registers.LyCompareFlag)
            {
                HandleStatInterrupt(StatInterruptSourceFlags.LyEqualsLyc);
            }
        }

        private void HandleModeSwitching(PpuStatus previousStatus)
        {
            switch (cycles)
            {
                case < 80:
                    Registers.CurrentStatus = PpuStatus.SearchingOam;
                    HandleStatInterrupt(StatInterruptSourceFlags.SearchingOam);
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
                    HandleStatInterrupt(StatInterruptSourceFlags.HorizontalBlank);
                    break;
                default:
                    IncrementLy();
                    cycles -= 456;
                    break;
            }
        }

        private void HandleVBlank(PpuStatus previousStatus)
        {
            Registers.CurrentStatus = PpuStatus.VerticalBlank;
            HandleStatInterrupt(StatInterruptSourceFlags.VerticalBlank);

            if (Registers.CurrentStatus != previousStatus)
            {
                interruptManager.RequestInterrupt(InterruptFlags.VBlank);
                renderQueue.Enqueue(frameBuffer);
            }

            if (cycles >= 456)
            {
                IncrementLy();
                cycles -= 456;
            }
        }

        private void RenderScanline()
        {
            RenderBgScanline();
            RenderWindowScanline();
            RenderSpritesScanline();
        }

        private void RenderBgScanline()
        {
            var line = Registers.LY;

            // Calculate the adjusted Y position accounting for vertical scroll
            int yPos = (line + Registers.SCY) & 0xFF;

            // Iterate through each pixel in the current scanline
            for (int pixel = 0; pixel < LcdWidth; pixel++)
            {
                // Calculate the adjusted X position accounting for horizontal scroll
                int xPos = (pixel + Registers.SCX) & 0xFF;

                // Fetch the color index for the given pixel from the active tilemap
                var colorIndex = tileMapManager.GetBgColorIndex(xPos, yPos);

                // Get the actual RGB color using the fetched color index
                var color = BgpColorMap[colorIndex];

                // Fill the framebuffer with the RGB values
                DrawPixel(pixel, line, color);
            }
        }

        private void RenderSpritesScanline()
        {
            if (!Registers.LCDC.HasFlag(LcdcFlags.ObjEnable))
            {
                return;
            }

            var spriteHeight = Registers.LCDC.HasFlag(LcdcFlags.ObjSize) ? 16 : 8;
            var currentScanline = Registers.LY;

            var visibleSprites = spriteManager.GetVisibleSprites(Registers.LY, spriteHeight);

            foreach (var sprite in visibleSprites)
            {
                int lineToRender = currentScanline - (sprite.YPos - 16);
                var lineData = sprite.GetLineToRender(lineToRender, spriteHeight);

                for (int i = 0; i < lineData.Length; i++)
                {
                    var index = lineData[i];
                    if (index == 0)
                    {
                        continue;
                    }

                    int screenX = sprite.XPos + i - 8;
                    int screenY = sprite.YPos + lineToRender - 16;

                    if (screenX < 0 || screenY < 0 || screenX >= LcdWidth || screenY >= LcdHeight)
                    {
                        continue;
                    }

                    if (sprite.BgAndWindowHasPriority)
                    {
                        var bgIndex = GetBgPixelColorIndex(screenX, screenY);
                        if (bgIndex != 0)
                        {
                            continue;
                        }
                    }

                    var color = GetSpriteColor(index, sprite.UseObp1Palette);
                    DrawPixel(screenX, screenY, color);
                }
            }
        }

        private void RenderWindowScanline()
        {
            if (!Registers.LCDC.HasFlag(LcdcFlags.WindowEnable))
            {
                return;
            }

            var line = Registers.LY;

            int windowX = Registers.WX - 7;
            int windowY = Registers.WY;

            if (line < windowY)
            {
                return;
            }

            int yPos = line - windowY;

            for (int pixel = 0; pixel < LcdWidth; pixel++)
            {
                int xPos = pixel;

                if (xPos < windowX)
                {
                    continue;
                }

                var colorIndex = tileMapManager.GetWindowColorIndex(xPos - windowX, yPos);
                var color = BgpColorMap[colorIndex];

                DrawPixel(pixel, line, color);
            }
        }

        private void DrawPixel(int x, int y, ColorRgb color)
        {
            int bufferPosition = ((y * LcdWidth) + x) * 4;

            frameBuffer[bufferPosition] = color.Red;
            frameBuffer[bufferPosition + 1] = color.Green;
            frameBuffer[bufferPosition + 2] = color.Blue;
            frameBuffer[bufferPosition + 3] = 0xff;
        }

        private ColorRgb GetPixelColor(int x, int y)
        {
            int bufferPosition = ((y * LcdWidth) + x) * 4;

            var red = frameBuffer[bufferPosition];
            var green = frameBuffer[bufferPosition + 1];
            var blue = frameBuffer[bufferPosition + 2];

            return new ColorRgb(red, green, blue);
        }

        private int GetBgPixelColorIndex(int x, int y)
        {
            return Array.IndexOf(BgpColorMap, GetPixelColor(x, y));
        }

        private ColorRgb GetSpriteColor(int colorIndex, bool useObp1)
        {
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

        private void HandleStatInterrupt(StatInterruptSourceFlags flagToCheck)
        {
            if (Registers.StatInterruptSource != StatInterruptSourceFlags.None && Registers.StatInterruptSource.HasFlag(flagToCheck))
            {
                interruptManager.RequestInterrupt(InterruptFlags.LcdStat);
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
