using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Rendering
{
    internal interface IRenderer
    {
        event Action OnClose;
        void Initialise();
        void Run();
    }
}
