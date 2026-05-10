using System;
using System.Collections.Generic;

namespace Domoto.Services
{
    /// <summary>
    /// No-op stub. The original implementation used Hardcodet.NotifyIcon.Wpf
    /// for tray balloon notifications, but the package isn't vendored in this
    /// repo. To keep the build green without fetching external packages we
    /// stub the public API here. Restore the real implementation (plus the
    /// NuGet reference in the csproj) when tray notifications are wanted.
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

        private NotificationService()
        {
        }

        public void Check()
        {
            // Intentionally no-op. Tasks are still visible via the UI; we
            // just don't raise tray balloons.
        }

        public void Reset()
        {
            _notified.Clear();
        }

        public void Dispose()
        {
            // Nothing to release.
        }
    }
}
