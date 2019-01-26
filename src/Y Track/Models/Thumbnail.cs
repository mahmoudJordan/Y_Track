using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.Models
{
    public class Thumbnail
    {
        [PrimaryKey , AutoIncrement]
        public int Id { get; set; }
        public byte[] thumbnailBytes { get; set; }
    }
}
