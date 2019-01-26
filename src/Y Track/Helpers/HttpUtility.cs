using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.Helpers
{
    // taken from Xamarin .NET Core implementation of HttpUtility
    // https://github.com/xamarin/PortableRazor/blob/master/PortableRazor.Web/HttpUtility.cs
    public sealed class HttpUtility
    {
        #region Constructors

        public HttpUtility()
        {
        }

        #endregion // Constructors

        #region Methods

        public static Dictionary<string, string> ParseQueryString(string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }

        public static Dictionary<string, string> ParseQueryString(string query, Encoding encoding)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
                return new Dictionary<string, string>();
            if (query[0] == '?')
                query = query.Substring(1);

            var result = new Dictionary<string, string>();
            _parseQueryString(query, encoding, result);
            return result;
        }

        internal static void _parseQueryString(string query, Encoding encoding, Dictionary<string, string> result)
        {
            if (query.Length == 0)
                return;

            string decoded = System.Net.WebUtility.HtmlDecode(query);
            int decodedLength = decoded.Length;
            int namePos = 0;
            bool first = true;
            while (namePos <= decodedLength)
            {
                int valuePos = -1, valueEnd = -1;
                for (int q = namePos; q < decodedLength; q++)
                {
                    if (valuePos == -1 && decoded[q] == '=')
                    {
                        valuePos = q + 1;
                    }
                    else if (decoded[q] == '&')
                    {
                        valueEnd = q;
                        break;
                    }
                }

                if (first)
                {
                    first = false;
                    if (decoded[namePos] == '?')
                        namePos++;
                }

                string name, value;
                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else
                {
                    name = System.Net.WebUtility.UrlDecode(decoded.Substring(namePos, valuePos - namePos - 1));
                }
                if (valueEnd < 0)
                {
                    namePos = -1;
                    valueEnd = decoded.Length;
                }
                else
                {
                    namePos = valueEnd + 1;
                }
                value = System.Net.WebUtility.UrlDecode(decoded.Substring(valuePos, valueEnd - valuePos));

                result.Add(name, value);
                if (namePos == -1)
                    break;
            }
        }

        #endregion // Methods
    }




}
