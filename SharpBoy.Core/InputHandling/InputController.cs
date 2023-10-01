namespace SharpBoy.Core.InputHandling
{
    public class InputController : IInputController
    {
        private JoypadFlags joypadRegister = 0;
        private readonly IInputHandler inputHandler;
        private static readonly IEnumerable<GameBoyButton> directionButtons = Enum.GetValues<GameBoyButton>().Where(x => x < GameBoyButton.A);
        private static readonly IEnumerable<GameBoyButton> actionButtons = Enum.GetValues<GameBoyButton>().Where(x => x >= GameBoyButton.A);

        public InputController(IInputHandler inputHandler)
        {
            this.inputHandler = inputHandler;
        }

        public void CheckForInputs()
        {
            byte state = 0x0f;

            if (!joypadRegister.HasFlag(JoypadFlags.SelectActionNotSelected))
            {
                state = CheckButtons(true);
            }
            else if (!joypadRegister.HasFlag(JoypadFlags.SelectDirectionNotSelected))
            {
                state = CheckButtons(false);
            }

            joypadRegister |= (JoypadFlags)state;
        }

        private byte CheckButtons(bool isAction)
        {
            var buttons = isAction ? actionButtons : directionButtons;
            byte state = 0;

            foreach (var btn in buttons)
            {
                if (inputHandler.IsButtonPressed(btn))
                {
                    state |= (byte)btn;
                }
            }

            if (isAction)
            {
                state >>>= 4;
            }

            return (byte)(~state & 0x0f);
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
            SelectActionNotSelected = 1 << 5,
            SelectDirectionNotSelected = 1 << 4,
            DownOrStartReleased = 1 << 3,
            UpOrSelectReleased = 1 << 2,
            LeftOrBReleased = 1 << 1,
            RightOrAReleased = 1 << 0,
        }
    }
}
