using Microsoft.Win32;
using SharpBoy.Core.Graphics.Interfaces;
using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    public class PpuRenderer : IPpuRenderer
    {
        private const int LcdWidth = 160;
        private const int LcdHeight = 144;

        private static readonly ColorRgb[] Colors =
        {
            new ColorRgb(0xff, 0xff, 0xff),
            new ColorRgb(0xaa, 0xaa, 0xaa),
            new ColorRgb(0x55, 0x55, 0x55),
            new ColorRgb(0x00, 0x00, 0x00)
        };

        private static readonly ColorRgb[] BgpColorMap = (ColorRgb[])Colors.Clone();
        private static readonly ColorRgb[] Obp0ColorMap = (ColorRgb[])Colors.Clone();
        private static readonly ColorRgb[] Obp1ColorMap = (ColorRgb[])Colors.Clone();

        private byte[] frameBuffer = new byte[LcdWidth * LcdHeight * 4];
        private int windowLineCounter = 0;

        private readonly TileMapManager tileMapManager;
        private readonly SpriteManager spriteManager;
        private readonly IFrameBufferManager fbManager;

        public PpuRenderer(IFrameBufferManager fbManager, IPpuMemory memory)
        {
            tileMapManager = new TileMapManager(memory.Vram);
            spriteManager = new SpriteManager(memory.Oam, memory.Vram);
            this.fbManager = fbManager;
        }

        public void RenderScanline(PpuRegisters registers)
        {
            if (registers.LCDC.HasFlag(LcdcFlags.BgWindowPriority))
            {
                RenderBgScanline(registers);
                RenderWindowScanline(registers);
            }
            RenderSpritesScanline(registers);
        }

        public void PushFrame()
        {
            fbManager.PushFrame(frameBuffer);
        }

        public void UpdateBgColorMapping(byte value)
        {
            UpdateColorMap(BgpColorMap, value);
        }

        public void UpdateObp0ColorMapping(byte value)
        {
            UpdateColorMap(Obp0ColorMap, value);
        }

        public void UpdateObp1ColorMapping(byte value)
        {
            UpdateColorMap(Obp1ColorMap, value);
        }

        public void ResetWindowLineCounter()
        {
            windowLineCounter = 0;
        }

        public void SetActiveTileMapAndTileData(LcdcFlags lcdc)
        {
            tileMapManager.SetActiveBgTileMap(lcdc.HasFlag(LcdcFlags.BgTileMapArea));
            tileMapManager.SetActiveWindowTileMap(lcdc.HasFlag(LcdcFlags.WindowTileMapArea));
            tileMapManager.SetActiveTileData(lcdc.HasFlag(LcdcFlags.TileDataArea));
        }

        public void ClearBuffers()
        {
            fbManager.ClearBuffers();
        }

        private void RenderBgScanline(PpuRegisters registers)
        {
            var line = registers.LY;

            // Calculate the adjusted Y position accounting for vertical scroll
            int yPos = (line + registers.SCY) & 0xFF;

            // Iterate through each pixel in the current scanline
            for (int pixel = 0; pixel < LcdWidth; pixel++)
            {
                // Calculate the adjusted X position accounting for horizontal scroll
                int xPos = (pixel + registers.SCX) & 0xFF;

                // Fetch the color index for the given pixel from the active tilemap
                var colorIndex = tileMapManager.GetBgColorIndex(xPos, yPos);

                // Get the actual RGB color using the fetched color index
                var color = BgpColorMap[colorIndex];

                // Fill the framebuffer with the RGB values
                DrawPixel(pixel, line, color);
            }
        }

        private void RenderSpritesScanline(PpuRegisters registers)
        {
            if (!registers.LCDC.HasFlag(LcdcFlags.ObjEnable))
            {
                return;
            }

            var spriteHeight = registers.LCDC.HasFlag(LcdcFlags.ObjSize) ? 16 : 8;
            var currentScanline = registers.LY;

            var visibleSprites = spriteManager.GetVisibleSprites(registers.LY, spriteHeight);

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

        private void RenderWindowScanline(PpuRegisters registers)
        {
            if (!registers.LCDC.HasFlag(LcdcFlags.WindowEnable))
            {
                return;
            }

            var line = registers.LY;

            int windowX = registers.WX - 7;
            int windowY = registers.WY;

            if (line < windowY)
            {
                return;
            }

            var incrementLineCounter = false;

            for (int pixel = 0; pixel < LcdWidth; pixel++)
            {
                int xPos = pixel;

                if (xPos < windowX)
                {
                    continue;
                }

                var colorIndex = tileMapManager.GetWindowColorIndex(xPos - windowX, windowLineCounter);
                var color = BgpColorMap[colorIndex];

                DrawPixel(pixel, line, color);
                incrementLineCounter = true;
            }

            // If the window is active on this line, increment the counter
            if (incrementLineCounter)
            {
                windowLineCounter++;
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
