using SDL2;
using SharpBoy.Core.InputHandling;

namespace SharpBoy.App.SdlCore
{
    public class SdlInputHandler : IInputHandler
    {

        private static readonly Dictionary<GameBoyButton, SDL.SDL_Scancode> mapping = new()
        {
            { GameBoyButton.A, SDL.SDL_Scancode.SDL_SCANCODE_X },
            { GameBoyButton.B, SDL.SDL_Scancode.SDL_SCANCODE_Z },
            { GameBoyButton.Start, SDL.SDL_Scancode.SDL_SCANCODE_RETURN },
            { GameBoyButton.Select, SDL.SDL_Scancode.SDL_SCANCODE_SPACE },
            { GameBoyButton.Up, SDL.SDL_Scancode.SDL_SCANCODE_UP },
            { GameBoyButton.Down, SDL.SDL_Scancode.SDL_SCANCODE_DOWN },
            { GameBoyButton.Left, SDL.SDL_Scancode.SDL_SCANCODE_LEFT },
            { GameBoyButton.Right, SDL.SDL_Scancode.SDL_SCANCODE_RIGHT },
        };

        public unsafe bool IsButtonPressed(GameBoyButton btn)
        {
            var keyStatePtr = SDL.SDL_GetKeyboardState(out int _);
            var scancode = mapping[btn];

            unsafe
            {
                byte* keyStates = (byte*)keyStatePtr;
                return keyStates[(int)scancode] != 0;
            }
        }
    }
}
