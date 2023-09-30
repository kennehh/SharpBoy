using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.SdlCore
{
    public class SdlManager : IDisposable
    {
        public SdlManager()
        {
            Initialise();
        }

        private void Initialise()
        {
            CheckSdlResult(SDL.SDL_Init(SDL.SDL_INIT_VIDEO), "Could not initialise SDL");
            CheckSdlResult(SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3), "Could not set attribute SDL_GL_CONTEXT_MAJOR_VERSION");
            CheckSdlResult(SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 3), "Could not set attribute SDL_GL_CONTEXT_MINOR_VERSION");
            CheckSdlResult(SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE), "Could not set attribute SDL_GL_CONTEXT_PROFILE_MASK");
        }

        public void Dispose()
        {
            SDL.SDL_Quit();
        }


        private void CheckSdlResult(int result, string errorMessage)
        {
            if (result != 0)
            {
                throw new SdlException(errorMessage);
            }
        }
    }
}
