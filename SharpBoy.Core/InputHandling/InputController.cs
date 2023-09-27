using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.InputHandling
{
    public class InputController : IInputController
    {
        private byte CombinedState => (byte)~buttonState;
        private byte DirectionState => (byte)(CombinedState >>> 4 & 0x0f);
        private byte ActionState => (byte)(CombinedState & 0x0f);

        private GameBoyButton buttonState = 0;
        private JoypadFlags joypadRegister = 0;
        private readonly IInputHandler inputHandler;
        private static readonly IEnumerable<GameBoyButton> buttons = Enum.GetValues<GameBoyButton>();

        public InputController(IInputHandler inputHandler)
        {
            this.inputHandler = inputHandler;
        }

        public void CheckForInputs()
        {
            buttonState = 0;
            byte state = 0x0f;

            if (joypadRegister.HasFlag(JoypadFlags.SelectAction) || joypadRegister.HasFlag(JoypadFlags.SelectDirection))
            {                
                foreach (var btn in buttons)
                {
                    if (inputHandler.IsButtonPressed(btn))
                    {
                        buttonState |= btn;
                    }
                }

                state = joypadRegister.HasFlag(JoypadFlags.SelectDirection) ? DirectionState : ActionState;
            }

            joypadRegister |= (JoypadFlags)state;
        }

        public byte ReadRegister()
        {
            return (byte)joypadRegister;
        }

        public void WriteRegister(byte value)
        {
            joypadRegister = (JoypadFlags)(value & 0b0011_0000);
        }

        [Flags]
        private enum JoypadFlags : byte
        {
            SelectAction = 1 << 5,
            SelectDirection = 1 << 4,
            DownOrStartReleased = 1 << 3,
            UpOrSelectReleased = 1 << 2,
            LeftOrBReleased = 1 << 1,
            RightOrAReleased = 1 << 0,
        }
    }
}
