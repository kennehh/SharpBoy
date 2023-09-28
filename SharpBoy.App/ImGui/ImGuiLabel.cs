using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.ImGui
{
    public class ImGuiLabel : IImGuiWidget
    {
        private readonly string text;

        public ImGuiLabel(string text)
        {
            this.text = text;
        }

        public void Render()
        {
            ImGuiNET.ImGui.Text(text);
        }
    }
}
