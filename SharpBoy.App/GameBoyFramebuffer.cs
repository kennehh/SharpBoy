using SDL2;
using SharpBoy.App.SdlCore;
using SharpBoy.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App
{
    public class GameBoyFramebuffer : IDisposable
    {
        private const int TextureWidth = 160;
        private const int TextureHeight = 144;

        private IRenderQueue renderQueue;
        private SdlRenderer renderer;
        private SdlTexture texture;
        private int width = TextureWidth;
        private int height = TextureHeight;
        private int xPosition = 0;
        private int yPosition = 0;

        public GameBoyFramebuffer(IRenderQueue renderQueue)
        {
            this.renderQueue = renderQueue;
        }

        public void Initialise(SdlRenderer renderer)
        {
            this.renderer = renderer;
            texture = new SdlTexture(this.renderer, TextureWidth, TextureHeight, SDL.SDL_PIXELFORMAT_ABGR8888, 4);
        }

        public void Render()
        {
            //_renderQueue.WaitForNextFrame();
            if (renderQueue.TryDequeue(out ReadOnlySpan<byte> frameBuffer))
            {
                texture.Update(frameBuffer);
            }

            SDL.SDL_Rect destRect = new SDL.SDL_Rect
            {
                x = xPosition,
                y = yPosition,
                w = width,
                h = height
            };

            SDL.SDL_RenderCopy(renderer.Handle, texture.Handle, IntPtr.Zero, ref destRect);
        }

        public void Resize(int width, int height)
        {
            var ratioX = width / (float)TextureWidth;
            var ratioY = height / (float)TextureHeight;
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            // Calculate the width and height that the will be rendered to
            this.width = Convert.ToInt32(TextureWidth * ratio);
            this.height = Convert.ToInt32(TextureHeight * ratio);
            // Calculate the position, which will apply proper "pillar" or "letterbox" 
            xPosition = Convert.ToInt32((width - TextureWidth * ratio) / 2);
            yPosition = Convert.ToInt32((height - TextureHeight * ratio) / 2);
        }

        public void Dispose()
        {
            texture?.Dispose();
        }
    }

}
