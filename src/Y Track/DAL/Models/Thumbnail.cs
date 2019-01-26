using SQLite;

namespace Y_Track.DAL.Models
{
    public class Thumbnail
    {
        [PrimaryKey , AutoIncrement]
        public int? Id { get; set; }
        public byte[] thumbnailBytes { get; set; }
    }
}
