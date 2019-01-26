using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.Titanium
{
    public class ProxyStatusEventsArguments : EventArgs
    {
        public ProxyStatus Status { set; get; }
    }
}
