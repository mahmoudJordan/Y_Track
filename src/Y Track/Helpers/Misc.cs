using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Y_Track.Helpers
{
    public class Misc
    {
        /// <summary>
        /// remove illegal character from pathes
        /// </summary>
        /// <param name="illegal"></param>
        /// <returns></returns>
        public static string RemoveIllegalPathChars(string illegal)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(illegal, "");
        }

        /// <summary>
        /// parse a query string and return a dictionary
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseQueryString(string uri)
        {
            var matches = Regex.Matches(uri, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            return matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value)
            );
        }

        /// <summary>
        /// NameValueCollection to dictionary convertor
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(NameValueCollection a)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var k in a.AllKeys)
            {
                dict.Add(k, a[k]);
            }
            return dict;
        }

        /// <summary>
        /// dictionary to query string
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string ToQueryString(IDictionary<string, string> dict)
        {
            var list = new List<string>();
            foreach (var item in dict)
            {
                list.Add(item.Key + "=" + item.Value);
            }
            return string.Join("&", list);
        }

        /// <summary>
        /// append a byte array to end of path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public static void AppendAllBytes(string path, byte[] bytes)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// convert byte[] to image
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

    }
}
