using System;
using System.Net;

namespace Y_Track.YoutubeCaptureEngine.Models.Exceptions
{
    public class PacketDownloadExceptionEventArgumanets : EventArgs
    {
        public WebException Exception { get; set; }
        public YoutubeRequestURL FailedPacketRequestURL { get; set; }
        public Range Range { get; set; }
    }
}
