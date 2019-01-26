using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Y_Track.DAL;
using Y_Track.DAL.Models;

namespace Y_Track.UserControls
{
    /// <summary>
    /// Interaction logic for VideoCard.xaml
    /// </summary>
    public partial class VideoCard : UserControl
    {

        public static readonly DependencyProperty VideoProperty = DependencyProperty.Register("Video",
                                                  typeof(Youtube_Video), typeof(VideoCard), new FrameworkPropertyMetadata(default(Youtube_Video)));

        public Youtube_Video Video
        {
            get { return (Youtube_Video)GetValue(VideoProperty); }
            set { SetValue(VideoProperty, value); }
        }

        public VideoCard()
        {
            InitializeComponent();
            if (this.Video == null) throw new ArgumentNullException(nameof(this.Video));
            this.Loaded += VideoCard_Loaded;
        }

        public VideoCard(Youtube_Video video)
        {
            this.Video = video ?? throw new ArgumentNullException(nameof(video));

            InitializeComponent();
            this.Loaded += VideoCard_Loaded;
            this.Opacity = 0;
        }



        private void VideoCard_Loaded(object sender, RoutedEventArgs e)
        {
            _initializeVideo();
        }


        private void _initializeVideo()
        {
            ImageSource thumbSource = this.Video.ThumbnailImageSource;
            this.CardImage.Source = thumbSource != null ? thumbSource : new BitmapImage(new Uri("../Resources/Thumbnail placeholder.png", UriKind.Relative));
            this.VideoTitle.Text = this.Video.Title;
            this.VideoDescription.Text = this.Video.Description;
            Helpers.AnimationHelper.AnimateOpacity(this, 0, 1);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.File.Exists(this.Video.PhysicalPath))
            {
                _handleUnexistence();
                return;
            }
            string argument = "/select, \"" + this.Video.PhysicalPath + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        /// <summary>
        /// removes the file from UI/database if it not exist
        /// </summary>
        /// <returns>void</returns>
        private void _handleUnexistence()
        {
            // file was removed , delete it's record from the database
            MessageBox.Show("The Video Is No Longer Exists on the disk", "File Deleted", MessageBoxButton.OK, MessageBoxImage.Warning);
            // delete it from the database
            Database.Instance.DeleteVideo(this.Video);
            // collapse the current card from the area
            this.Visibility = Visibility.Collapsed;
            // don't open the file explorer

        }

        private void DeleteVideoButton_Click(object sender, RoutedEventArgs e)
        {
            Database.Instance.DeleteVideo(this.Video);
            this.Visibility = Visibility.Collapsed;
        }

        private void WatchVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.File.Exists(this.Video.PhysicalPath))
            {
                _handleUnexistence();
                return;
            }
            System.Diagnostics.Process.Start(this.Video.PhysicalPath);
        }
    }
}
