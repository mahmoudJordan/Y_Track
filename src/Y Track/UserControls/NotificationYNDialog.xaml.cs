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
    public enum NotificationDialogResult
    {
        Yes,
        No , 
        NotChoosen
    }

    /// <summary>
    /// Interaction logic for NotificationYNDialog.xaml
    /// </summary>
    public partial class NotificationYNDialog : UserControl
    {

        // default value if the dialog hide is true
        public NotificationDialogResult NotificationDialogResult { get; set; } = NotificationDialogResult.NotChoosen;

        public delegate void DialogResultAvailable(object sender, NotificationDialogResult NotificationDialogResult);
        public event DialogResultAvailable OnDialogResultAvailable;


        public string Title { get; set; }
        public ImageSource Thumbnail { get; set; }


        public NotificationYNDialog(string title , ImageSource thumb)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (thumb == null) throw new ArgumentNullException(nameof(thumb));
            InitializeComponent();
            Title = "\"" + title + "\" Is Captured But it's Not Completed, Do You Want To Download the Rest of it ?";
            Thumbnail = thumb;
            this.Loaded += VideoCard_Loaded;
            this.Unloaded += NotificationYNDialog_Unloaded;
        }

        private void VideoCard_Loaded(object sender, RoutedEventArgs e)
        {
            _initializeVideo();
        }

        private void _initializeVideo()
        {
            this.VideoTitle.Text = Title;
            this.CardImage.Source = Thumbnail;
            Helpers.AnimationHelper.AnimateOpacity(this, 0, 1);
        }

        private void NotificationYNDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            this.NotificationDialogResult = NotificationDialogResult.NotChoosen;
            _raiseOnDialogResultAvailable();
        }

        private void _raiseOnDialogResultAvailable()
        {
            OnDialogResultAvailable?.Invoke(this, this.NotificationDialogResult);
        }

        private void YesDialogButton_Click(object sender, RoutedEventArgs e)
        {
            this.NotificationDialogResult = NotificationDialogResult.Yes;
            // unsubscribe to unloaded event to make sure onDialogResultAvailable not fire twice
            this.Unloaded -= NotificationYNDialog_Unloaded;
            _raiseOnDialogResultAvailable();
        }

        private void NoDialogButton_Click(object sender, RoutedEventArgs e)
        {
            this.NotificationDialogResult = NotificationDialogResult.No;
            // unsubscribe to unloaded event to make sure onDialogResultAvailable not fire twice
            this.Unloaded -= NotificationYNDialog_Unloaded;
            _raiseOnDialogResultAvailable();
        }
    }
}
