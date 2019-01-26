using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Y_Track.YoutubeCaptureEngine.Models;

namespace Y_Track.YoutubeCaptureEngine
{
    public static class YoutubePlaybackRequestPathParser
    {
        /// <summary>
        /// Parse requestPathString to YoutubePlaybackRequestPath
        /// </summary>
        /// <param name="requestPathString"></param>
        /// <returns></returns>
        public static YoutubePlaybackRequestPath ParseRequestPathString(string requestPathString)
        {
            string[] pathAndQueryStringSplitted = requestPathString.Split(new[] { "?" }, StringSplitOptions.None);
            var result = new YoutubePlaybackRequestPath();
            result.Path = pathAndQueryStringSplitted[0];
            result.QueryString = new QueryString(pathAndQueryStringSplitted[1]);
            return result;
        }
       
    }
}
