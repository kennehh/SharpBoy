using ImGuiNET;
using SharpBoy.App.SdlCore;

namespace SharpBoy.App.ImGuiCore
{
    public class ImGuiManager : IDisposable
    {
        private readonly ImGuiRenderer imguiRenderer;
        private nint handle = nint.Zero;

        public string Title { get; }

        public ImGuiManager(ImGuiRenderer imguiRenderer)
        {
            this.imguiRenderer = imguiRenderer;
        }

        public void Initialise(SdlRenderer sdlRenderer)
        {
            //imguiRenderer.Initialise(sdlRenderer);
            handle = ImGui.CreateContext();
            ImGui.SetCurrentContext(handle);
        }

        public void SetAsCurrent()
        {
            ImGui.SetCurrentContext(handle);
        }

        public void Render()
        {
            ImGui.NewFrame();
            ImGui.ShowDemoWindow();
            ImGui.Render();
            ImGui.EndFrame();
        }

        public void Dispose()
        {
            ImGuiNET.ImGui.DestroyContext(handle);
            handle = nint.Zero;
        }
    }
}
