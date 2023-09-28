using SharpBoy.Core.InputHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Tests.Mocks
{
    internal class InputHandlerMock : IInputHandler
    {
        public bool IsButtonPressed(GameBoyButton btn)
        {
            return false;
        }
    }
}
