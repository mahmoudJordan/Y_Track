using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.YoutubeCaptureEngine.Models
{
    public class VideoPacket
    {
        public string FilePath { get; set; }
        public Range Range { get; set; }
        public string URL { get; set; }
        public string VideoDomainBase { get; set; }
    }
}
