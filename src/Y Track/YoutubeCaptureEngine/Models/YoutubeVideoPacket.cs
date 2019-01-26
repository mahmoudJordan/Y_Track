
using System;
using System.Diagnostics;
using System.IO;
using Y_Track.Helpers;
using Y_Track.YoutubeCaptureEngine.Models.Exceptions;

namespace Y_Track.YoutubeCaptureEngine.Models
{
    public class YoutubeMediaPacket
    {
       
        public string VideoFingerPrint { get; private set; }
        public string VideoId { get; private set; }
        public Range Range { get; private set; }
        public double OverAllLength { get; private set; }
        public string FilePath { get; private set; }
        public YoutubeMediaPacketType VideoPacketType { get; private set; }
        public double PacketLength { get; private set; }
        public YoutubeRequestURL YoutubeRequestURL { get; private set; }
        public MediaFileDataInfo PacketMediaFileDataInfo { get; private set; }

        public YoutubeMediaPacket(YoutubeMediaPacketType videoPacketType, YoutubeRequestURL requestURL, byte[] packetData)
        {
            FilePath = _writePacketDataToDisk(packetData);
            VideoPacketType = videoPacketType;
            YoutubeRequestURL = requestURL;
            _populateFromYoutubeRequestURL(requestURL);
            VideoId = requestURL.RequestPath.QueryString.HasValue(YoutubeChatterClientHandler.VIDEO_ID_FIELD_NAME) ?
             requestURL.RequestPath.QueryString.GetValue(YoutubeChatterClientHandler.VIDEO_ID_FIELD_NAME)
             : null;
        }

        public YoutubeMediaPacket(YoutubeMediaPacketType videoPacketType, YoutubeRequestURL requestURL, string packetFilePath)
        {
            FilePath = packetFilePath;
            VideoPacketType = videoPacketType;
            YoutubeRequestURL = requestURL;
            VideoId = requestURL.RequestPath.QueryString.HasValue(YoutubeChatterClientHandler.VIDEO_ID_FIELD_NAME) ?
             requestURL.RequestPath.QueryString.GetValue(YoutubeChatterClientHandler.VIDEO_ID_FIELD_NAME)
             : null;
            _populateFromYoutubeRequestURL(requestURL);
        }

        private string _writePacketDataToDisk(byte[] packetData)
        {
            string tmpFile = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllBytes(tmpFile, packetData);
            }
            catch (IOException)
            {
                throw;
            }

            return tmpFile;
        }


        public static bool IsValidYoutubeRequestURL(YoutubeRequestURL requestURL)
        {
            return
                // range is required for track packet index
                requestURL.RequestPath.QueryString.HasValue("range")
                // clen is required to know the actual size of the video
                && requestURL.RequestPath.QueryString.HasValue("clen")
                // stream id is requried for distinguish each video (in case two or more videos are buffering at the same time)
                && requestURL.RequestPath.QueryString.HasValue("id")
                // itag is required to parse media info (quality , bitrate , containers...)
                && requestURL.RequestPath.QueryString.HasValue("itag");

        }



        private void _populateFromYoutubeRequestURL(YoutubeRequestURL youtubePlaybackRequestURL)
        {
            // make sure the url contains the required fields (packet range , total video lenght, stream id , itag)
            // if not we can't intercept this video
            if (IsValidYoutubeRequestURL(youtubePlaybackRequestURL))
            {
                Range = new Range(youtubePlaybackRequestURL.RequestPath.QueryString.GetValue("range"));
                OverAllLength = double.Parse(youtubePlaybackRequestURL.RequestPath.QueryString.GetValue("clen"));
                PacketLength = Range.Length;
                VideoFingerPrint = youtubePlaybackRequestURL.RequestPath.QueryString.GetValue("id");
                PacketMediaFileDataInfo = ItagHelper.FindByItag(int.Parse(youtubePlaybackRequestURL.RequestPath.QueryString.GetValue("itag")));
            }
            else throw new InsuffesientYoutubeURLParams();
        }



        public override string ToString()
        {
            return
                "Video FingerPrint : " + this.VideoFingerPrint + "\n" +
                "Video Range : " + this.Range.ToString() + "\n" +
                "Video Packet Length : " + this.PacketLength + "\n" +
                "Video Packet Type : " + this.VideoPacketType + "\n" +
                "Video Quality Info : " + this.PacketMediaFileDataInfo + "\n" +
                "Video Length : " + this.OverAllLength + "\n" +
                "Video File Path : " + this.FilePath +
                "Video Requested URL : " + YoutubeRequestURL.ToRequestableURL();

        }


    }
}
