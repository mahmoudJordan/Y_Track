﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.Fiddler
{
    public class FiddlerStatusEventArguments : EventArgs
    {
        public FiddlerStatus Status { set; get; }
    }
}
