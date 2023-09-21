using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Rendering.Silk
{
    internal class Texture
    {
        private uint handle;
        private GL gl;

        public unsafe Texture(GL gl)
        {
            //Saving the gl instance.
            this.gl = gl;

            //Generating the opengl handle;
            handle = this.gl.GenTexture();
            Bind();

            SetParameters();
        }

        public void SetData(ReadOnlySpan<byte> data, uint width, uint height, PixelFormat format, PixelType pixelType)
        {
            Bind();
            //Setting the data of a texture.
            gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, format, pixelType, data);
        }

        private void SetParameters()
        {
            //Setting some texture perameters so the texture behaves as expected.
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            //When we bind a texture we can choose which textureslot we can bind it to.
            gl.ActiveTexture(textureSlot);
            gl.BindTexture(TextureTarget.Texture2D, handle);
        }

        public void Dispose()
        {
            //In order to dispose we need to delete the opengl handle for the texure.
            gl.DeleteTexture(handle);
        }
    }
}
