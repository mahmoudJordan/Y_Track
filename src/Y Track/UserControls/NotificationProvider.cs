using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Y_Track.UserControls
{
    public class NotificationProvider
    {
        private static NotificationProvider _instance;
        public static NotificationProvider Instance
        {
            get
            {
                if (_instance is null) _instance = new NotificationProvider();
                return _instance;
            }
        }

        public void ShowMessageNotification(string messageTitle , string Message , NotificationMessageType _type , Dispatcher dispatcher = null)
        {
            // if the dispatcher passed null assume the calling thread is owned by mainWindow Dispatcher
            Dispatcher __ = dispatcher != null
                ? dispatcher
                : MainWindow.MainWindowAccessor.Dispatcher;

            __.Invoke((Action)delegate
            {
                var messageNoti = new NotificationMessage();
                messageNoti.SetMessageText(messageTitle, Message, _type);
                NotificationContainerWindow notificationWindow = new NotificationContainerWindow(messageNoti);
                notificationWindow.NotificationType = NotificationType.Message;
                notificationWindow.Show();
            });
        }
    }
}
