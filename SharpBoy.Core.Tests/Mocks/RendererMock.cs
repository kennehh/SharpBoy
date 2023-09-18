using SharpBoy.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Tests.Mocks
{
    internal class RendererMock : IRenderer
    {
        public void Initialise(Func<string, nint> getProcAddress)
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
            throw new NotImplementedException();
        }

        public void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}
