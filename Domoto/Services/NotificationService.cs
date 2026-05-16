using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Domoto.Models;

namespace Domoto.Services
{
    /// <summary>
    /// Checks for due-soon and overdue tasks every minute and raises
    /// ToastRequested so the UI can show an in-app notification banner.
    /// Each task only fires once per session (tracked by Id in _notified).
    /// </summary>
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

        private readonly HashSet<int> _notified = new HashSet<int>();
        private readonly DispatcherTimer _timer;

        // UI subscribes to this to show the toast banner
        public event Action<string, bool> ToastRequested; // (message, isWarning)

        private NotificationService()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += OnTick;
        }

        public void Start()
        {
            if (!_timer.IsEnabled)
                _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Reset()
        {
            _notified.Clear();
        }

        // Call this immediately after login to check right away
        public void Check()
        {
            if (!SessionService.IsLoggedIn) return;

            List<TaskItem> tasks;
            try
            {
                tasks = SessionService.IsAdmin
                    ? DatabaseService.Instance.GetAllTasks()
                    : DatabaseService.Instance.GetTasks(SessionService.CurrentUser.Id);
            }
            catch { return; }

            // Overdue tasks not yet notified
            var overdue = tasks.Where(t => t.IsOverdue && !_notified.Contains(t.Id)).ToList();
            if (overdue.Count > 0)
            {
                foreach (var t in overdue) _notified.Add(t.Id);
                string msg = overdue.Count == 1
                    ? string.Format("⚠ \"{0}\" is overdue!", overdue[0].Title)
                    : string.Format("⚠ {0} tasks are overdue!", overdue.Count);
                Raise(msg, true);
                return; // show one toast at a time
            }

            // Due within 24 hours, not yet notified
            var dueSoon = tasks.Where(t => t.IsDueSoon && !_notified.Contains(t.Id)).ToList();
            if (dueSoon.Count > 0)
            {
                foreach (var t in dueSoon) _notified.Add(t.Id);
                string msg = dueSoon.Count == 1
                    ? string.Format("🔔 \"{0}\" is due in {1}h.",
                        dueSoon[0].Title,
                        (int)(dueSoon[0].DueDate - DateTime.Now).TotalHours + 1)
                    : string.Format("🔔 {0} tasks are due within 24 hours.", dueSoon.Count);
                Raise(msg, false);
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            Check();
        }

        private void Raise(string message, bool isWarning)
        {
            var handler = ToastRequested;
            if (handler != null)
                handler(message, isWarning);
        }

        public void Dispose()
        {
            _timer.Stop();
        }
    }
}
