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

namespace Y_Track.UserControls
{
    /// <summary>
    /// Interaction logic for NotificationMessage.xaml
    /// </summary>
    public partial class NotificationMessage : UserControl
    {
        public NotificationMessage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// set inner context of the notification
        /// </summary>
        /// <param name="messageTitle"></param>
        /// <param name="messageText"></param>
        /// <param name="_type"></param>
        public void SetMessageText(string messageTitle , string messageText , NotificationMessageType _type)
        {
            this.MessageText.Text = messageText;
            this.MessageTitle.Text = messageTitle;
            this.MessageTitle.Foreground = _type == NotificationMessageType.Error ? Brushes.Red : Brushes.White;
        }

    
    }


    public enum NotificationMessageType
    {
        Error,
        Reminder
    }
}
