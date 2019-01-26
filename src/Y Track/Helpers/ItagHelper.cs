using System;
using System.Collections.Generic;
using Y_Track.Extentions;
using Y_Track.YoutubeCaptureEngine.Models;

namespace Y_Track.Helpers
{
    public static class ItagHelper
    {
   
        private static readonly Dictionary<int, MediaFileDataInfo> ItagMap = new Dictionary<int, MediaFileDataInfo>
        {

            // Muxed
            {5, new MediaFileDataInfo( 5,Container.Flv, AudioEncoding.Mp3, VideoEncoding.H263, VideoQuality.Low144 , 144)},
            {6, new MediaFileDataInfo(6 , Container.Flv, AudioEncoding.Mp3, VideoEncoding.H263, VideoQuality.Low240 , 240 )},
            {13, new MediaFileDataInfo(13 , Container.Tgpp, AudioEncoding.Aac, VideoEncoding.Mp4V, VideoQuality.Low144 , 144 )},
            {17, new MediaFileDataInfo(17  ,Container.Tgpp, AudioEncoding.Aac, VideoEncoding.Mp4V, VideoQuality.Low144 , 144)},
            {18, new MediaFileDataInfo(18 , Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium360 , 360)},
            {22, new MediaFileDataInfo( 22, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High720 , 720)},
            {34, new MediaFileDataInfo( 34, Container.Flv, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium360 , 360)},
            {35, new MediaFileDataInfo( 34, Container.Flv, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480 , 480)},
            {36, new MediaFileDataInfo(36 , Container.Tgpp, AudioEncoding.Aac, VideoEncoding.Mp4V, VideoQuality.Low240 , 240)},
            {37, new MediaFileDataInfo(37 , Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High1080 , 1080)},
            {38, new MediaFileDataInfo(38, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High3072 , 3072)},
            {43, new MediaFileDataInfo(43 , Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.Medium360 , 360)},
            {44, new MediaFileDataInfo(44 , Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.Medium480 , 480)},
            {45, new MediaFileDataInfo(45 , Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.High720 , 720)},
            {46, new MediaFileDataInfo(46 , Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.High1080 , 1080)},
            {59, new MediaFileDataInfo(59 , Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480 , 480)},
            {78, new MediaFileDataInfo( 78, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480 , 480)},
            {82, new MediaFileDataInfo( 82, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium360 , 360)},
            {83, new MediaFileDataInfo( 83, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480 , 480)},
            {84, new MediaFileDataInfo( 84, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High720 , 720)},
            {85, new MediaFileDataInfo(85 , Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High1080 , 1080)},
            {91, new MediaFileDataInfo(91 , Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Low144 , 144)},
            {92, new MediaFileDataInfo( 92, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Low240 , 240)},
            {93, new MediaFileDataInfo( 93, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium360 , 360)},
            {94, new MediaFileDataInfo( 94, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Medium480 , 480)},
            {95, new MediaFileDataInfo( 95, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High720 , 720)},
            {96, new MediaFileDataInfo(96 , Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.High1080 , 1080)},
            {100, new MediaFileDataInfo( 100, Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.Medium360 , 360)},
            {101, new MediaFileDataInfo(101 , Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.Medium480 , 480)},
            {102, new MediaFileDataInfo( 102, Container.WebM, AudioEncoding.Vorbis, VideoEncoding.Vp8, VideoQuality.High720 , 720)},
            {132, new MediaFileDataInfo(132 , Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Low240 , 240)},
            {151, new MediaFileDataInfo( 151, Container.Mp4, AudioEncoding.Aac, VideoEncoding.H264, VideoQuality.Low144 , 144)},

            // Video-only (mp4)
            {133, new MediaFileDataInfo( 133, Container.Mp4, null, VideoEncoding.H264, VideoQuality.Low240 , 240)},
            {134, new MediaFileDataInfo(134 , Container.Mp4, null, VideoEncoding.H264, VideoQuality.Medium360 , 360)},
            {135, new MediaFileDataInfo( 135, Container.Mp4, null, VideoEncoding.H264, VideoQuality.Medium480 , 480)},
            {136, new MediaFileDataInfo( 136, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High720 , 720)},
            {137, new MediaFileDataInfo( 137, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1080 , 1080)},
            {138, new MediaFileDataInfo( 138, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High4320 , 4320)},
            {160, new MediaFileDataInfo( 160, Container.Mp4, null, VideoEncoding.H264, VideoQuality.Low144 , 144)},
            {212, new MediaFileDataInfo( 212, Container.Mp4, null, VideoEncoding.H264, VideoQuality.Medium480 , 480)},
            {213, new MediaFileDataInfo( 213, Container.Mp4, null, VideoEncoding.H264, VideoQuality.Medium480 , 480)},
            {214, new MediaFileDataInfo( 214, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High720 , 720)},
            {215, new MediaFileDataInfo( 215, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High720 , 720)},
            {216, new MediaFileDataInfo( 216, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1080 , 1080)},
            {217, new MediaFileDataInfo( 217, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1080 , 1080)},
            {264, new MediaFileDataInfo( 264, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1440 , 1440)},
            {266, new MediaFileDataInfo( 266, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High2160 , 2160)},
            {298, new MediaFileDataInfo( 298, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High720 , 720)},
            {299, new MediaFileDataInfo( 299, Container.Mp4, null, VideoEncoding.H264, VideoQuality.High1080 , 1080)},
            {399, new MediaFileDataInfo( 399, Container.Mp4, null, VideoEncoding.Av1, VideoQuality.High1080 , 1080)},
            {398, new MediaFileDataInfo( 398, Container.Mp4, null, VideoEncoding.Av1, VideoQuality.High720 , 720)},
            {397, new MediaFileDataInfo( 397, Container.Mp4, null, VideoEncoding.Av1, VideoQuality.Medium480 , 480)},
            {396, new MediaFileDataInfo( 396, Container.Mp4, null, VideoEncoding.Av1, VideoQuality.Medium360 , 360)},
            {395, new MediaFileDataInfo( 395, Container.Mp4, null, VideoEncoding.Av1, VideoQuality.Low240 , 240)},
            {394, new MediaFileDataInfo( 394, Container.Mp4, null, VideoEncoding.Av1, VideoQuality.Low144 , 144)},

            // Video-only (webm)
            {167, new MediaFileDataInfo( 167, Container.WebM, null, VideoEncoding.Vp8, VideoQuality.Medium360 , 360)},
            {168, new MediaFileDataInfo( 168, Container.WebM, null, VideoEncoding.Vp8, VideoQuality.Medium480 , 480)},
            {169, new MediaFileDataInfo( 169, Container.WebM, null, VideoEncoding.Vp8, VideoQuality.High720 , 720)},
            {170, new MediaFileDataInfo( 170, Container.WebM, null, VideoEncoding.Vp8, VideoQuality.High1080 , 1080)},
            {218, new MediaFileDataInfo( 218, Container.WebM, null, VideoEncoding.Vp8, VideoQuality.Medium480 , 480)},
            {219, new MediaFileDataInfo( 219, Container.WebM, null, VideoEncoding.Vp8, VideoQuality.Medium480 , 480)},
            {242, new MediaFileDataInfo( 242, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Low240 , 240)},
            {243, new MediaFileDataInfo( 243, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium360 , 360)},
            {244, new MediaFileDataInfo( 244, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium480 , 480)},
            {245, new MediaFileDataInfo( 245, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium480 , 480)},
            {246, new MediaFileDataInfo( 246, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium480 , 480)},
            {247, new MediaFileDataInfo( 247, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High720 , 720)},
            {248, new MediaFileDataInfo( 248, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1080 , 1080)},
            {271, new MediaFileDataInfo( 271, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1440 , 1440)},
            {272, new MediaFileDataInfo( 272, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High2160 , 2160)},
            {278, new MediaFileDataInfo( 278, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Low144 , 144)},
            {302, new MediaFileDataInfo( 302, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High720 , 720)},
            {303, new MediaFileDataInfo( 303, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1080 , 1080)},
            {308, new MediaFileDataInfo( 308, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1440 , 1440)},
            {313, new MediaFileDataInfo( 313, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High2160 , 2160)},
            {315, new MediaFileDataInfo( 315, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High2160 , 2160)},
            {330, new MediaFileDataInfo( 330, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Low144 , 144)},
            {331, new MediaFileDataInfo( 331, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Low240 , 240)},
            {332, new MediaFileDataInfo( 332, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium360 , 360)},
            {333, new MediaFileDataInfo( 333, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.Medium480 , 480)},
            {334, new MediaFileDataInfo( 334, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High720 , 720)},
            {335, new MediaFileDataInfo( 335, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1080 , 1080)},
            {336, new MediaFileDataInfo( 336, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High1440 , 1440)},
            {337, new MediaFileDataInfo( 337, Container.WebM, null, VideoEncoding.Vp9, VideoQuality.High2160 , 2160)},

            // Audio-only (mp4)
            {139, new MediaFileDataInfo(139 , Container.M4A, AudioEncoding.Aac, null, null, null)},
            {140, new MediaFileDataInfo(140 , Container.M4A, AudioEncoding.Aac, null, null, null)},
            {141, new MediaFileDataInfo(141 , Container.M4A, AudioEncoding.Aac, null, null, null)},
            {256, new MediaFileDataInfo(256 , Container.M4A, AudioEncoding.Aac, null, null, null)},
            {258, new MediaFileDataInfo(258 , Container.M4A, AudioEncoding.Aac, null, null, null)},
            {325, new MediaFileDataInfo(325 , Container.M4A, AudioEncoding.Aac, null, null, null)},
            {328, new MediaFileDataInfo(328 , Container.M4A, AudioEncoding.Aac, null, null, null)},

            // Audio-only (webm)
            {171, new MediaFileDataInfo(171 , Container.WebM, AudioEncoding.Vorbis, null, null, null)},
            {172, new MediaFileDataInfo(172 , Container.WebM, AudioEncoding.Vorbis, null, null, null)},
            {249, new MediaFileDataInfo(249 , Container.WebM, AudioEncoding.Opus, null, null, null)},
            {250, new MediaFileDataInfo(250 , Container.WebM, AudioEncoding.Opus, null, null, null)},
            {251, new MediaFileDataInfo(251 , Container.WebM, AudioEncoding.Opus, null, null, null)}
        };

        public static Container GetContainer(int itag)
        {
            var result = ItagMap.GetOrDefault(itag)?.Container;

            if (!result.HasValue)
                throw new ArgumentOutOfRangeException(nameof(itag), $"Unexpected itag [{itag}].");

            return result.Value;
        }

        public static AudioEncoding GetAudioEncoding(int itag)
        {
            var result = ItagMap.GetOrDefault(itag)?.AudioEncoding;

            if (!result.HasValue)
                throw new ArgumentOutOfRangeException(nameof(itag), $"Unexpected itag [{itag}].");

            return result.Value;
        }

        public static VideoEncoding GetVideoEncoding(int itag)
        {
            var result = ItagMap.GetOrDefault(itag)?.VideoEncoding;

            if (!result.HasValue)
                throw new ArgumentOutOfRangeException(nameof(itag), $"Unexpected itag [{itag}].");

            return result.Value;
        }

        public static VideoQuality GetVideoQuality(int itag)
        {
            var result = ItagMap.GetOrDefault(itag)?.VideoQuality;

            if (!result.HasValue)
                throw new ArgumentOutOfRangeException(nameof(itag), $"Unexpected itag [{itag}].");

            return result.Value;
        }

        public static bool IsKnown(int itag)
        {
            return ItagMap.ContainsKey(itag);
        }

        public static MediaFileDataInfo FindByItag(int itag)
        {
            if (IsKnown(itag))
            {
                return ItagMap[itag];
            }
            else return null;
        }
    }
}