using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Video
{
    internal interface IRenderer
    {
        void OpenWindow();
        void CloseWindow();
        bool IsWindowOpen();
        void Render(ReadOnlySpan<PixelValue> frameBuffer);
    }
}
