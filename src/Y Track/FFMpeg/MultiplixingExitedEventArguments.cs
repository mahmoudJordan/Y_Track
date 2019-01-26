
using System;

namespace FFMpeg
{
    public class MultiplexingExitedEventArguments : EventArgs
    {
        public string OutputFilePath { get; set; }
        public string OutputFileName { get; set; }
        public string OutputFileExtention { get; set; }
    }
}
