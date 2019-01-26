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

        //public Youtube_Video Youtube_VideoFromYoutubeVideoInfo(YoutubeVideoInfo info)
        //{
        //    return new Youtube_Video()
        //    {
        //        Author = info.Author,
        //        Description = info.Description,
        //        Duration = info.Duration,
        //        PhysicalPath = info.PhysicalPath,
        //        Title = info.Title,
        //        Id = info.Id
        //    };
        //}
       

        protected Mapper() { }
    }
}
