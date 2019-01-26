using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.YoutubeCaptureEngine.Models
{
    public class MediaFileDataInfo
    {
        
        public Container Container { get; }

        public AudioEncoding? AudioEncoding { get; }

        public VideoEncoding? VideoEncoding { get; }

        public VideoQuality? VideoQuality { get; }
        public int? VerticalResolution { get; }
        public int ITag { get; }

        public MediaFileDataInfo(int iTag , Container container,
            AudioEncoding? audioEncoding,
            VideoEncoding? videoEncoding,
            VideoQuality? videoQuality,
            int? verticalResolution)
        {
            this.ITag = iTag;
            Container = container;
            AudioEncoding = audioEncoding;
            VideoEncoding = videoEncoding;
            VideoQuality = videoQuality;
            VerticalResolution = verticalResolution;
        }


        public override string ToString()
        {
            return $"Container : {Container} , Video Endocding : {VideoEncoding} , Video Quality : {VideoQuality} , Video Vertical Rosolution : {VerticalResolution}";
        }
    }
}
