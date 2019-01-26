using System;

namespace Y_Track.YoutubeCaptureEngine.Models
{
    [Serializable]
    public class YoutubePlaybackRequestPath
    {
        public QueryString QueryString { get; set; }
        public string Path { get; set; }
        public string ToYoutubePlaybackRequestPathString() => $"{Path}?{Helpers.Misc.ToQueryString(QueryString.Tokens)}";

    }
}
