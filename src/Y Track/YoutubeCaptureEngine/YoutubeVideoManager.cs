using FFMpeg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Y_Track.Extentions;
using Y_Track.Helpers;
using Y_Track.Network;
using Y_Track.YoutubeCaptureEngine.Models;
using Y_Track.YoutubeCaptureEngine.Models.Exceptions;

namespace Y_Track.YoutubeCaptureEngine
{
    public class YoutubeVideoManager
    {

        /// <summary>
        /// the Video object managed by YoutubeVideoTracker
        /// </summary>
        public YoutubeVideo Video { get; private set; }

        /// <summary>
        /// set to true if the video is being stored , i.e DownloadAndMutiplex was called
        /// </summary>
        public bool VideoBeingStored { get; private set; } = false;

        /// <summary>
        /// set to true if the video is successfully downloaded,multiplexed and copied to final distination
        /// </summary>
        public bool VideoStored { get; private set; } = false;


        /// <summary>
        /// fires when an exception(s) are thrown while downloading a gab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void PacketGabDownloadExceptionThrown(object sender, PacketDownloadExceptionEventArgumanets e);
        public event PacketGabDownloadExceptionThrown _onGabPacketDownloadExceptionThrown;


        /// <summary>
        /// fires when youtube video is successfully downloaded,multiplexed and copied to final distination
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="finalVideoPath"></param>
        public delegate void YoutubeVideoStored(object manager, string finalVideoPath);
        public event YoutubeVideoStored OnYoutubeStored;


        /// <summary>
        /// fires when the Progress of packet downloading changed
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="percentage"></param>
        public delegate void PacketDownloadProgressChanged(object manager, int percentage);
        public event PacketDownloadProgressChanged OnPacketDownloadProgressChanged;


        /// <summary>
        /// fires when a packet is finished downloading
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="packet"></param>
        public delegate void PacketDownloadCompleted(object manager, YoutubeMediaPacket packet);
        public event PacketDownloadCompleted OnPacketDownloadCompleted;


        /// <summary>
        /// fires when a packets is arrived and the end of it's range equals the length of the video (does not guarantee that the video packets are complete)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="f"></param>
        public delegate void YoutubeLastPacketRecieved(object sender, YoutubeMediaPacket f);
        public event YoutubeLastPacketRecieved OnYoutubeLastPacketRecieved;

        private bool _mediaInfoBeingFetched = false;
        private readonly HttpClient _client;

        public YoutubeVideoManager(YoutubeVideo youtubeVideo, HttpClient client)
        {
            Video = youtubeVideo;
            _client = client;
        }


        /// <summary>
        /// validate the new packet and add it the video packets
        /// </summary>
        /// <param name="newPacketToAdd"></param>
        public void AddPacketFile(YoutubeMediaPacket newPacketToAdd)
        {

            // if the video is stored or being stored don't add any packets
            if (this.VideoBeingStored || VideoStored) return;

            Debug.WriteLine("adding " + newPacketToAdd.VideoPacketType.ToString() + " Packet : " + newPacketToAdd.Range.Start + "-" + newPacketToAdd.Range.End);

            // check if the new coming packets are having a different video quality than exiting packets 
            // if so .. remove them and consider all of them as a gabs to fill in the final step
            if (newPacketToAdd.VideoPacketType == YoutubeMediaPacketType.Video
                && newPacketToAdd.PacketMediaFileDataInfo.VerticalResolution.HasValue
                && newPacketToAdd.PacketMediaFileDataInfo.VerticalResolution != this.Video.CurrentVideoVerticalRosolution) // to make sure not considering the first packet as a gab ... because _currentHigherVideoVerticalRosolution starts by 0
            {
                this.Video.CurrentVideoVerticalRosolution = newPacketToAdd.PacketMediaFileDataInfo.VerticalResolution.Value;
                // truncating all previous packets 
                _truncateVideoPackets(newPacketToAdd.VideoPacketType);
            }

            // this prevent packets from being intersecting   
            // intersecting packets are packets that has it range intersect with the previous packets
            // main cause of these packets is the inturrupting video ads that rebuffer a part of stream suddenly
            bool newComingIntersectingPacketAlreadyStored = this.Video.Packets.ToList().Any(x => x.Range.End + 1 > newPacketToAdd.Range.Start
            && x.VideoPacketType == newPacketToAdd.VideoPacketType);

            if (!newComingIntersectingPacketAlreadyStored)
            {
                // i will add the new packet if it's range start is greater than all stored packets range's end
                this.Video.Packets.Add(newPacketToAdd);
            }
            _checkIfLastPacket(newPacketToAdd);
        }


        /// <summary>
        /// check if the video have gabs and begin downloading/multiplixeng it after filling them
        /// </summary>
        /// <param name="outputPath"></param>
        /// <returns></returns>
        public async Task<bool> DownloadAndMultiplix(string outputPath)
        {

            if (VideoBeingStored || VideoStored) throw new Exception("The Video You are going to store is stored already  or being stored");
            // set the flag in case the video is being saved
            VideoBeingStored = true;

            // fill the gabs (a gab simply is a removed different quality packet or a lost packet when the user seek for unbuffered location)
            await _handleGabs();

            string videoFileName = Path.GetTempFileName();
            string audioFileName = Path.GetTempFileName();
            await this.PopulateToFile(videoFileName, audioFileName);
            _muxMediaFiles(outputPath, videoFileName, audioFileName);
            return true;
        }


        /// <summary>
        /// initiate and new request using YoutubeExploder Library and populate video info
        /// </summary>
        /// <returns></returns>
        public async Task PopulateMediaInfo()
        {
            var packetWithVideoId = this.Video.Packets.FirstOrDefault(x => x.VideoId != null);
            if (packetWithVideoId != null)
            {
                if (!string.IsNullOrEmpty(packetWithVideoId.VideoId))
                {
                    // making sure that the video info is not already fetched or being fetched 
                    if (this.Video.VideoInfo == null && !_mediaInfoBeingFetched)
                    {
                        _mediaInfoBeingFetched = true;
                        await _fetchYoutubeInfo(packetWithVideoId.VideoId);
                    }
                }
            }
        }

        /// <summary>
        /// Populate both video stream and Audio Stream each to a specific file path
        /// </summary>
        /// <param name="videoFilePath"></param>
        /// <param name="audioFilePath"></param>
        /// <returns></returns>
        public async Task PopulateToFile(string videoFilePath, string audioFilePath)
        {
            if (!File.Exists(videoFilePath)) throw new FileNotFoundException("File not found : ", videoFilePath);
            if (!File.Exists(audioFilePath)) throw new FileNotFoundException("File not found : ", audioFilePath);

            // populate video packets
            await _populate(YoutubeMediaPacketType.Video, videoFilePath);
            // populate audio packets
            await _populate(YoutubeMediaPacketType.Audio, audioFilePath);
        }



        private void _truncateVideoPackets(YoutubeMediaPacketType type = YoutubeMediaPacketType.Video)
        {
            this.Video.Packets.RemoveAll(x => x.VideoPacketType == type);
        }


        private void _checkIfLastPacket(YoutubeMediaPacket packet)
        {
            // Get the last packet added to videos packets and the audios packets
            var lastVideoPacketAdded = _getTheLastAddedMediaPacket(YoutubeMediaPacketType.Video);
            var lastAudioPacketAdded = _getTheLastAddedMediaPacket(YoutubeMediaPacketType.Audio);

            // if the video does not recieve audio or video packet then don't proceed
            if (lastVideoPacketAdded == null || lastAudioPacketAdded == null) return;

            // if the packet is the last packet from the video stream proceed
            if (_doesLastAudioAndVideoPacketsRecieved())
            {
                // now I'll raise OnYoutubeLastPacketRecieved 
                OnYoutubeLastPacketRecieved?.Invoke(this, packet);
            }
        }


        private bool _doesLastAudioAndVideoPacketsRecieved()
        {
            // check if the last inserted packet to video is the last one
            var lastAddedVideoPacket = _getTheLastAddedMediaPacket(YoutubeMediaPacketType.Video);
            var lastAddedAudioPacket = _getTheLastAddedMediaPacket(YoutubeMediaPacketType.Audio);

            // the last packet's range end should be the equal to the total length
            return lastAddedVideoPacket.Range.End == lastAddedVideoPacket.OverAllLength - 1
                && lastAddedAudioPacket.Range.End == lastAddedAudioPacket.OverAllLength - 1;
        }





        private string _getFFmpegPath()
        {
            var appDirectory = Path.GetDirectoryName(Path.Combine(Assembly.GetEntryAssembly().Location));
            return appDirectory + @"\FFMpeg\lib\FFMpeg.exe";
        }

        private void _muxMediaFiles(string outputPath, string videoFileName, string audioFileName)
        {
            var ffmpegPath = _getFFmpegPath();
            YTrackLogger.Log(ffmpegPath);


            FFMpegMultiplexer fFMpegMuxer = new FFMpegMultiplexer(ffmpegPath, videoFileName, audioFileName, Properties.Settings.Default.FallBackToEncodeMedia);
            fFMpegMuxer.OnMultiplixingComplete += (s, meea) =>
            {
                try
                {
                    string finalVideoPath = outputPath + meea.OutputFileExtention;
                    // copy muxed video to the final output path
                    File.Copy(meea.OutputFilePath, finalVideoPath, true);
                    // set the stored flag
                    this.VideoStored = true;
                    // firing youtubeStored
                    OnYoutubeStored?.Invoke(this, finalVideoPath);
                }
                catch
                {
                    YTrackLogger.Log("Cannot Move Output file from temp to Output Path : " + outputPath);
                    throw;
                }
            };
            if (fFMpegMuxer.Muliplix() == null)
            {
                YTrackLogger.Log("Unable to Start FFMpeg\n" + "Input Video : " + videoFileName + "\nInput Audio : " + audioFileName);
                throw new Exception("Unable to Start FFMpeg");
            }
        }



        private async Task _fetchYoutubeInfo(string youtubeVideoId)
        {
            var client = new YoutubeExplode.YoutubeClient();
            YoutubeExplode.Models.Video video = null;
            try
            {
                video = await client.GetVideoAsync(youtubeVideoId);
                this.Video.VideoInfo = YoutubeVideoInfo.FromVideoObject(video);
            }
            catch (Exception e)
            {
                _mediaInfoBeingFetched = false;
                YTrackLogger.Log("Cannot Fetch Video info for " + youtubeVideoId + " : \n\n" + e.Message);
            }
        }




        private async Task<bool> _populate(YoutubeMediaPacketType type, string filePath)
        {
            return await Task.Run(() =>
            {
                Debug.WriteLine(type + " packets clen : " + this.Video.Packets.Where(x => x.VideoPacketType == type).First().OverAllLength);
                var sorted = this.Video.Packets.Where(x => x.VideoPacketType == type).OrderBy(x => x.Range.Start).ToList();
                sorted.ForEach(item =>
                {
                    Debug.WriteLine("Appending Packet : " + item.VideoPacketType.ToString() + " " + item.Range.Start + "-" + item.Range.End + " :: " + item.FilePath);

                    Helpers.Misc.AppendAllBytes(filePath, File.ReadAllBytes(item.FilePath));
                });
                return true;
            });

        }

        private async Task _handleGabs()
        {
            // determine the broken video packets and download them
            await _fillVideoGabs(YoutubeMediaPacketType.Video);
            // determine the broken audio packets and download them
            await _fillVideoGabs(YoutubeMediaPacketType.Audio);
        }




        private async Task _fillVideoGabs(YoutubeMediaPacketType type)
        {
            // compute the broken ranges for both videos packets and audio packets
            List<Range> LostPacketsRanges = _computeBrokenRanges(type);
            // fill the missing packets
            await _fillBrokenRanges(LostPacketsRanges, type);
        }



        private List<Range> _computeBrokenRanges(YoutubeMediaPacketType type)
        {
            YoutubeMediaPacket[] storedPackets;
            lock (this.Video.Packets)
            {
                storedPackets = this.Video.Packets.Where(x => x.VideoPacketType == type).OrderBy(x => x.Range.Start).ToArray();
            }

            var lostPacketsRanges = new List<Range>();

            // checking if there is a missing packets at start or end of media
            var firstPacket = storedPackets.FirstOrDefault();
            var lastPacket = storedPackets.LastOrDefault();

            // at start
            if (firstPacket != null && firstPacket.Range.Start != 0)
            {
                lostPacketsRanges.Add(new Range(0, firstPacket.Range.Start - 1));
            }

            // at end
            if (lastPacket != null && lastPacket.Range.End + 1 != lastPacket.OverAllLength)
            {
                lostPacketsRanges.Add(new Range(lastPacket.Range.End + 1, lastPacket.OverAllLength - 1));
            }

            // checking if there is a missing packets at middle of the video packets
            for (int i = 0; i < storedPackets.Length; i++)
            {
                // to make sure the counter don't exceed the array bounds when declaring nextPacket
                if (i >= storedPackets.Length - 1) break;


                var currentPacket = storedPackets[i];
                var nextPacket = storedPackets[i + 1];

                // a gap will appear when two consecutive ranges are broken 
                // i.e when the end of one packet range + 1 not equal to the next packet's range start 
                if (currentPacket.Range.End + 1 != nextPacket.Range.Start)
                {
                    // two cases may happen here 
                    // 1- a user seeking to unbuffer location so we have a gab
                    // 2- a buffered location faced an inturrupting ad 
                    // case #2 is handled in AddPacket (newComingIntersectWithAlreadyStored flag) so it will not occure here
                    // i will add broken ranges to lostPacketsRanges to handle them later
                    var start = currentPacket.Range.End + 1;
                    var end = nextPacket.Range.Start - 1;
                    lostPacketsRanges.Add(new Range(start, end));
                }
            }
            return lostPacketsRanges;
        }



        private async Task _fillBrokenRanges(List<Range> lostPacketsRanges, YoutubeMediaPacketType lostPacketsType)
        {
            // debugging purposes
            if (lostPacketsRanges.Count > 0)
            {
                YTrackLogger.Log("\nBroken Ranges for type " + lostPacketsType + " : \n");
                lostPacketsRanges.ForEach(x =>
                {
                    YTrackLogger.Log(x.ToString());
                });
                YTrackLogger.Log("\n\n");
            }


            // constructing the lost packates new requests and fetch them
            // firstly i will get clone from  first packet url 
            // no need for higher quality checks because lower ones are already removed
            var lostPacketRequestURL = this.Video.Packets.FirstOrDefault(x => x.VideoPacketType == lostPacketsType)?.YoutubeRequestURL?.DeepClone();


            // Youtube Contains M4A containers .... the video may contain only media with Audio content type 
            // if no video packets are present return
            if (lostPacketRequestURL == null) return;


            // now loop throught all lost packets and fill them 
            foreach (var lostRange in lostPacketsRanges)
            {
                var newPacket = await _fillGab(lostPacketRequestURL, lostRange, lostPacketsType);
                _insertGabPacket(newPacket);
            }
        }

        private async Task<YoutubeMediaPacket> _fillGab(YoutubeRequestURL packetRequestURL, Range lostRange, YoutubeMediaPacketType type)
        {
            packetRequestURL.RequestPath.QueryString.SetValue("range", $"{lostRange.Start}-{lostRange.End}");
            var requestableURL = packetRequestURL.ToRequestableURL();

            // debugging
            YTrackLogger.Log("\nfilling broken range  for type " + type + " : " + lostRange.ToString() + " from : " + requestableURL);

            long httpSegmentSize = packetRequestURL.RequestPath.QueryString.HasValue("ratebypass") && packetRequestURL.RequestPath.QueryString.GetValue("ratebypass") == "yes"
                ? (long)lostRange.Length
                : 9_898_989;
            YoutubeMediaPacket packet;


            using (SegmentedHttpStream segmentedHttpStream = new SegmentedHttpStream(_client, requestableURL, (long)lostRange.Length, httpSegmentSize))
            {
                try
                {
                    string tmpFileName = Path.GetTempFileName();
                    using (Stream outputStream = new FileStream(tmpFileName, FileMode.Append))
                    {
                        IProgress<double> progressPercentage = new Progress<double>(b => packetDownloadProgressChanged(b));
                        await segmentedHttpStream.CopyToStreamAsync(outputStream, progressPercentage);
                    }
                    // no need to check whether the packetRequestURL is a valid requist URL
                    // because we requested it anyway
                    packet = new YoutubeMediaPacket(type, packetRequestURL, tmpFileName);
                    OnPacketDownloadCompleted?.Invoke(this, packet);
                }
                catch (Exception e)
                {
                    // two exception may be thrown 
                    // WebException if something went wrong when fetch the new packets
                    // IOException if something went wrong when writing the packet file to disk
                    // I'll raise PacketDownloadExceptionThrown and re-throw exception
                    if (e is WebException)
                    {
                        _onGabPacketDownloadExceptionThrown?.Invoke(this, new PacketDownloadExceptionEventArgumanets()
                        {
                            Exception = e as WebException,
                            FailedPacketRequestURL = packetRequestURL,
                            Range = lostRange
                        });
                    }

                    // re-throw the error to the caller
                    Helpers.YTrackLogger.Log("Failed to download Packet : " + e.Message + "\n\n" + e.StackTrace);
                    throw;
                }
            }
            return packet;
        }


        private void _insertGabPacket(YoutubeMediaPacket packetToInsert)
        {
            lock (this.Video.Packets)
            {
                // gab is the first packet
                if (packetToInsert.Range.Start == 0)
                {
                    this.Video.Packets.Insert(0, packetToInsert);
                }
                // gab is the last packet
                else if (packetToInsert.Range.End + 1 == packetToInsert.OverAllLength)
                {
                    this.Video.Packets.Insert(this.Video.Packets.Count - 1, packetToInsert);
                }
                // gab is in the middle of the video packets
                else
                {
                    // see where the previous packet is and add it after
                    int previousPacketIndex = this.Video.Packets.FindIndex(x => (x.Range.End + 1) == packetToInsert.Range.Start);
                    this.Video.Packets.Insert(++previousPacketIndex, packetToInsert);
                }
            }

        }



        private void packetDownloadProgressChanged(double progress)
        {
            int progressPercentage = (int)(progress * 100);
            this.OnPacketDownloadProgressChanged?.Invoke(this, progressPercentage);
            Debug.WriteLine("Progress For Packet Downloading : " + progressPercentage);
        }

        private YoutubeMediaPacket _getTheLastAddedMediaPacket(YoutubeMediaPacketType typeToSearchFor)
        {
            // lock is for "Exception thrown: 'System.ArgumentException' in mscorlib.dll
            // An exception of type 'System.ArgumentException' occurred in mscorlib.dll but was not handled in user code
            // Destination array was not long enough. Check destIndex and length, and the array's lower bounds."

            lock (this.Video)
            {
                return this.Video.Packets.ToList() // ToList to avoid (Collection was modified; enumeration operation may not execute) Exception .. see https://stackoverflow.com/questions/604831/collection-was-modified-enumeration-operation-may-not-execute
                  .Where(x => x.VideoPacketType == typeToSearchFor)
                  .OrderByDescending(x => x.Range.End).FirstOrDefault();
            }

        }








    }
}
