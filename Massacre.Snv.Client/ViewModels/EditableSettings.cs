using DevExpress.Mvvm;
using Massacre.Snv.Core.Configuration;
using System;

namespace Massacre.Snv.Client.ViewModels
{
    public class EditableSettings : BindableBase
    {
        public string Name { get; set; }
        public string ServerAddress { get; set; }
        public ushort ServerPort { get; set; }

        public string RecPrefix { get; set; }
        public uint RecSplitMaxMegabytes { get; set; }
        public uint RecSplitMaxCount { get; set; }

        public EditableSettings()
        {
            var chest = Chest.Get();
            var coreChest = new Chest("Massacre.Snv.Core");

            Name = chest.Data.EnsureValue(nameof(Name), "", true);
            ServerAddress = chest.Data.EnsureValue(nameof(ServerAddress), "", true);

            try
            {
                ServerPort = Convert.ToUInt16(chest.Data.EnsureValue(nameof(ServerPort), "8664", true));
            } catch { }

            RecPrefix = chest.Data.EnsureValue(nameof(RecPrefix), "", true);

            try
            {
                RecSplitMaxMegabytes = Convert.ToUInt32(chest.Data.EnsureValue(nameof(RecSplitMaxMegabytes), "0", true));
            }
            catch { }

            try
            {
                RecSplitMaxCount = Convert.ToUInt32(chest.Data.EnsureValue(nameof(RecSplitMaxCount), "0", true));
            }
            catch { }
        }

        public void Save()
        {
            var chest = Chest.Get();
            var coreChest = new Chest("Massacre.Snv.Core");

            chest.Data[nameof(Name)] = PreProcString(Name);
            chest.Data[nameof(ServerAddress)] = PreProcString(ServerAddress);
            chest.Data[nameof(ServerPort)] = ServerPort.ToString();

            chest.Data[nameof(RecPrefix)] = PreProcString(RecPrefix);
            chest.Data[nameof(RecSplitMaxMegabytes)] = RecSplitMaxMegabytes.ToString();
            chest.Data[nameof(RecSplitMaxCount)] = RecSplitMaxCount.ToString();
        }

        string PreProcString(string s)
        {
            return s == null ? null : (s.Length == 0 ? null : s);
        }

        public void OnExternalChange()
        {
            RaisePropertyChanged(nameof(RecPrefix));
        }
    }
}
