using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Rendering.Silk
{
    internal class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        //Our handle, buffertype and the GL instance this class will use, these are private because they have no reason to be public.
        //Most of the time you would want to abstract items to make things like this invisible.
        private uint handle;
        private BufferTargetARB _bufferType;
        private GL gl;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            //Setting the gl instance and storing our buffer type.
            this.gl = gl;
            _bufferType = bufferType;

            //Getting the handle, and then uploading the data to said handle.
            handle = this.gl.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                this.gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public void Bind()
        {
            //Binding the buffer object, with the correct buffer type.
            gl.BindBuffer(_bufferType, handle);
        }

        public void Dispose()
        {
            //Remember to delete our buffer.
            gl.DeleteBuffer(handle);
        }
    }
}
