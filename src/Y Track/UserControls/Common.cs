using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Y_Track.DAL.Models;
using YoutubeExplode.Models;

namespace Y_Track.UserControls
{
    class Common
    {
        public static async Task<Thumbnail> FetchThumbnail(string thumbnailUrl)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] thumbnailBytes = await client.DownloadDataTaskAsync(new Uri(thumbnailUrl));
                    return new Thumbnail()
                    {
                        thumbnailBytes = thumbnailBytes,
                    };
                }
            }
            catch (Exception e)
            {
                Y_Track.Helpers.YTrackLogger.Log("Cannot Fetch Video Thumbnail from : " + thumbnailUrl + "\n\n --> " + e.Message + "\n\n" + e.StackTrace);
                return null;
            }
        }



        public static async Task<Youtube_Video> ToYoutubeVideo(Video eVideo)
        {

            return new Youtube_Video()
            {
                Id = eVideo.Id,
                Author = eVideo.Author,
                Description = eVideo.Description,
                Duration = eVideo.Duration,
                PhysicalPath = null,
                StoreDate = null,
                Title = eVideo.Title,
                Thumbnail = await FetchThumbnail(eVideo.Thumbnails.HighResUrl)
            };
        }

    }
}
