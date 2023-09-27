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
        private GameBoyButton buttonState = 0;
        private JoypadFlags register = 0;
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

            if (register.HasFlag(JoypadFlags.SelectAction) || register.HasFlag(JoypadFlags.SelectDirection))
            {                
                foreach (var btn in buttons)
                {
                    if (inputHandler.IsButtonPressed(btn))
                    {
                        buttonState |= btn;
                    }
                }

                state = (byte)buttonState;
                if (register.HasFlag(JoypadFlags.SelectDirection))
                {
                    state >>>= 4;
                }
                state = (byte)(~state & 0x0f);
            }
            register |= (JoypadFlags)state;
        }

        public byte ReadRegister()
        {
            return (byte)register;
        }

        public void WriteRegister(byte value)
        {
            register = (JoypadFlags)(value & 0b0011_0000);
        }

        [Flags]
        private enum JoypadFlags : byte
        {
            SelectAction = 1 << 5,
            SelectDirection = 1 << 4,
            DownOrStartNotPressed = 1 << 3,
            UpOrSelectNotPressed = 1 << 2,
            LeftOrBNotPressed = 1 << 1,
            RightOrANotPressed = 1 << 0,
        }
    }
}
