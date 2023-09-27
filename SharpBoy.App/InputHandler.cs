using SDL2;
using SharpBoy.Core.InputHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App
{
    public class InputHandler : IInputHandler
    {

        private static readonly Dictionary<GameBoyButton, SDL.SDL_Scancode> mapping = new()
        {
            { GameBoyButton.A, SDL.SDL_Scancode.SDL_SCANCODE_X },
            { GameBoyButton.B, SDL.SDL_Scancode.SDL_SCANCODE_Z },
            { GameBoyButton.Start, SDL.SDL_Scancode.SDL_SCANCODE_RETURN },
            { GameBoyButton.Select, SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE },
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
