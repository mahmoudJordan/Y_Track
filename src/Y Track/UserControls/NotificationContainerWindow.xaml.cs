using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Y_Track.Extentions;
using UserControl = System.Windows.Controls.UserControl;

namespace Y_Track.UserControls
{
    /// <summary>
    /// a Message without button or a Y/N Dialog
    /// </summary>
    public enum NotificationType
    {
        Message,
        Dialog
    }


    /// <summary>
    /// Interaction logic for NotificationContainerWindow.xaml
    /// </summary>
    public partial class NotificationContainerWindow : Window
    {


        public NotificationType NotificationType { set; get; }
        // freeze notification closing timer when the notification is hovered
        private bool FreezeClose = false;
        
        public NotificationContainerWindow(NotificationYNDialog content)
        {
            InitializeComponent();
            _initializeContent(content);
            content.OnDialogResultAvailable += Content_OnDialogResultAvailable;
        }

     
        /// <summary>
        /// Notification Inner Content
        /// </summary>
        /// <param name="content"></param>
        public NotificationContainerWindow(UserControl content)
        {
            InitializeComponent();
            _initializeContent(content);
        }

        private void Content_OnDialogResultAvailable(object sender, NotificationDialogResult NotificationDialogResult)
        {
            this._proceedClose();
        }

        /// <summary>
        /// constructor to initialize the notification with it's content
        /// </summary>
        /// <param name="content"></param>
        public NotificationContainerWindow(NotificationVideoCard content)
        {
            InitializeComponent();
            _initializeContent(content);
        }

        private void _initializeContent(System.Windows.Controls.UserControl content)
        {
            // add the new content to Content Grid Container
            this.Content.Children.Add(content);
            // to enable blur effect
            this.Loaded += NotificationContainerWindow_Loaded;

            // prevent the notification window from stealing focus when popup
            this.ShowActivated = false;

            // set the content type implicitly
            this.NotificationType = content is NotificationVideoCard
                ? this.NotificationType = NotificationType.Message
                : NotificationType = NotificationType.Dialog;

        }

        public new void Show()
        {
            // set start up location
            _setDimentionsAndLocation();
            // proceed base show
            base.Show();
        }

        private void _configureWindow()
        {
            // allow drug when mouse clicked
            this.MouseDown += Window_MouseDown;
            // show it on top of all other windows
            this.Topmost = true;
            // hide it from taskbar
            this.ShowInTaskbar = false;
            //close current window if the main window closed
            System.Windows.Application.Current.MainWindow.Closed += NotificationContainerWindow_Closed;
        }

        private void NotificationContainerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _configureWindow();
            // for keeping the window at top of every other window (system wide)
            this.ShowOnTopOfAllOtherWindows_SystemWide();
            // to make it glass like
            this.EnableBlur();

        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // makes the notification draggable (configurable)
            if (Properties.Settings.Default.NotificationDraggable) DragMove();
        }

        private void NotificationContainerWindow_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void _setDimentionsAndLocation()
        {
            // force the content dimentions to be computed
            this.Content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            this.Content.Arrange(new Rect(0, 0, Content.DesiredSize.Width, Content.DesiredSize.Height));

            // shrink window to fit child grid
            this.Height = this.Content.ActualHeight;
            this.Width = this.Content.ActualWidth + 30; // +30 is for the exit buttons

            // compute startup location
            double taskBarHeight = System.Windows.Forms.SystemInformation.CaptionHeight;
            double screenHeight = System.Windows.SystemParameters.WorkArea.Height;
            double screenWidth = System.Windows.SystemParameters.WorkArea.Width;
            double notificationWindowHeight = this.Content.ActualHeight;
            double notificationWindowWidth = this.Content.ActualWidth;
            double y_point = screenHeight - (notificationWindowHeight + taskBarHeight);
            double x_point = screenWidth - (notificationWindowWidth + 60);
            this.Top = y_point;
            this.Left = x_point;
            // set the animation "To" Parameter to x point
            WindowSlideDoubleAnimation.To = x_point;
        }



        private void _proceedClose()
        {
            var CloseStoryBoard = this.FindResource("CloseStoryBoard") as Storyboard;
            Storyboard.SetTarget(this, CloseStoryBoard);
            CloseStoryBoard.Completed += CloseStoryBoard_Completed;
            CloseStoryBoard.Begin();
        }

        /// <summary>
        /// tiggered when the notification end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Storyboard_Completed(object sender, EventArgs e)
        {
            Timer timer = new Timer();
            timer.Tick += (s, ev) =>
            {
                if (this.FreezeClose) return;
                this._proceedClose();
                timer.Stop();
            };
            timer.Start();
            // here the time out
            timer.Interval = this.NotificationType == NotificationType.Message
                ? Properties.Settings.Default.NotificationMessageTimeout
                : Properties.Settings.Default.NotificationDialogTimeout;
        }

        private void CloseStoryBoard_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _proceedClose();
        }

        private void NotificationWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Opacity = 0;
            this.FreezeClose = true;
        }

        private void NotificationWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Opacity = 0.7;
            this.FreezeClose = false;
        }


        // to prevent the notification windows from stealing focus from other windows application
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    }
}
