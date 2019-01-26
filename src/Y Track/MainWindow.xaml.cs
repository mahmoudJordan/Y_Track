using FFMpeg;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Y_Track.DAL;
using Y_Track.DAL.Models;
using Y_Track.Extentions;
using Y_Track.Titanium;
using Y_Track.UI;
using Y_Track.UserControls;
using Y_Track.UserControls.MenuItemsUserControls;
using Y_Track.YoutubeCaptureEngine;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace Y_Track
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MenuItemsUserControls.MenuItem[] MenuItems { get; private set; }
        public MenuItemType ActiveMenuItem { get; private set; }

        public static MainWindow MainWindowAccessor;
        public bool IsClosed { get; private set; }

        public delegate void InnerVideoAreaScrollChanged(object sender, ScrollChangedEventArgs e);
        public event InnerVideoAreaScrollChanged OnInnerVideoAreaScrollChanged;

        public MainWindow()
        {
            InitializeComponent();
            MainWindowAccessor = this;
            this.Loaded += MainWindow_Loaded;
            DataContext = this;
            _fireUpEngine();
            _configureMenuItems();
            _configureTaskBarAndSystemTray();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleLoading(true, "Reading Local Database...");

            this.EnableBlur();
        }

        private void WrapperScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            OnInnerVideoAreaScrollChanged?.Invoke(sender, e);
        }


        public void ToggleLoading(bool show)
        {
            this.LoadingBar.Visibility = show ? Visibility.Visible : Visibility.Hidden;
        }

        public void ToggleLoading(bool show, string message)
        {
            ToggleLoading(show);
            this.BottomBarMessage.Text = show ? message : "";

        }

        private void _configureTaskBarAndSystemTray()
        {
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            // require a System.Drawing reference ... 
            // TODO :: replace it without the reference
            ni.Icon = _loadTrayIcon();
            ni.Visible = true;
            ni.DoubleClick += delegate (object sender, EventArgs args)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };
        }

        private System.Drawing.Icon _loadTrayIcon()
        {
            var appDirectory = Path.GetDirectoryName(Path.Combine(Assembly.GetEntryAssembly().Location));
            var trayIconPath = (appDirectory + "\\Resources\\youtube icon.ico");
            return new System.Drawing.Icon(trayIconPath);
        }
        public static bool SameProcessRunning()
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            return (Process.GetProcessesByName(thisProc.ProcessName).Length > 1);
        }


        private void _fireUpEngine()
        {
            if (!SameProcessRunning())
            {
                YoutubeInterceptEngine.Instance.StartIntercepting(Properties.Settings.Default.DefaultProxyPort);
                // reading from registry directly may return proxyServer as null
                // waiting three seconds would keep us at safe zone
                setPrxoyEndPointAfter3Seconds();
            }
        }

        private void setPrxoyEndPointAfter3Seconds()
        {
            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += delegate
            {
                // make sure the main window is not closed before loading videos
                if (MainWindow.MainWindowAccessor.IsClosed) return;

                Dispatcher.Invoke(delegate
                {
                    string[] proxyLocalEndPoints = YoutubeInterceptEngine.Instance.SystemProxyAddress.Split(new[] { ";" }, StringSplitOptions.None);
                    if (proxyLocalEndPoints != null && proxyLocalEndPoints.Length > 1)
                    {
                        this.HttpProxyEndpoint.Text = proxyLocalEndPoints[0];
                        this.HttpsProxyEndpoint.Text = proxyLocalEndPoints[1];
                    }

                    t.Stop();
                });
            };
            t.Interval = 3000;
            t.Start();
        }





        private void _configureMenuItems()
        {
            MenuItems = new[]
            {
                new MenuItemsUserControls.MenuItem("Local", new LocalVideosUserControl() , MenuItemType.LocalVideos),
                new MenuItemsUserControls.MenuItem("Home", new HomeUserControl(), MenuItemType.OnlineVideos),
                new MenuItemsUserControls.MenuItem("About",new SettingsUserControl(), MenuItemType.About),
                new MenuItemsUserControls.MenuItem("Settings", new SettingsUserControl(), MenuItemType.Setting),
            };
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            ActiveMenuItem = (MenuItemType)ItemsListBox.SelectedIndex;
            MenuToggleButton.IsChecked = false;
        }




        private void _showNewVideoCapturedNotification(Youtube_Video video)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                var notificationVideoCard = new NotificationVideoCard(video);
                NotificationContainerWindow notificationWindow = new NotificationContainerWindow(notificationVideoCard);
                notificationWindow.Show();
            });
        }

        private void Card_OnDialogResultAvailable(object sender, bool NotificationDialogResult)
        {
            System.Diagnostics.Debug.WriteLine(NotificationDialogResult);
        }

        private async void SearchTermTextBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (ActiveMenuItem == MenuItemType.OnlineVideos)
                {
                    await _doSearch();
                }
                else if (ActiveMenuItem == MenuItemType.LocalVideos)
                {
                    await _searchDatabase();
                }
            }
        }

        private async Task<bool> _searchDatabase()
        {
            MainWindow.MainWindowAccessor.ToggleLoading(true, "Reading From Database ...");



            LocalVideosUserControl local = this.MenuItems.First(x => x.MenuItemType == MenuItemType.LocalVideos).Content as LocalVideosUserControl;
            local.ClearVideos();
            List<Youtube_Video> videos = null;
            if (string.IsNullOrEmpty(SearchTermTextBox.Text))
            {
                videos = await Database.Instance.ReadLocalYoutubeVideosInfo(0, Properties.Settings.Default.NumberOfVideosShowAtStartup);
            }
            else
            {
                videos = await Database.Instance.GetVideosLike(this.SearchTermTextBox.Text); ;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (videos != null)
                {
                    videos.ForEach(x =>
                    {
                        local.VideosArea.AddVideo(new UserControls.VideoCard(x));
                    });
                }
                // end of videos
                MainWindow.MainWindowAccessor.ToggleLoading(false, "");
            });

            return true;
        }


        private async Task<bool> _doSearch()
        {
            string searchQuery = this.SearchTermTextBox.Text.Trim();
            if (searchQuery == "") return false;

            _toggleLoading(true);

            var client = new YoutubeClient();
            IReadOnlyList<Video> videos = null;

            try
            {
                videos = await client.SearchVideosAsync(searchQuery, 1);
            }
            catch (Exception e)
            {
                // connot fetch results 
                //LOG_ERROR
                Helpers.YTrackLogger.Log("Cannot Fetch any Result .. Please Check Internet Connection : " + e.Message + "\n\n" + e.StackTrace);
                this.BottomBarMessage.Text = "Cannot Fetch any Result .. Please Check Internet Connection";
                return false;
            }
            finally
            {
                _toggleLoading(false);
            }
            await _fillVideosToHomeArea(videos);
            return true;
        }

        private async Task _fillVideosToHomeArea(IReadOnlyList<Video> videos)
        {
            HomeUserControl home = this.MenuItems.First(x => x.MenuItemType == MenuItemType.OnlineVideos).Content as HomeUserControl;
            home.ClearVideos();
            foreach (var video in videos)
            {
                OnlineVideoCard card = new OnlineVideoCard(await Common.ToYoutubeVideo(video));
                home.AddVideo(card);
            }

        }



        private async Task<Thumbnail> _fetchThumbnail(string url)
        {
            return await Common.FetchThumbnail(url);
        }

        private void _toggleLoading(bool show)
        {
            if (show)
                DialogHost.OpenDialogCommand.Execute(DialogHostContents, null);
            else
                DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            YoutubeInterceptEngine.Instance.StopIntercepting();
            SystemProxyManager.StopSystemProxy();
        }



        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ToggleMaximizeButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.MaximizeButton.Visibility = Visibility.Visible;
                this.ResetWindowButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                this.MaximizeButton.Visibility = Visibility.Collapsed;
                this.ResetWindowButton.Visibility = Visibility.Visible;
            }
        }






        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        private void TrackerStartStopToggle_Click(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToggleButton;
            if (tb.IsChecked.Value)
            {
                if (!YoutubeInterceptEngine.Instance.IsStarted)
                {
                    YoutubeInterceptEngine.Instance.StartIntercepting(Properties.Settings.Default.DefaultProxyPort);
                    setPrxoyEndPointAfter3Seconds();
                }
            }
            else
            {
                if (YoutubeInterceptEngine.Instance.IsStarted)
                {
                    YoutubeInterceptEngine.Instance.StopIntercepting();
                    this.HttpProxyEndpoint.Text = "";
                    this.HttpsProxyEndpoint.Text = "";
                }
            }

        }


    }// class
}
