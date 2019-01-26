using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.YoutubeCaptureEngine.Models
{

    /// <summary>
    /// a YoutubeVideoGab is a packet that was not captured from the youtube session 
    /// Or a packet with a lower quality than the higher quality packet
    /// </summary>
    class YoutubeVideoGab
    {
        public Range Range { get; set; }
        public MediaFileDataInfo HigherQualityMediaFileDataInfo { get; set; }
    }
}
