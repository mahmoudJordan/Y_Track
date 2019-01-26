using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Y_Track.DAL.Models;
using Y_Track.Properties;
using Y_Track.UI;

namespace Y_Track.UserControls
{
    /// <summary>
    /// Interaction logic for OnlineVideoCard.xaml
    /// </summary>
    public partial class OnlineVideoCard : UserControl
    {
        public static readonly DependencyProperty VideoProperty = DependencyProperty.Register("Video",
                                             typeof(Youtube_Video), typeof(OnlineVideoCard), new FrameworkPropertyMetadata(default(Youtube_Video)));

        public Youtube_Video Video
        {
            get { return (Youtube_Video)GetValue(VideoProperty); }
            set { SetValue(VideoProperty, value); }
        }

        private bool _isBeingDownloaded = false;


        public OnlineVideoCard()
        {
            InitializeComponent();
        }

        public OnlineVideoCard(Youtube_Video video)
        {
            this.Video = video ?? throw new ArgumentNullException(nameof(video));

            InitializeComponent();
            this.Loaded += OnlineVideoCard_Loaded; ;
            this.Opacity = 0;
        }

        private void OnlineVideoCard_Loaded(object sender, RoutedEventArgs e)
        {
            _initializeVideo();
        }


        private void _initializeVideo()
        {
            ImageSource thumbSource = this.Video.ThumbnailImageSource;
            this.CardImage.Source = thumbSource != null ? thumbSource : new BitmapImage(new Uri("../Resources/Thumbnail placeholder.png", UriKind.Relative));
            this.VideoTitle.Text = this.Video.Title;
            this.VideoDescription.Text = this.Video.Description;
            this.DurationLabel.Text = this.Video.Duration.ToString();
            Helpers.AnimationHelper.AnimateOpacity(this, 0, 1);
        }


        private void WatchVideoButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerWindow pw = new PlayerWindow();
            pw.SetVideo(this.Video.Id);
            pw.Show();
        }

        private void DeleteVideoButton_Click(object sender, RoutedEventArgs e)
        {
            var homeControl = MainWindow.MainWindowAccessor.MenuItems
                .First(x => x.MenuItemType == MenuItemsUserControls.MenuItemType.OnlineVideos)
                .Content as HomeUserControl;

            homeControl.VideosArea.VideosContainer.Children.Remove(this);
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBeingDownloaded) return;
            _isBeingDownloaded = true;

            YoutubeExplode.YoutubeClient client = new YoutubeExplode.YoutubeClient();
            var stream = await client.GetVideoMediaStreamInfosAsync(this.Video.Id);
            var mediaInfo = stream.Video.OrderBy(x => x.Resolution.Height).LastOrDefault();
            if (mediaInfo is null)
            {
                MessageBox.Show("Cannot Download This Video", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var downloadUrl = mediaInfo.Url;

            _downloadVideo(downloadUrl);
        }

        private void _downloadVideo(string downloadUrl)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    this.DownloadStatus.Text = "Downloading...";
                    this.DownloadLoadingCircle.Visibility = Visibility.Visible;

                    if (!Directory.Exists(Settings.Default.OutputDirectory))
                    {
                        Settings.Default.OutputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos);
                        Settings.Default.Save();
                    }
                    string outputPath = System.IO.Path.Combine(Settings.Default.OutputDirectory, Helpers.Misc.RemoveIllegalPathChars(this.Video.Title));
                    this.Video.PhysicalPath = outputPath;

                    client.DownloadProgressChanged += (s, e) =>
                    {
                        this.Dispatcher.Invoke(delegate
                        {
                            ProgressBar.Value = e.ProgressPercentage;
                        });
                    };


                    client.DownloadFileCompleted += (s, e) =>
                    {
                        this.Dispatcher.Invoke(delegate
                        {
                            ProgressBar.Foreground = Brushes.Green;

                            //store in the database
                            int? thumbId = DAL.Database.Instance.AddThumbnail(Video.Thumbnail);
                            this.Video.ThumbnailId = thumbId;
                            DAL.Database.Instance.AddVideo(this.Video);

                            this.DownloadStatus.Text = "Downloaded Successfully...";
                            this.DownloadLoadingCircle.Visibility = Visibility.Collapsed;

                        });
                    };


                    client.DownloadFileAsync(new Uri(downloadUrl), outputPath);
                }
            }
            catch (Exception e)
            {
                NotificationProvider.Instance.ShowMessageNotification("Download Failed", "Failed To Download Video : " + this.Video.Title, NotificationMessageType.Error, this.Dispatcher);
                Helpers.YTrackLogger.Log("Failed To Download Video : " + this.Video.Title + "\n\n" + e.Message + "\n\n" + e.StackTrace);
            }
        }





    }
}
