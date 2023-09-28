using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.ImGui
{
    public class ImGuiManager : IDisposable
    {
        private readonly List<IImGuiWidget> widgets = new List<IImGuiWidget>();
        private nint handle = nint.Zero;

        public string Title { get; }

        public ImGuiManager(string title)
        {
            Title = title;

            widgets.Add(new ImGuiLabel("Hello world"));
        }

        public void Initialise()
        {
            handle = ImGuiNET.ImGui.CreateContext();
        }

        public void SetAsCurrent()
        {
            ImGuiNET.ImGui.SetCurrentContext(handle);
        }

        public void Render()
        {
            ImGuiNET.ImGui.NewFrame();
            if (ImGuiNET.ImGui.Begin(Title))
            {
                foreach (var widget in widgets)
                {
                    widget.Render();
                }
            }
            ImGuiNET.ImGui.End();
            ImGuiNET.ImGui.Render();
        }

        public void Dispose()
        {
            ImGuiNET.ImGui.DestroyContext(handle);
            handle = nint.Zero;
        }
    }
}
