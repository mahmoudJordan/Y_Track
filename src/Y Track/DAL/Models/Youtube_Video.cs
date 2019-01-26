using System;
using System.Windows.Media;
using SQLite;

namespace Y_Track.DAL.Models
{
    public class Youtube_Video
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public TimeSpan Duration { get; set; }
        public string PhysicalPath { get; set; }
        public int? ThumbnailId { get; set; }
        public DateTime? StoreDate { get; set; }

        [Ignore]
        public Thumbnail Thumbnail { get; set; }
        [Ignore]
        public ImageSource ThumbnailImageSource => _getImageSource();

        private ImageSource _getImageSource()
        {
            try
            {
                byte[] data = this.Thumbnail.thumbnailBytes;
                ImageSource imageSource = Helpers.Misc.ToImage(data);
                return imageSource;
            }
            catch (Exception e)
            {
                Y_Track.Helpers.YTrackLogger.Log("Cannot Read Thumbnail Bytes : " + e.Message + "\n\n" + e.StackTrace);
                // thumbnail didn't read  successfully from the database
                // fill it with  thumbnail placeholder
                return null;
            }
        }
    }
}
