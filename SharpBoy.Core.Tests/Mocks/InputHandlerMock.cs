using SharpBoy.Core.InputHandling;

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
