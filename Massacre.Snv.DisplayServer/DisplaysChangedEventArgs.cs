using Massacre.Snv.DisplayServer.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Massacre.Snv.DisplayServer
{
    public class DisplaysChangedEventArgs : EventArgs
    {
        public Client Parent { get; }

        public IEnumerable<Display> Old { get; }
        public IEnumerable<Display> New { get; }

        public DisplaysChangedEventArgs(Client c, IEnumerable o, IEnumerable n)
        {
            Parent = c;

            var _old = new List<Display>();
            var _new = new List<Display>();

            if(o != null)
            {
                foreach(Display d in o)
                {
                    _old.Add(d);
                }
            }

            if(n != null)
            {
                foreach(Display d in n)
                {
                    _new.Add(d);
                }
            }

            Old = _old;
            New = _new;
        }
    }
}
