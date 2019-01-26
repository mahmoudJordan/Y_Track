using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Y_Track.UserControls
{
    /// <summary>
    /// Interaction logic for NoficationVideoCard.xaml
    /// </summary>
    public partial class NotificationVideoCard : UserControl
    {
     

        public static readonly DependencyProperty VideoProperty = DependencyProperty.Register("Video",
                                                 typeof(Youtube_Video), typeof(NotificationVideoCard), new FrameworkPropertyMetadata(default(Youtube_Video)));


        public Youtube_Video Video
        {
            get { return (Youtube_Video)GetValue(VideoProperty); }
            set { SetValue(VideoProperty, value); }
        }

        public NotificationVideoCard(Youtube_Video video)
        {
            InitializeComponent();
            if (video == null) throw new ArgumentNullException(nameof(this.Video));
            this.Video = video;
            this.Loaded += VideoCard_Loaded;
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
            this.VideoDuration.Text = this.Video.Duration.ToString() ;
            Helpers.AnimationHelper.AnimateOpacity(this, 0, 1);
        }
    }
}
