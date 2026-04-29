using System;
using System.Windows.Input;
using Domoto.Helpers;
using Domoto.Services;

namespace Domoto.ViewModels
{
    public class SidebarViewModel : BaseViewModel
    {
        private string _searchText;
        private string _activeNavItem;

        public string CurrentUsername
        {
            get
            {
                return SessionService.CurrentUser != null
                    ? SessionService.CurrentUser.Username
                    : string.Empty;
            }
        }

        public string CurrentRole
        {
            get
            {
                return SessionService.CurrentUser != null
                    ? SessionService.CurrentUser.Role
                    : string.Empty;
            }
        }

        public bool IsAdmin
        {
            get { return SessionService.IsAdmin; }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
                var handler = SearchTextChanged;
                if (handler != null)
                    handler(value);
            }
        }

        public string ActiveNavItem
        {
            get { return _activeNavItem; }
            set { _activeNavItem = value; OnPropertyChanged("ActiveNavItem"); }
        }

        public event Action<string> NavigationRequested;
        public event Action LogoutRequested;
        public event Action<string> SearchTextChanged;

        public ICommand NavigateHomeCommand { get; private set; }
        public ICommand NavigateTasksCommand { get; private set; }
        public ICommand NavigateProfileCommand { get; private set; }
        public ICommand NavigateAdminCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public SidebarViewModel()
        {
            _activeNavItem = "Home";

            NavigateHomeCommand = new RelayCommand(o =>
            {
                ActiveNavItem = "Home";
                var handler = NavigationRequested;
                if (handler != null)
                    handler("Home");
            });

            NavigateTasksCommand = new RelayCommand(o =>
            {
                ActiveNavItem = "Tasks";
                var handler = NavigationRequested;
                if (handler != null)
                    handler("Tasks");
            });

            NavigateProfileCommand = new RelayCommand(o =>
            {
                ActiveNavItem = "Profile";
                var handler = NavigationRequested;
                if (handler != null)
                    handler("Profile");
            });

            NavigateAdminCommand = new RelayCommand(o =>
            {
                ActiveNavItem = "Admin";
                var handler = NavigationRequested;
                if (handler != null)
                    handler("Admin");
            });

            LogoutCommand = new RelayCommand(o =>
            {
                SessionService.Logout();
                var handler = LogoutRequested;
                if (handler != null)
                    handler();
            });
        }

        public void RefreshUserInfo()
        {
            OnPropertyChanged("CurrentUsername");
            OnPropertyChanged("CurrentRole");
            OnPropertyChanged("IsAdmin");
        }
    }
}
