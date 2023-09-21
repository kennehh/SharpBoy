using ReactiveUI;
using SharpBoy.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.ViewModels
{
    public class DebugViewModel : ViewModelBase
    {
        private IReadOnlyDictionary<string, string> registers = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> Registers
        {
            get => registers;
            private set => registers = value;
        }

        public DebugViewModel()
        {
        }

        public DebugViewModel(GameBoy gb) 
        {
            gb.StateUpdated += state =>
            {
                // Use ReactiveUI's RxApp.MainThreadScheduler to update UI on the main thread
                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    Registers = new Dictionary<string, string>(state.Registers);
                    this.RaisePropertyChanged(nameof(Registers));
                });
            };
        }
    }
}
