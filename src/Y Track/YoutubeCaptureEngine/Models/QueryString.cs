using Y_Track.YoutubeCaptureEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Y_Track.Extentions;

namespace Y_Track.YoutubeCaptureEngine.Models
{
    [Serializable]
    public class QueryString : ICloneable 
    {
        public IDictionary<string, string> Tokens { get; private set; }

        public QueryString(IDictionary<string, string> tokens)
        {
            this.Tokens = tokens;
        }

        public QueryString(string queryString)
        {
            if (queryString == null) throw new ArgumentNullException();
            Tokens = Helpers.HttpUtility.ParseQueryString(queryString);
        }


        public string GetValue(string key)
        {
            if (this.Tokens.ContainsKey(key))
            {
                return this.Tokens[key];
            }
            return null;
        }


        public bool HasValue(string key)
        {
            return this.Tokens.ContainsKey(key);
        }

        public void SetValue(string key, string value)
        {
            if (key == null || value == null) throw new ArgumentNullException();

            if (this.Tokens.ContainsKey(key))
            {
                this.Tokens[key] = value;
            }
            else
            {
                this.Tokens.Add(key, value);
            }
        }

        public override string ToString()
        {
            return Helpers.Misc.ToQueryString(Tokens);
        }

        public object Clone()
        {
            return this.DeepClone();
        }



    }
}
