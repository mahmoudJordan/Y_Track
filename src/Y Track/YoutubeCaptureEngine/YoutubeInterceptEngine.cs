using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;
using Y_Track.Extentions;
using Y_Track.Helpers;
using Y_Track.Titanium;
using Y_Track.YoutubeCaptureEngine.Models;

namespace Y_Track.YoutubeCaptureEngine
{
    public class YoutubeInterceptEngine
    {


        /// <summary>
        /// fires when a new video is downloaded and multiplexed 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="physicalPath"></param>
        public delegate void NewVideoStored(YoutubeVideoManager manager, string physicalPath);
        public event NewVideoStored OnNewYoutubeStored;


        /// <summary>
        /// fires when a packets is arrived and the end of it's range equals the length of the video (does not guarantee that the video packets are complete)
        /// </summary>
        /// <param name="manager"></param>
        public delegate void NewVideoLastPacketRecieved(YoutubeVideoManager manager);
        public event NewVideoLastPacketRecieved OnNewVideoLastPacketRecieved;

        /// <summary>
        /// Fires when user enters main youtube page or any watch page
        /// </summary>
        public event EventHandler OnYoutubeSessionDetected;


        /// <summary>
        /// fires when user exit youtube video or change it 
        /// </summary>
        /// <param name="manager">current Youtube Manager</param>
        /// <param name="messageType">chatter message type (closed/changed)</param>
        public delegate void YoutubeChatterClientMessageRecieved(YoutubeVideoManager manager, YoutubeChatterClientMessageType messageType);
        public event YoutubeChatterClientMessageRecieved OnYoutubeChatterClientMessageRecieved;


        /// <summary>
        /// Observable collection for per-video created video managers
        /// </summary>
        public ObservableCollection<YoutubeVideoManager> VideosManagers;


        /// <summary>
        /// indicates if the interceptor is running
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// fetch the system proxy for both http/https from registry
        /// </summary>
        public string SystemProxyAddress => getSystemProxyEndPoint();


        /// <summary>
        /// default output directory for output videos if DownloadAndMux method doen't provided with one
        /// </summary>
        public string OutputDefaultDirectory { get; set; }


        private readonly string YOUTUBE_INJECTED_CLIENT_FILE_PATH = @"\YoutubeCaptureEngine\ClientTracker\YoutubeChatterClient.js";
        private readonly string DUMMY_CLIENT_CHATTER_PATH = "www.youtube_chatter_dummy.com";
        private string TrackingScript;
        private List<string> _downloadedBefore = new List<string>();
        private Titanium.TitaniumManager _titaniumManager;
        private HttpClient _client;

        private static YoutubeInterceptEngine _instance;

        /// <summary>
        /// get a singleton candidate from the YoutubeInterceptEngine class to use all over the application
        /// </summary>
        public static YoutubeInterceptEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new YoutubeInterceptEngine();
                    _instance._titaniumManager = Titanium.TitaniumManager.Instance;
                    _instance.TrackingScript = _instance._readTrackerScript();
                    _instance.VideosManagers = new ObservableCollection<YoutubeVideoManager>();
                    _instance._client = new HttpClient();
                };
                return _instance;
            }
        }


        /// <summary>
        /// protected constructor to prevent initiating another instance of YoutubeInterceptEngine
        /// </summary>
        protected YoutubeInterceptEngine() { }


        /// <summary>
        /// list of youtube video/audio http content types
        /// </summary>
        public readonly string[] YoutubeVideoMIMEs = new string[]
        {
            "text/event-stream"
            , "multipart/x-mixed-replace"
            , "video/"
            , "audio/"
            , "application/x-mms-framed"
        };





        private string _readTrackerScript()
        {
            var appDirectory = Path.GetDirectoryName(Path.Combine(Assembly.GetEntryAssembly().Location));
            var trackerScriptPath = appDirectory + _instance.YOUTUBE_INJECTED_CLIENT_FILE_PATH;
            return File.ReadAllText(trackerScriptPath);
        }





        private string getSystemProxyEndPoint()
        {

            try
            {
                return SystemProxyManager.GetSystemProxyEndPoint();
            }
            catch
            {
                Y_Track.Helpers.YTrackLogger.Log("Cannot read ProxyServer URI");
                return null;
            }
        }





        /// <summary>
        /// starts youtube intercepting
        /// </summary>
        /// <param name="proxyPort">port for the MLIMA proxy</param>
        public async void StartIntercepting(int proxyPort)
        {
            if (!IsStarted)
            {
                var started = await _titaniumManager.Start(proxyPort);
                _titaniumManager.Proxy.BeforeRequest += Proxy_BeforeRequest;
                _titaniumManager.Proxy.BeforeResponse += Proxy_BeforeResponse;
                IsStarted = true;
            }
        }



        /// <summary>
        /// Stops the interceptor
        /// </summary>
        public async void StopIntercepting()
        {
            if (IsStarted)
            {
                _titaniumManager.Proxy.BeforeRequest -= Proxy_BeforeRequest;
                _titaniumManager.Proxy.BeforeRequest -= Proxy_BeforeResponse;
                await _titaniumManager.Stop();
                IsStarted = false;
            }
        }


        private Task Proxy_BeforeRequest(object sender, SessionEventArgs session)
        {

            if (isYoutubeChatterMessage(session))
            {
                _handleChatterMessage(session?.HttpClient?.Request.RequestUri?.AbsoluteUri);
            }

            return Task.FromResult(0);
        }

        private async Task Proxy_BeforeResponse(object sender, SessionEventArgs session)
        {
            if (isYoutubeInjectablePage(session))
            {
                await _injectTrackingChatterScript(session);

                // see if this is youtube main page  response
                if (isYoutubeMainPageDetected(session))
                {
                    // raise session detection event
                    this.OnYoutubeSessionDetected?.Invoke(this, EventArgs.Empty);
                }
            }

            // if this is a chatter message return 200 response 
            // Technically this response will be ignored from the client tracker side)
            if (isYoutubeChatterMessage(session))
            {
                session.Ok("<b>OK</b>");
            }

            // handle youtube packets
            if (isYoutubeVideoPacket(session))
            {
                await _handleYoutubePacket(session);
            }
        }






        private bool isYoutubeMainPageDetected(SessionEventArgs session)
        {
            return (session?.HttpClient?.Request.RequestUri?.AbsolutePath == "/"
                && session?.HttpClient?.Request.Host == "www.youtube.com");
        }



        private async Task _injectTrackingChatterScript(SessionEventArgs session)
        {
            // getting the request body 
            string reponseString = await session.GetResponseBodyAsString();
            // finding end head tag and replace it with the javascript tracker
            string injectedResponse = reponseString.Replace("</head>", "<script>" + this.TrackingScript + "</script>" + "</head>");
            // turning the injected document into byte[] 
            byte[] toInjectBytes = System.Text.Encoding.UTF8.GetBytes(injectedResponse);
            // replace the response with the injected one
            session.Ok(toInjectBytes);
        }




     

        private void _handleChatterMessage(string requestPath)
        {
            var message = YoutubeChatterClientHandler.Instance.ParseRequestPath(requestPath);
            switch (message.MessageType)
            {
                case YoutubeChatterClientMessageType.VideoChanged:
                case YoutubeChatterClientMessageType.VideoClosed:
                    var videoManager = this.VideosManagers.Where(x => x.Video.Packets.Any(p => p.VideoId == message.VideoId)).FirstOrDefault();
                    if (videoManager != null)
                    {
                        if (videoManager.VideoBeingStored || videoManager.VideoStored) return;
                        OnYoutubeChatterClientMessageRecieved?.Invoke(videoManager, message.MessageType);
                    }
                    break;
            }
        }

        private bool isYoutubeChatterMessage(SessionEventArgs session)
        {
            // is this coming from youtube chatter javascript client ?
            return
                session?.HttpClient?.Request.RequestUri?.AbsolutePath != null
                && session?.HttpClient?.Request.RequestUri?.AbsolutePath.IndexOf(DUMMY_CLIENT_CHATTER_PATH) > -1;
        }

        private void _handleCachingHeaders(ref SessionEventArgs session)
        {

            // forcing to response reply with text/html pure page without any compression
            session.HttpClient.Request.Headers.RemoveHeader("Accept-Encoding");

            // prevent the browser from caching the main/watch page 
            // to make sure the tracker is injected with each reponse (prevent caching it)
            session.HttpClient.Request.Headers.RemoveHeader("Last-Modefied");
            session.HttpClient.Request.Headers.RemoveHeader("ETag");

            session.HttpClient.Request.Headers.RemoveHeader("Expires");
            session.HttpClient.Request.Headers.AddHeader("Expires", "0");
            session.HttpClient.Request.Headers.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            session.HttpClient.Request.Headers.AddHeader("Pragma", "no-cache");
            session.HttpClient.Request.Headers.AddHeader("Vary", "*");
        }

        private bool isYoutubeInjectablePage(SessionEventArgs session)
        {
            // TODO :: check also if this is a html page by content-type
            // if the session is originated from the watch?v={video_id} page or the main page return true
            return (session.HttpClient.Request.RequestUri.AbsolutePath == "/" || session.HttpClient.Request.RequestUri.AbsolutePath.IndexOf("/watch") > -1)
                && session.HttpClient.Request.Host == "www.youtube.com";
        }






        private bool isYoutubeVideoPacket(SessionEventArgs session)
        {
            var isMediaContentType = isMediaContent(session);

            if (!isMediaContentType) return false;
            var isPlayback = isYoutubePlayback(session);
            var isYoutubeOriginated = isYoutubeOriginatedMedia(session);
            return
                isMediaContentType && isPlayback && isYoutubeOriginated
                // check if the request are not coming from YTrack 
                // if it's so then it's comming from PlayerWindow and we don't want to intercept it
                // if session.LocalProcessID is 0 then MITM Proxy can't say from where the request came, 
                // in this case i'll assume it's not coming from PlayerWindow
                && (session.HttpClient.ProcessId.Value != Process.GetCurrentProcess().Id || session.HttpClient.ProcessId.Value == 0);
        }

        private bool isYoutubeOriginatedMedia(SessionEventArgs session)
        {
            // check if playback is originated by youtube
            return (session.HttpClient.Request.Headers.Headers.ContainsKey("Origin") && session.HttpClient.Request.Headers.Headers["Origin"].Value.IndexOf("https://www.youtube.com") > -1)
            ||
            (session.HttpClient.Request.Headers.Headers.Keys.Contains("Timing-Allow-Origin") && session.HttpClient.Request.Headers.Headers["Timing-Allow-Origin"].Value.IndexOf("https://www.youtube.com") > -1);
        }

        private bool isYoutubePlayback(SessionEventArgs session)
        {
            // check if media data comes from youtube playback
            return session.HttpClient.Request.RequestUri.AbsolutePath.Contains("/videoplayback");
        }

        private bool isMediaContent(SessionEventArgs session)
        {
            // check if the response contains media data
            return (session.HttpClient.Response.Headers.Headers.ContainsKey("Content-Type")
            && session.HttpClient.Response.Headers.Headers["Content-Type"].Value.StartWithAny(YoutubeVideoMIMEs));
        }


        private async Task _handleYoutubePacket(SessionEventArgs session)
        {

            // check if the packet in session is Audio or Video
            YoutubeMediaPacketType PacketType = _parseMediaPacketType(session.HttpClient.Response.Headers.Headers["Content-Type"].Value);

            // if content type is not video nor audio cancel it
            if (PacketType == YoutubeMediaPacketType.Unknown) return;

            var requestURL = requestURLFromSession(session);

            // making sure that the packet is valid to parse
            if (!YoutubeMediaPacket.IsValidYoutubeRequestURL(requestURL)) return;

            byte[] bodyBytes = await session.GetResponseBody();
            var newPacket = new YoutubeMediaPacket(PacketType, requestURL, bodyBytes);

            // if videos list contains a video with the same fingerprint append the packet to it 
            // otherwise create another video and add new packet to
            bool isVideoAddedBefore = this.VideosManagers.Any(x => x.Video.VideoFingerPrint == newPacket.VideoFingerPrint);
            if (!isVideoAddedBefore)
            {
                var newVideo = new YoutubeVideo(newPacket.VideoFingerPrint);
                var newVideoManager = new YoutubeVideoManager(newVideo, _client);
                VideosManagers.Add(newVideoManager);
                newVideoManager.OnYoutubeLastPacketRecieved += videoLastPacketRecieved;
                newVideoManager.OnYoutubeStored += NewVideoManager_OnYoutubeStored;
                newVideoManager.AddPacketFile(newPacket);
            }
            else
            {
                var packetVideo = VideosManagers.Where(x => x.Video.VideoFingerPrint == newPacket.VideoFingerPrint).FirstOrDefault();
                packetVideo.AddPacketFile(newPacket);
            }
        }


        private YoutubeMediaPacketType _parseMediaPacketType(string sessionContentType)
        {
            // returns the videoMediaType from a contentType header
            YoutubeMediaPacketType PacketType = YoutubeMediaPacketType.Unknown;

            if (sessionContentType.StartsWith("video"))
                PacketType = YoutubeMediaPacketType.Video;

            if (sessionContentType.StartsWith("audio"))
                PacketType = YoutubeMediaPacketType.Audio;

            return PacketType;
        }



        private YoutubeRequestURL requestURLFromSession(SessionEventArgs session)
        {
            YoutubePlaybackRequestPath youtubeRequestPath = YoutubePlaybackRequestPathParser.ParseRequestPathString(session.HttpClient.Request.RequestUri.PathAndQuery);
            string host = session.HttpClient.Request.Host;
            return new YoutubeRequestURL()
            {
                RequestPath = youtubeRequestPath,
                Host = host
            };
        }



        private void NewVideoManager_OnYoutubeStored(object sender, string finalVideoPath)
        {
            var manager = sender as YoutubeVideoManager;
            OnNewYoutubeStored?.Invoke(manager, finalVideoPath);
        }

        private void videoLastPacketRecieved(object sender, YoutubeMediaPacket lastPacket)
        {
            YoutubeVideoManager videoManager = sender as YoutubeVideoManager;
            OnNewVideoLastPacketRecieved?.Invoke(videoManager);
        }


    }
}
