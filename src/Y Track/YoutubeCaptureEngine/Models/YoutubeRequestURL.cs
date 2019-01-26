using System;
using Y_Track.Extentions;

namespace Y_Track.YoutubeCaptureEngine.Models
{
    [Serializable]
    public class YoutubeRequestURL : ICloneable
    {
        public YoutubePlaybackRequestPath RequestPath { get; set; }
        public string Host { get; set; }

        public object Clone()
        {
            return this.DeepClone();
        }

        public string ToRequestableURL()
        {
            return $"https://{this.Host}{this.RequestPath.ToYoutubePlaybackRequestPathString()}";
        }
    }
}
