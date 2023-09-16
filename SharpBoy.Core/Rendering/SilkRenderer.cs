using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.ComponentModel;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace SharpBoy.Core.Rendering
{
    internal class SilkRenderer : IRenderer, IDisposable
    {
        private const int LcdWidth = 160;
        private const int LcdHeight = 144;
        private readonly RenderQueue renderQueue;
        private bool disposed = false;

        private IWindow window = null;
        private GL gl = null;

        private uint texture;
        private uint shaderProgram;
        private uint vao, vbo, ebo;

        public event Action OnClose;

        public SilkRenderer(RenderQueue renderQueue)
        {
            this.renderQueue = renderQueue;
        }

        public void Initialise()
        {
            var options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(LcdWidth * 4, LcdHeight * 4),
                Title = "Game Boy Emulator"
            };
            window = Window.Create(options);

            window.Load += () =>
            {
                gl = window.CreateOpenGL();
                gl.ClearColor(Color.Black);

                GenerateShaderProgram();
                GenerateBuffers();
                SetupTexture();
            };

            window.Resize += size =>
            {
                var width = size.X;
                var height = size.Y;

                var ratioX = width / (float)LcdWidth;
                var ratioY = height / (float)LcdHeight;
                var ratio = ratioX < ratioY ? ratioX : ratioY;

                // Calculate the width and height that the will be rendered to
                var viewWidth = Convert.ToUInt32(LcdWidth * ratio);
                var viewHeight = Convert.ToUInt32(LcdHeight * ratio);
                // Calculate the position, which will apply proper "pillar" or "letterbox" 
                var viewX = Convert.ToInt32((width - LcdWidth * ratio) / 2);
                var viewY = Convert.ToInt32((height - LcdHeight * ratio) / 2);

                gl.Viewport(viewX, viewY, viewWidth, viewHeight);
            };

            window.Closing += () => OnClose?.Invoke();
        }

        public void Run()
        {
            window.Render += deltaTime =>
            {
                renderQueue.WaitForNextFrame();
                if (renderQueue.TryDequeue(out var fb))
                {
                    Render(fb);
                }
            };
            window.Run();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                window?.Close();
                window?.Dispose();
                gl?.Dispose();
                disposed = true;
            }
        }

        private void GenerateShaderProgram()
        {
            const string vertexShaderCode = @"
#version 330 core
        
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoords;
out vec2 frag_texCoords;
        
void main()
{
    gl_Position = vec4(aPosition, 1.0);
    frag_texCoords = aTexCoords;
}";

            const string fragmentShaderCode = @"
#version 330 core

in vec2 frag_texCoords;
out vec4 out_color;
uniform sampler2D uTexture;
        
void main()
{
    out_color = texture(uTexture, frag_texCoords);
}";

            // Vertex Shader
            uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);
            gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out var vStatus);
            if (vStatus != (int)GLEnum.True)
            {
                throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(vertexShader));
            }

            // Fragment Shader
            uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderCode);
            gl.CompileShader(fragmentShader);
            gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out var fStatus);
            if (fStatus != (int)GLEnum.True)
            {
                throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(fragmentShader));
            }

            // Shader Program
            shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);
            gl.GetProgram(shaderProgram, ProgramPropertyARB.LinkStatus, out var pStatus);
            if (pStatus != (int)GLEnum.True)
            {
                throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(shaderProgram));
            }

            gl.DetachShader(shaderProgram, vertexShader);
            gl.DetachShader(shaderProgram, fragmentShader);
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
        }

        private unsafe void GenerateBuffers()
        {
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);

            float[] vertices =
            {
                 // positions        // texture coordinates
                 1.0f,  1.0f, 0.0f,  1.0f, 0.0f,
                 1.0f, -1.0f, 0.0f,  1.0f, 1.0f,
                -1.0f, -1.0f, 0.0f,  0.0f, 1.0f,
                -1.0f,  1.0f, 0.0f,  0.0f, 0.0f
            };

            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (float* buf = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            }

            uint[] indices =
            {
                0, 1, 3,
                1, 2, 3
            };

            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            fixed (uint* buf = indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
            }

            const uint stride = 3 * sizeof(float) + 2 * sizeof(float);
            // position attribute
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
            // texture attribute
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(1);

            gl.BindVertexArray(0);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        private void SetupTexture()
        {
            texture = gl.GenTexture();

            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, texture);

            gl.TextureParameter(texture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            gl.TextureParameter(texture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            gl.BindTexture(TextureTarget.Texture2D, 0);
        }

        private unsafe void Render(ReadOnlySpan<byte> frameBuffer)
        {
            gl.BindTexture(TextureTarget.Texture2D, texture);
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, 160, 144, 0, PixelFormat.Rgb, PixelType.UnsignedByte, frameBuffer);
            gl.BindTexture(TextureTarget.Texture2D, 0);

            gl.Clear(ClearBufferMask.ColorBufferBit);

            gl.BindVertexArray(vao);
            gl.UseProgram(shaderProgram);

            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, texture);

            gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }
    }
}
