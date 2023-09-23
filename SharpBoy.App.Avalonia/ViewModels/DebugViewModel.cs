using Avalonia.Threading;
using SharpBoy.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.Avalonia.ViewModels
{
    public class DebugViewModel : ViewModelBase
    {
        public ObservableCollection<RegisterState> Registers { get; set; } = new ObservableCollection<RegisterState>();

        public DebugViewModel()
        {
        }

        public DebugViewModel(GameBoy gb)
        {
            gb.StateUpdated += state =>
            {
                // Use Avalonia's UIThread to update UI on the main thread
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    int count = state.Registers.Count;

                    // Update existing items
                    for (int i = 0; i < count && i < Registers.Count; i++)
                    {
                        Registers[i] = state.Registers[i];
                    }

                    // If there are more items in the new state, add them
                    for (int i = Registers.Count; i < count; i++)
                    {
                        Registers.Add(state.Registers[i]);
                    }

                    // If there are fewer items in the new state, remove the extra ones
                    while (Registers.Count > count)
                    {
                        Registers.RemoveAt(Registers.Count - 1);
                    }
                });
            };
        }
    }
}
