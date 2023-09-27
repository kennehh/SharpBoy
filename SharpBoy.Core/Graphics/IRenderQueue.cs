using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    public interface IRenderQueue
    {
        void Enqueue(ReadOnlyMemory<byte> frame);
        bool TryDequeue(out ReadOnlySpan<byte> frameBuffer);
        void WaitForNextFrame();
    }
}
