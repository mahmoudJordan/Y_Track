using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Y_Track.DAL;
using Y_Track.DAL.Models;
using Y_Track.Helpers;
using Y_Track.Properties;
using Y_Track.UserControls;
using Y_Track.YoutubeCaptureEngine;
using Y_Track.YoutubeCaptureEngine.Models;

namespace Y_Track.UI
{
    /// <summary>
    /// Interaction logic for LocalVideosUserControl.xaml
    /// </summary>
    public partial class LocalVideosUserControl : UserControl
    {

        public void ClearVideos() => VideosArea.VideosContainer.Children.Clear();
        private int _currentOffset = 0;
        private List<string> _videosIds;
        private List<string> _alreadyDownloadDialogedVideos;
        public LocalVideosUserControl()
        {
            InitializeComponent();
            YoutubeInterceptEngine.Instance.OnNewYoutubeStored += _mainEngine_OnNewYoutubeStored;
            YoutubeInterceptEngine.Instance.OnNewVideoLastPacketRecieved += Instance_OnNewVideoLastPacketRecieved;
            YoutubeInterceptEngine.Instance.OnYoutubeChatterClientMessageRecieved += _mainEngine_OnYoutubeChatterClientMessageRecieved;
            YoutubeInterceptEngine.Instance.OnYoutubeSessionDetected += Instance_OnYoutubeSessionDetected;
            this.VideosArea.OnVideosScrollHitEnd += VideosArea_OnVideosScrollHitEnd;
            readStartupVideos();
            readVideosIds();
            _alreadyDownloadDialogedVideos = new List<string>();
        }


        private async void readVideosIds()
        {
            _videosIds = await Database.Instance.GetAllVideosIds();
        }


        private void Instance_OnYoutubeSessionDetected(object sender, EventArgs e)
        {
            NotificationProvider.Instance.ShowMessageNotification("I'm Listening", "Youtube Session Detected, I'll try to Capture any video you watch...", NotificationMessageType.Reminder, this.Dispatcher);
        }

        private async void Instance_OnNewVideoLastPacketRecieved(YoutubeVideoManager manager)
        {
            // populates the media information if it's not populated and the tracker send the video id in the url
            await manager.PopulateMediaInfo();
            Application.Current.Dispatcher.Invoke((Action)async delegate
            {
                // show YesNow Dialog Asking user if he want download the rest of the video or not
                await _showDownloadAskingDialog(manager);
            });
            //_startDownloadingVideo(manager);
        }

        private void readStartupVideos()
        {
            new Thread(() =>
            {
                // prevent the UI Lagging on startup while reading local database
                Thread.Sleep(3000);

                // make sure the main window is not closed before loading videos
                if (MainWindow.MainWindowAccessor.IsClosed) return;

                this.Dispatcher.Invoke(async () =>
                {
                    await _loadVideos(_currentOffset, Properties.Settings.Default.NumberOfVideosShowAtStartup);
                });
            }).Start();

        }

        private async void VideosArea_OnVideosScrollHitEnd()
        {
            await _loadVideos(_currentOffset, Properties.Settings.Default.NumberOfVideosShowAtStartup);
        }

        private async void _mainEngine_OnYoutubeChatterClientMessageRecieved(YoutubeVideoManager manager, YoutubeChatterClientMessageType messageType)
        {
            // populates the media information if it's not populated and the tracker send the video id in the url
            await manager.PopulateMediaInfo();
            Application.Current.Dispatcher.Invoke((Action)async delegate
            {
                // show YesNow Dialog Asking user if he want download the rest of the video or not
                await _showDownloadAskingDialog(manager);

            });
        }



        private async Task _showDownloadAskingDialog(YoutubeVideoManager manager)
        {
            // making sure the video info is populated
            if (manager.Video.VideoInfo is null)
            {
                YTrackLogger.Log($"Starting to download the rest of {manager.Video.VideoFingerPrint} before the info was populated ... ignoring download");
                return;
            }

            var thumbnail = await Common.FetchThumbnail(manager.Video.VideoInfo.ThumbnailUrl);
            var thumbBytes = Helpers.Misc.ToImage(thumbnail.thumbnailBytes);

            // check if video is currently saved in database
            // if so .. I won't display store notification dialog
            if (_videosIds.Contains(manager.Video.VideoInfo.Id)) return;

            // check if the current video download dialog is already shown before
            if (_alreadyDownloadDialogedVideos.Contains(manager.Video.VideoInfo.Id)) return;

            var dialog = new NotificationYNDialog(manager.Video.VideoInfo.Title, thumbBytes);
            NotificationContainerWindow notificationWindow = new NotificationContainerWindow(dialog);
            notificationWindow.Show();

            dialog.OnDialogResultAvailable += (sender, dialogResult) =>
            {
                // add to already dialog list ... to make sure the dialog doesn't show for the same video twice
                _alreadyDownloadDialogedVideos.Add(manager.Video.VideoInfo.Id);

                if (dialogResult == NotificationDialogResult.Yes || ((dialogResult == NotificationDialogResult.NotChoosen) && Settings.Default.KeepIncompleteCaptureByDefault))
                {
                    _startDownloadingVideo(manager);
                }
            };
        }



        private void _startDownloadingVideo(YoutubeVideoManager manager)
        {
            if (!Directory.Exists(Settings.Default.OutputDirectory))
            {
                Settings.Default.OutputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos);
                Settings.Default.Save();
            }

            string outputPath = Path.Combine(Settings.Default.OutputDirectory, Helpers.Misc.RemoveIllegalPathChars(manager.Video.VideoInfo.Title));

            new Thread(async () =>
            {
                try
                {
                    await manager.DownloadAndMultiplix(outputPath);
                }
                catch (Exception e)
                {
                    YTrackLogger.Log("Exception Occured while Downloading and muxing : " + e.Message + "\n\n" + e.StackTrace);
                    NotificationProvider.Instance.ShowMessageNotification("Capture Failed", "Error While Downloading The Rest Of The Video", NotificationMessageType.Error, this.Dispatcher);
                }
            }).Start();
        }




        private async Task _loadVideos(int videosOffset, int videosCount)
        {
            _currentOffset += videosCount;
            MainWindow.MainWindowAccessor.ToggleLoading(true, "Reading Local Database...");
            await Task.Run(async () =>
            {
                var videos = await Database.Instance.ReadLocalYoutubeVideosInfo(videosOffset, videosCount);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (videos != null)
                    {
                        videos.ForEach(x =>
                        {
                            this.VideosArea.AddVideo(new UserControls.VideoCard(x));
                        });
                    }
                    // end of videos
                    MainWindow.MainWindowAccessor.ToggleLoading(false, "");
                });
            });
        }

        private async void _mainEngine_OnNewYoutubeStored(YoutubeVideoManager manager, string path)
        {
            var video = await _storeVideo(manager.Video.VideoInfo, path);
            _showNewVideoCapturedNotification(video);
        }

        private void _showNewVideoCapturedNotification(Youtube_Video video)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                var notificationVideoCard = new NotificationVideoCard(video);
                NotificationContainerWindow notificationWindow = new NotificationContainerWindow(notificationVideoCard);
                notificationWindow.Show();
            });
        }


        private async Task<Youtube_Video> _storeVideo(YoutubeVideoInfo info, string path)
        {
            if (info is null) throw new ArgumentNullException(nameof(info));

            return await Task.Run(async () =>
            {
                var db_video = await _saveVideoToDatabase(info, path);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    this.VideosArea.AddVideo(new UserControls.VideoCard(db_video));
                });
                return db_video;
            });
        }


        private async Task<Youtube_Video> _saveVideoToDatabase(YoutubeVideoInfo info, string physicalPath)
        {
            DAL.Models.Thumbnail thumbnail = await Common.FetchThumbnail(info.ThumbnailUrl);

            var youtubeVideo = new DAL.Models.Youtube_Video();
            youtubeVideo.Id = info.Id;
            youtubeVideo.Title = info.Title;
            youtubeVideo.Author = info.Author;
            youtubeVideo.Duration = info.Duration;
            youtubeVideo.Description = info.Description;
            youtubeVideo.StoreDate = DateTime.Now;
            youtubeVideo.ThumbnailId = Database.Instance.AddThumbnail(thumbnail);
            youtubeVideo.Thumbnail = thumbnail;
            youtubeVideo.PhysicalPath = physicalPath;
            Database.Instance.AddVideo(youtubeVideo);
            return youtubeVideo;
        }


    }
}
