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
using Y_Track.UserControls;

namespace Y_Track.UI
{
    /// <summary>
    /// Interaction logic for HomeUserControl.xaml
    /// </summary>
    public partial class HomeUserControl : UserControl
    {
        public HomeUserControl()
        {
            InitializeComponent();
        }

        public void AddVideo(OnlineVideoCard videocard)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (videocard != null)
                {
                    this.VideosArea.AddVideo(videocard);
                }
            });
        }


        public void ClearVideos() => VideosArea.VideosContainer.Children.Clear();



    }
}
