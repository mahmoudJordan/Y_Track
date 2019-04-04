using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Y_Track.YoutubeCaptureEngine.Models;

namespace Y_Track.DAL.Models
{
    public class Mapper
    {
        private static Mapper _instance { get; set; }
        public static Mapper Instance
        {
            get
            {
                if (_instance == null) _instance = new Mapper();
                return _instance;
            }
        }


        /// <summary>
        /// Map YoutubeVideoInfo from Youtube_Video
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        public YoutubeVideoInfo FromYoutubeVideoToYoutubeVideoInfo(Youtube_Video video)
        {
            return new YoutubeVideoInfo()
            {
                Author = video.Author,
                Description = video.Description,
                Duration = video.Duration,
                Id = video.Id,
                Title = video.Title,
            };
        }

        protected Mapper() { }
    }
}
