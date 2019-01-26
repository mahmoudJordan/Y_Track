using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.FFMpeg
{
    public class MultiplexingException : Exception
    {
        public MultiplexingException(string message) : base(message){}
    }
}
