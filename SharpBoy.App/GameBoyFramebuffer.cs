using SDL2;
using SharpBoy.App.Sdl;
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

        private IRenderQueue _renderQueue;
        private SdlRenderer _renderer;
        private SdlTexture _texture;
        private int _width = TextureWidth;
        private int _height = TextureHeight;
        private int _xPosition = 0;
        private int _yPosition = 0;

        public GameBoyFramebuffer(IRenderQueue renderQueue)
        {
            _renderQueue = renderQueue;
        }

        public void Initialise(SdlRenderer renderer)
        {
            _renderer = renderer;
            _texture = new SdlTexture(_renderer, TextureWidth, TextureHeight);
        }

        public void Render()
        {
            //_renderQueue.WaitForNextFrame();
            if (_renderQueue.TryDequeue(out ReadOnlySpan<byte> frameBuffer))
            {
                _texture.Update(frameBuffer);
            }

            SDL.SDL_Rect destRect;
            destRect.x = _xPosition;
            destRect.y = _yPosition;
            destRect.w = _width;  // Window width
            destRect.h = _height; // Window height

            SDL.SDL_RenderCopy(_renderer.Handle, _texture.Handle, IntPtr.Zero, ref destRect);
        }

        public void Resize(int width, int height)
        {
            var ratioX = width / (float)TextureWidth;
            var ratioY = height / (float)TextureHeight;
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            // Calculate the width and height that the will be rendered to
            _width = Convert.ToInt32(TextureWidth * ratio);
            _height = Convert.ToInt32(TextureHeight * ratio);
            // Calculate the position, which will apply proper "pillar" or "letterbox" 
            _xPosition = Convert.ToInt32((width - TextureWidth * ratio) / 2);
            _yPosition = Convert.ToInt32((height - TextureHeight * ratio) / 2);
        }

        public void Dispose()
        {
            _texture?.Dispose();
        }
    }

}
