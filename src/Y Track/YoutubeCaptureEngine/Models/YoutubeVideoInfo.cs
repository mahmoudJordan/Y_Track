using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Y_Track.Extentions;
using YoutubeExplode.Models;

namespace Y_Track.YoutubeCaptureEngine.Models
{

    // the class is a copy of Youtube Explode Video class
    // I'm re writing it with different name to avoid confusing with YoutubeVideo
    public class YoutubeVideoInfo
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public DateTimeOffset UploadDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public IReadOnlyList<string> Keywords { get; set; }
        public Statistics Statistics { get; set; }




        public static YoutubeVideoInfo FromVideoObject(Video videoObject)
        {
            YoutubeVideoInfo info = new YoutubeVideoInfo();
            info.Id = videoObject.Id;
            info.Title = videoObject.Title;
            info.Statistics = videoObject.Statistics;
            info.UploadDate = videoObject.UploadDate;
            info.Duration = videoObject.Duration;
            info.Description = videoObject.Description;
            info.Author = videoObject.Author;
            info.ThumbnailUrl = videoObject.Thumbnails.HighResUrl;
            info.Author = videoObject.Author;
            //videoObject.CopyPropertiesTo(info);
            return info;
        }
    }
}
