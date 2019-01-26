using System.Linq;
using System.Collections.Generic;
using Y_Track.Helpers;

namespace Y_Track.YoutubeCaptureEngine.Models
{
    public class YoutubeVideo 
    {
        public string VideoFingerPrint { get; private set; }
        public bool HaveGabs => this._areVideoPacketsCompleted();
        public YoutubeVideoInfo VideoInfo { get; set; }
        public List<YoutubeMediaPacket> Packets { get; private set; }
        public YoutubeMediaPacket HigherPacketQuality => _getHigherQualityPacket();
        public int? CurrentVideoVerticalRosolution;


        public YoutubeVideo(string videoFingerPrint)
        {
            Packets = new List<YoutubeMediaPacket>();
            this.VideoFingerPrint = videoFingerPrint;
        }


        private bool _areVideoPacketsCompleted()
        {

            // get the first video and audio packets with higher quality
            var videoPacketOverAllLength = this.Packets.First(x => x.VideoPacketType == YoutubeMediaPacketType.Video
            && x.PacketMediaFileDataInfo.VerticalResolution == CurrentVideoVerticalRosolution).OverAllLength;

            var audioPacketOverAllLength = this.Packets.First(x => x.VideoPacketType == YoutubeMediaPacketType.Audio).OverAllLength;

            // compute the summation for both audio and video stored packages
            double? videoBytesSum = _computePacketsSum(YoutubeMediaPacketType.Video);
            double? audioBytesSum = _computePacketsSum(YoutubeMediaPacketType.Audio);

            if (!videoBytesSum.HasValue || !audioBytesSum.HasValue) return false;

            // if both the summation of audio packets and the summation of video packets equal the clen (overall packets length) then we have all the packets
            return videoBytesSum == videoPacketOverAllLength && audioBytesSum == audioPacketOverAllLength;
        }


        private double? _computePacketsSum(YoutubeMediaPacketType type)
        {
            var higherQualityPacket = _getHigherQualityPacket();

            if (higherQualityPacket == null) return null;

            int? higherQuality = higherQualityPacket.PacketMediaFileDataInfo.VerticalResolution;

            if (!higherQuality.HasValue) return null;

            return this.Packets
              .Where(x => x.VideoPacketType == type)
              .Sum(x => x.Range.Length);
        }


        private YoutubeMediaPacket _getHigherQualityPacket()
        {
            return this.Packets
                .OrderByDescending(x => x.PacketMediaFileDataInfo.VerticalResolution)
                .FirstOrDefault();
        }

    }
}
