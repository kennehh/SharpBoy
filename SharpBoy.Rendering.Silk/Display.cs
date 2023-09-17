using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Rendering.Silk
{
    internal class Display : IDisposable
    {
        private float[] vertices =
        {
             // positions        // texture coordinates
             1.0f,  1.0f, 0.0f,  1.0f, 0.0f,
             1.0f, -1.0f, 0.0f,  1.0f, 1.0f,
            -1.0f, -1.0f, 0.0f,  0.0f, 1.0f,
            -1.0f,  1.0f, 0.0f,  0.0f, 0.0f
        };

        private uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private Shader shader;
        private BufferObject<uint> ebo;
        private BufferObject<float> vbo;
        private VertexArrayObject<float, uint> vao;
        private Texture texture;
        private GL gl;

        private uint width, height;

        public Display(GL gl, uint width, uint height)
        {
            this.gl = gl;
            this.width = width;
            this.height = height;

            shader = Shader.LoadFromEmbeddedResources(gl, "display.vert", "display.frag");
            ebo = new BufferObject<uint>(gl, indices, BufferTargetARB.ElementArrayBuffer);
            vbo = new BufferObject<float>(gl, vertices, BufferTargetARB.ArrayBuffer);
            vao = new VertexArrayObject<float, uint>(gl, vbo, ebo);
            vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
            texture = new Texture(gl);
        }

        public unsafe void Render(ReadOnlySpan<byte> framebuffer)
        {
            texture.Bind();
            texture.SetData(framebuffer, width, height, PixelFormat.Rgb, PixelType.UnsignedByte);

            vao.Bind();
            shader.Use();
            
            shader.SetUniform("uTexture", 0);

            gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, (void*)0);
        }

        public void Dispose()
        {
            vbo.Dispose();
            ebo.Dispose();
            vao.Dispose();
            shader.Dispose();
            texture.Dispose();
        }
    }
}
