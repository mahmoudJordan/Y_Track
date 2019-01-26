using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Y_Track.UserControls;

namespace Y_Track.UI
{
    /// <summary>
    /// Interaction logic for VideosArea.xaml
    /// </summary>
    public partial class VideosArea : UserControl
    {
        public delegate void VideosScrollHitEnd();
        public event VideosScrollHitEnd OnVideosScrollHitEnd;
        private int _currentRow = 0;
        private int _currentColumn = 0;


        public VideosArea()
        {
            InitializeComponent();
            DataContext = this;
        }


        private void MainWindowAccessor_OnInnerVideoAreaScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer is null) return;
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                OnVideosScrollHitEnd?.Invoke();
        }


        public void FillVideos(List<VideoCard> VideoCards)
        {
            if (VideoCards == null) throw new ArgumentNullException(nameof(VideoCards));

            _hidePlaceHolderText();

            _resetVideosArea();

            VideoCards.ForEach(x =>
            {
                AddVideoToList(x);
            });
        }


        private void _resetVideosArea()
        {
            this.VideosContainer.Children.Clear();
            _currentColumn = 0;
            _currentRow = 0;
        }

 

        public void AddVideoToList(UserControl card)
        {
            this.VideosContainer.Children.Add(card);
            Grid.SetColumn(card, _currentColumn);
            Grid.SetRow(card, _currentRow);
            _currentColumn++;

            if (_currentColumn == this.VideosContainer.ColumnDefinitions.Count)
            {
                RowDefinition def = new RowDefinition();
                this.VideosContainer.RowDefinitions.Add(def);
                _currentRow++;
                _currentColumn = 0;
            }
        }



        public void AddVideo(UserControl videoCard)
        {
            if (videoCard == null) throw new ArgumentNullException(nameof(videoCard));

            _hidePlaceHolderText();
            AddVideoToList(videoCard);
        }



        private void _hidePlaceHolderText()
        {
            MainWindow.MainWindowAccessor.OnInnerVideoAreaScrollChanged += MainWindowAccessor_OnInnerVideoAreaScrollChanged;
            this.MiddleMessagePanel.Visibility = Visibility.Collapsed;
            VideosContainer.Visibility = Visibility.Visible;
        }
    }
}
