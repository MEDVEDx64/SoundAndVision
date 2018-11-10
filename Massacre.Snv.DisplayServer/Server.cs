using DevExpress.Mvvm;
using Massacre.Snv.Core.Configuration;
using Massacre.Snv.Core.Network;
using Massacre.Snv.Core.Utils;
using Massacre.Snv.DisplayServer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Massacre.Snv.DisplayServer
{
    public class Server : ViewModelBase
    {
        Task srvTask = null;
        Queue<string> eventLogLines = new Queue<string>();
        string searchExpr = "";

        static readonly int MaxLogLines = 0x1000;

        public ObservableCollection<Client> Clients { get; }
        public DelegateCommand<string> PushSearchFilterCommand { get; }

        public IList<Client> ClientsSortedAndFiltered
        {
            get
            {
                try
                {
                    var list = new List<Client>();
                    var clients = Clients.ToList();
                    var expr = new StringBuilder();
                    var esc = ".$^{[(|)*+?\\";

                    if (searchExpr.Length > 0)
                    {
                        foreach (var c in searchExpr)
                        {
                            if (esc.Contains(c))
                            {
                                expr.Append("\\" + c);
                            }
                            else
                            {
                                expr.Append(char.ToLower(c));
                            }
                        }
                    }

                    var regex = new Regex(expr.ToString());
                    foreach (var cl in clients)
                    {
                        var m = regex.Match(cl.Name.ToLower());
                        while (m.Success)
                        {
                            list.Add(cl);
                            break;
                        }
                    }

                    return list.OrderBy(x => x.Name).ToList();
                }

                catch
                {
                    return new List<Client>();
                }
            }
        }

        public IList<Display> DisplayBundle
        {
            get
            {
                var bundle = new List<Display>();
                var clients = ClientsSortedAndFiltered.ToList();
                foreach(var cl in clients)
                {
                    var displays = cl.Displays.ToList();
                    foreach(var d in displays)
                    {
                        bundle.Add(d);
                    }
                }

                return bundle.OrderBy(x => x.Origin.Name).ToList();
            }
        }

        public string EventLogText
        {
            get
            {
                var sb = new StringBuilder();
                foreach(var l in eventLogLines)
                {
                    sb.Append((sb.Length == 0 ? "" : "\n") + l);
                }

                return sb.ToString();
            }
        }

        public string ComputersPanelCaption
        {
            get { return "Connections: " + Clients.Count; }
        }

        public Server()
        {
            Clients = new ObservableCollection<Client>();
            Clients.CollectionChanged += OnClientsChanged;
            PushSearchFilterCommand = new DelegateCommand<string>(PushSearchFilter);
            var port = Convert.ToUInt16(Chest.Get().Data.EnsureValue("Port", EndPointBase.Port.ToString()));

            srvTask = new Task(() =>
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                try
                {
                    while (true)
                    {
                        var cli = new Client(listener.AcceptSocket(), this);
                        cli.SetShutdownCallback(new Action(() =>
                        {
                            Clients.Remove(cli);
                        }));

                        cli.Start();

                        AppTools.TryInvoke(() =>
                        {
                            Clients.Add(cli);
                        });
                    }
                }

                catch (TaskCanceledException) { }
                finally
                {
                    listener.Stop();
                }
            });

            srvTask.ContinueWith(AppTools.TaskCrash);
            srvTask.Start();
        }

        private void OnClientsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Client cl in e.OldItems)
                {
                    cl.DisplaysChanged -= OnDisplaysChanged;
                    cl.NameChanged -= OnClientNameChanged;
                    cl.IdChanged -= OnClientIdChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (Client cl in e.NewItems)
                {
                    cl.DisplaysChanged += OnDisplaysChanged;
                    cl.NameChanged += OnClientNameChanged;
                    cl.IdChanged += OnClientIdChanged;
                }
            }

            RaisePropertyChanged(nameof(DisplayBundle));
            RaisePropertyChanged(nameof(ComputersPanelCaption));
        }

        private void OnDisplaysChanged(object sender, DisplaysChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(DisplayBundle));
        }

        private void OnClientNameChanged(object sender, ValueCarrierEventArgs<string> e)
        {
            RaisePropertyChanged(nameof(DisplayBundle));
        }

        private void OnClientIdChanged(object sender, ValueCarrierEventArgs<Guid?> e)
        {
            RaisePropertyChanged(nameof(DisplayBundle));
        }

        void PushSearchFilter(string expr)
        {
            searchExpr = expr;
            RaisePropertyChanged(nameof(DisplayBundle));
        }

        public void PrintLog(string msg)
        {
            try
            {
                if (eventLogLines.Count >= MaxLogLines)
                {
                    eventLogLines.Dequeue();
                }

                eventLogLines.Enqueue("[" + DateTime.Now + "] " + msg);
                RaisePropertyChanged(nameof(EventLogText));
            }

            catch
            {
            }
        }
    }
}
