using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Rendering
{
    public interface IRenderer : IDisposable
    {
        void Initialise(Func<string, nint> getProcAddress);
        void Render();
        void Resize(int width, int height);
    }
}
