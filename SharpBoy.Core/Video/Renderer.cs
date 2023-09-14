using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Video
{
    internal class Renderer : IRenderer, IDisposable
    {
        public void OpenWindow()
        {
            Raylib.InitWindow(160, 144, "Hello World");
            //Raylib.SetTargetFPS(60);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.WHITE);
            Raylib.EndDrawing();
        }

        public bool IsWindowOpen() => !Raylib.WindowShouldClose();

        public void CloseWindow() => Raylib.CloseWindow();

        public void Render(ReadOnlySpan<PixelValue> frameBuffer)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.WHITE);

            //Raylib.DrawText(Raylib.GetFPS().ToString(), 12, 40, 20, Color.BLACK);

            RenderFrameBuffer(frameBuffer);

            Raylib.EndDrawing();
        }

        private void RenderFrameBuffer(ReadOnlySpan<PixelValue> frameBuffer)
        {
            for (int y = 0; y < 144; y++)
            {
                for (int x = 0; x < 160; x++)
                {
                    Raylib.DrawPixel(x, y, ConvertPixelValueToColor(frameBuffer[y * 144 + x]));
                }
            }
        }

        private Color ConvertPixelValueToColor(PixelValue value) => value switch
        {
            PixelValue.Zero => new Color(15, 56, 15, 255),
            PixelValue.One => new Color(139, 172, 15, 255),
            PixelValue.Two => new Color(48, 98, 48, 255),
            PixelValue.Three => new Color(155, 188, 15, 255),
            _ => new Color(155, 188, 15, 255),
        };

        public void Dispose() => CloseWindow();
    }
}
