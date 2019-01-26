using Fiddler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Y_Track.Extentions;
using Y_Track.Fiddler;
using Y_Track.Helpers;
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
        private FiddlerApplicationManager _fiddlerManager;
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
                    _instance._fiddlerManager = new FiddlerApplicationManager();
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


        /// <summary>
        /// starts youtube intercepting
        /// </summary>
        /// <param name="proxyPort">port for the MLIMA proxy</param>
        public async void StartIntercepting(int proxyPort)
        {
            if (!IsStarted)
            {
                await _fiddlerManager.StartFiddler(proxyPort);
                FiddlerApplication.AfterSessionComplete += YoutubeHandler;
                FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
                FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;
                FiddlerApplication.ResponseHeadersAvailable += FiddlerApplication_ResponseHeadersAvailable;
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
                FiddlerApplication.AfterSessionComplete -= YoutubeHandler;
                FiddlerApplication.BeforeRequest -= FiddlerApplication_BeforeRequest;
                FiddlerApplication.BeforeResponse -= FiddlerApplication_BeforeResponse;
                FiddlerApplication.ResponseHeadersAvailable -= FiddlerApplication_ResponseHeadersAvailable;
                await _fiddlerManager.StopFiddler();
                IsStarted = false;
            }
        }

        private string getSystemProxyEndPoint()
        {

            try
            {
                return Fiddler.SystemProxyManager.GetSystemProxyEndPoint();
            }
            catch
            {
                Y_Track.Helpers.YTrackLogger.Log("Cannot read ProxyServer URI");
                return null;
            }
        }

        private string _readTrackerScript()
        {
            var appDirectory = Path.GetDirectoryName(Path.Combine(Assembly.GetEntryAssembly().Location));
            var trackerScriptPath = appDirectory + _instance.YOUTUBE_INJECTED_CLIENT_FILE_PATH;
            return File.ReadAllText(trackerScriptPath);
        }




        private void FiddlerApplication_ResponseHeadersAvailable(Session oSession)
        {
            if (isYoutubeInjectablePage(oSession))
            {
                _handleCachingHeaders(ref oSession);
            }
        }


        private void _handleCachingHeaders(ref Session session)
        {
            // this allows the response session to be modefied 
            session.bBufferResponse = true;

            // forcing to response reply with text/html pure page without any compression
            session.oRequest.headers.Remove("Accept-Encoding");

            // prevent the browser from caching the main/watch page 
            // to make sure the tracker is injected with each reponse (prevent caching it)
            session.oRequest.headers.Remove("Last-Modefied");
            session.oRequest.headers.Remove("ETag");
            session.oRequest.headers["Expires"] = "0";
            session.oRequest.headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            session.oRequest.headers.Add("Pragma", "no-cache");
            session.oRequest.headers.Add("Vary", "*");
        }

        private void FiddlerApplication_BeforeResponse(Session oSession)
        {
            if (isYoutubeInjectablePage(oSession))
            {
                _injectTrackingChatterScript(ref oSession);

                // see if this is youtube main page  response
                if (isYoutubeMainPageDetected(oSession))
                {
                    // raise session detection event
                    this.OnYoutubeSessionDetected?.Invoke(this, EventArgs.Empty);
                }
            }

            // if this is a chatter message return 200 response 
            // Technically this response will be ignored from the client tracker side)
            if (isYoutubeChatterMessage(oSession))
            {
                oSession.oResponse.headers.SetStatus(200, "OK");
            }
        }



        private void FiddlerApplication_BeforeRequest(Session oSession)
        {
            if (isYoutubeInjectablePage(oSession))
            {
                oSession.bBufferResponse = true;
                _handleCachingHeaders(ref oSession);
            }


            if (isYoutubeChatterMessage(oSession))
            {
                _handleChatterMessage(oSession.oRequest.headers.RequestPath);
                oSession.bBufferResponse = true;
            }
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

        private void YoutubeHandler(Session oSession)
        {
            // handle youtube packets
            if (isYoutubeVideoPacket(oSession))
            {
                _handleYoutubePacket(oSession);
            }
        }


        private bool isYoutubeMainPageDetected(Session session)
        {
            return (session.oRequest.headers.RequestPath == "/" || session.oRequest.headers.RequestPath.IndexOf("/?") > -1)
                && session.oRequest.host == "www.youtube.com";
        }


        private bool isYoutubeInjectablePage(Session session)
        {
            // TODO :: check also if this is a html page by content-type
            // if the session is originated from the watch?v={video_id} page or the main page return true
            return (session.oRequest.headers.RequestPath == "/" || session.oRequest.headers.RequestPath.IndexOf("/watch?") > -1 || session.oRequest.headers.RequestPath.IndexOf("/?") > -1)
                && session.oRequest.host == "www.youtube.com";
        }

        private bool isYoutubeChatterMessage(Session session)
        {
            // is this coming from youtube chatter javascript client ?
            return
                session?.oRequest?.headers?.RequestPath != null
                && session.oRequest.headers.RequestPath.IndexOf(DUMMY_CLIENT_CHATTER_PATH) > -1;
        }

        private bool isYoutubeVideoPacket(Session session)
        {
            return
                // check if the response contains media data
                session.ResponseHeaders["Content-Type"].OICStartsWithAny(YoutubeVideoMIMEs)
                // check if media data comes from youtube playback
                && session.oRequest.headers.RequestPath.Contains("/videoplayback?")
                // check if playback is originated by youtube
                && session.RequestHeaders["Origin"].IndexOf("https://www.youtube.com") > -1
                // check if the request are not coming from YTrack 
                // if it's so then it's comming from PlayerWindow and we don't want to intercept it
                // if session.LocalProcessID is 0 then MITM Proxy can't say from where the request came, 
                // in this case i'll assume it's not coming from PlayerWindow
                && (session.LocalProcessID != Process.GetCurrentProcess().Id || session.LocalProcessID == 0);


        }

        private YoutubeMediaPacketType _parseMediaPacketType(string sessionContentType)
        {

            // returns the videoMediaType from a contentType header
            YoutubeMediaPacketType PacketType = YoutubeMediaPacketType.Unknown;

            if (sessionContentType.OICStartsWith("video"))
                PacketType = YoutubeMediaPacketType.Video;

            if (sessionContentType.OICStartsWith("audio"))
                PacketType = YoutubeMediaPacketType.Audio;

            return PacketType;
        }

        //debugging
        private List<string> y_videos_hosts = new List<string>();

        private void _handleYoutubePacket(Session session)
        {

            // check if the packet in session is Audio or Video
            YoutubeMediaPacketType PacketType = _parseMediaPacketType(session.ResponseHeaders["Content-Type"]);

            // if content type is not video nor audio cancel it
            if (PacketType == YoutubeMediaPacketType.Unknown) return;

            var requestURL = requestURLFromSession(session);

            // making sure that the packet is valid to parse
            if (!YoutubeMediaPacket.IsValidYoutubeRequestURL(requestURL))
                return;

            var newPacket = new YoutubeMediaPacket(PacketType, requestURL, session.responseBodyBytes);

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

        private YoutubeRequestURL requestURLFromSession(Session session)
        {
            YoutubePlaybackRequestPath youtubeRequestPath = YoutubePlaybackRequestPathParser.ParseRequestPathString(session.oRequest.headers.RequestPath);
            string host = session.oRequest.host;
            return new YoutubeRequestURL()
            {
                RequestPath = youtubeRequestPath,
                Host = host
            };
        }

        private void _injectTrackingChatterScript(ref Session oSession)
        {
            // getting the request body 
            string reponseString = oSession.GetResponseBodyAsString();
            // finding end head tag and replace it with the javascript tracker
            string injectedResponse = reponseString.Replace("</head>", "<script>" + this.TrackingScript + "</script>" + "</head>");
            // turning the injected document into byte[] 
            byte[] toInjectBytes = System.Text.Encoding.UTF8.GetBytes(injectedResponse);
            // replace the response with the injected one
            oSession.ResponseBody = toInjectBytes;
        }



    }
}
