using ImGuiNET;
using SDL2;
using SharpBoy.App.SdlCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.ImGuiCore
{
    public class ImGuiRenderer
    {
        private SdlManager sdlManager;
        private SdlTexture fontTexture;

        public void Initialise(SdlManager sdlManager)
        {
            this.sdlManager = sdlManager;
            RebuildFontAtlas();
        }

        private unsafe void RebuildFontAtlas()
        {
            //// Get font texture from ImGui
            //var io = ImGui.GetIO();
            //io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

            //// Copy the data to a managed array
            //var pixels = new byte[width * height * bytesPerPixel];
            //unsafe { Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length); }

            //// Create and register the texture, dispose old one if necessary
            //fontTexture?.Dispose();
            //fontTexture = new SdlTexture(renderer, width, height, SDL.SDL_PIXELFORMAT_ABGR8888, bytesPerPixel);
            //fontTexture.Update(pixelData);

            //// Let ImGui know where to find the texture
            //io.Fonts.SetTexID(fontTexture.Handle);
            //io.Fonts.ClearTexData(); // Clears CPU side texture data
        }
    }
}
