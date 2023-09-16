using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Rendering
{
    public interface IRenderer
    {
        event Action Closing;
        void Initialise();
        void Run();
    }
}
