using System;
using System.Collections.Generic;
using Y_Track.Helpers;

namespace Y_Track.YoutubeCaptureEngine.Models
{
    public class YoutubeStreamingStateRequest
    {
        private string _requestPathString;
        public IDictionary<string, string> Tokens { get; private set; }


        public Range Range;
        public double Length;
        public string VideoId;

        public YoutubeStreamingStateRequest(string requestPathString)
        {
            _requestPathString = requestPathString;
            _parseRequestString();
        }

        private void _parseRequestString()
        {
            Tokens = HttpUtility.ParseQueryString(_requestPathString);
        }

        public string ToYoutubePlaybackRequestPathString() => Helpers.Misc.ToQueryString(this.Tokens);
    }
}
