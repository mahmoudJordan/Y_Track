using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Y_Track.YoutubeCaptureEngine.Models;

namespace Y_Track.YoutubeCaptureEngine
{
    public class YoutubeChatterClientHandler
    {
        private static YoutubeChatterClientHandler _instance;

        public const string ACTION_FIELD_NAME = "__ACTION__";
        public const string VIDEO_ID_FIELD_NAME = "__VIDEO__ID__";
        public const string ACTION_VIDEO_CAHNGED = "__VIDEO__CHANGED__";
        public const string ACTION_VIDEO_CLOSED = "__VIDEO__CLOSED__";

        public static YoutubeChatterClientHandler Instance
        {
            get
            {
                if (_instance == null) _instance = new YoutubeChatterClientHandler();
                return _instance;
            }
        }

        public YoutubeChatterClientMessage ParseRequestPath(string requestPath)
        {
            if (requestPath == null) throw new ArgumentNullException(nameof(requestPath));
            string messageQuery = requestPath.Split(new[] { "?" }, StringSplitOptions.None)[1];
            var tokens = Helpers.Misc.ParseQueryString(requestPath);

            var message = new YoutubeChatterClientMessage();

            var action = tokens[ACTION_FIELD_NAME];
            switch (action)
            {
                case ACTION_VIDEO_CAHNGED:
                    message.MessageType = YoutubeChatterClientMessageType.VideoChanged;
                    break;
                case ACTION_VIDEO_CLOSED:
                    message.MessageType = YoutubeChatterClientMessageType.VideoClosed;
                    break;
                default:
                    throw new Exception("Unkown Message Type");
            }

            message.VideoId = tokens[VIDEO_ID_FIELD_NAME];
            return message;
        }
    }
}
