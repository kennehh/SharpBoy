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
        public event Action Closing;

        public void Initialise()
        {
        }

        public void Run()
        {
        }
    }
}
