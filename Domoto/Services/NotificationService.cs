using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Domoto.Models;

namespace Domoto.Services
{
    public class NotificationService
    {
        private static NotificationService _instance;
        public static NotificationService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NotificationService();
                return _instance;
            }
        }

        private readonly TaskbarIcon _tray;
        private readonly DispatcherTimer _timer;
        private readonly HashSet<int> _notified = new HashSet<int>();

        // Configurable check interval (15 minutes)
        private const int CHECK_INTERVAL_MINUTES = 15;

        private NotificationService()
        {
            _tray = new TaskbarIcon
            {
                // IconSource = new BitmapImage(new Uri("pack://application:,,,/Assets/tray.ico")),
                ToolTipText = "Domoto - Task Manager"
            };

            // Handle balloon click to restore window
            _tray.TrayBalloonTipClicked += OnBalloonClicked;

            // TODO: Add context menu with "Open Domoto" and "Exit" options
            // _tray.ContextMenu = CreateContextMenu();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(CHECK_INTERVAL_MINUTES) };
            _timer.Tick += (s, e) => Check();
            _timer.Start();

            // Run initial check
            Check();
        }

        public void Check()
        {
            if (!SessionService.IsLoggedIn) return;

            var tasks = DatabaseService.Instance.GetTasks(SessionService.CurrentUser.Id);
            foreach (var task in tasks.Where(t => t.IsDueSoon && !_notified.Contains(t.Id)))
            {
                _tray.ShowBalloonTip(
                    "Task due soon",
                    string.Format("{0} — due {1:g}", task.Title, task.DueDate),
                    BalloonIcon.Info
                );
                _notified.Add(task.Id);
            }
        }

        private void OnBalloonClicked(object sender, RoutedEventArgs e)
        {
            // Restore and focus main window
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Activate();
                mainWindow.Focus();

                // TODO: Navigate to Task_View
                // This requires access to the navigation logic in Window1
            }
        }

        public void Reset()
        {
            _notified.Clear();
        }

        public void Dispose()
        {
            if (_timer != null)
                _timer.Stop();
            if (_tray != null)
                _tray.Dispose();
        }
    }
}
