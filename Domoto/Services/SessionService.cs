using Domoto.Models;

namespace Domoto.Services
{
    public static class SessionService
    {
        public static User CurrentUser { get; set; }

        public static bool IsAdmin
        {
            get { return CurrentUser != null && CurrentUser.Role == "Admin"; }
        }

        public static bool IsLoggedIn
        {
            get { return CurrentUser != null; }
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
