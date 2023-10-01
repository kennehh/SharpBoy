using SDL2;

namespace SharpBoy.App.SdlCore
{
    public class SdlException : Exception
    {
        public SdlException() : this(string.Empty)
        {
        }

        public SdlException(string message) : base(GetSdlMessage(message))
        {
        }

        private static string GetSdlMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message += ": ";
            }
            return message?.Trim() ?? string.Empty + $" {SDL.SDL_GetError()}";
        }
    }
}
