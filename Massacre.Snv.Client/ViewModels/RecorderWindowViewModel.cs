using DevExpress.Mvvm;
using Massacre.Snv.Core.Backend;

namespace Massacre.Snv.Client.ViewModels
{
    public class RecorderWindowViewModel : ViewModelBase
    {
        public DelegateCommand StopRecordingCommand { get; }

        public RecorderWindowViewModel()
        {
            StopRecordingCommand = new DelegateCommand(delegate { SnvBackend.RecStop(); });
        }
    }
}
