using System.Windows;
using System.Windows.Input;
using Domoto.ViewModels;
using Domoto.Views;

namespace Domoto
{
    public partial class Window1 : Window
    {
        private LoginViewModel _loginVm;
        private TaskViewModel _taskVm;
        private AdminViewModel _adminVm;
        private SidebarViewModel _sidebarVm;
        private DashboardViewModel _dashboardVm;

        public Window1()
        {
            InitializeComponent();
            ShowLogin();
        }

        // ── Custom title bar handlers ──────────────────────────────

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            else
            {
                DragMove();
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                BtnMaximize.Content = "☐";
                BtnMaximize.ToolTip = "Maximize";
            }
            else
            {
                WindowState = WindowState.Maximized;
                BtnMaximize.Content = "❐";
                BtnMaximize.ToolTip = "Restore Down";
            }
        }

        private void ShowLogin()
        {
            // Hide sidebar and reset its column width so login is full-screen
            SidebarControl.Visibility = Visibility.Collapsed;
            SidebarColumn.Width = new GridLength(0);

            _loginVm = new LoginViewModel();
            _loginVm.LoginCompleted += OnLoginCompleted;

            var loginView = new LoginView();
            loginView.DataContext = _loginVm;

            MainContent.Content = loginView;
        }

        private void OnLoginCompleted(bool success)
        {
            if (success)
            {
                // Show sidebar
                SidebarControl.Visibility = Visibility.Visible;
                SidebarColumn.Width = new GridLength(260);

                // Initialize sidebar view model
                _sidebarVm = new SidebarViewModel();
                _sidebarVm.RefreshUserInfo();
                _sidebarVm.NavigationRequested += OnSidebarNavigationRequested;
                _sidebarVm.LogoutRequested += OnLogoutRequested;
                _sidebarVm.SearchTextChanged += OnSidebarSearchTextChanged;
                SidebarControl.DataContext = _sidebarVm;

                // Initialize task view model (shared across navigation)
                _taskVm = new TaskViewModel();

                // Navigate to Dashboard
                NavigateToDashboard();
            }
        }

        private void OnSidebarNavigationRequested(string target)
        {
            switch (target)
            {
                case "Home":
                    NavigateToDashboard();
                    break;
                case "Tasks":
                    NavigateToTasks();
                    break;
                case "Profile":
                    NavigateToProfile();
                    break;
                case "Admin":
                    NavigateToAdmin();
                    break;
            }
        }

        private void OnLogoutRequested()
        {
            // Unhook sidebar events
            if (_sidebarVm != null)
            {
                _sidebarVm.NavigationRequested -= OnSidebarNavigationRequested;
                _sidebarVm.LogoutRequested -= OnLogoutRequested;
                _sidebarVm.SearchTextChanged -= OnSidebarSearchTextChanged;
            }

            _taskVm = null;
            _dashboardVm = null;
            _adminVm = null;
            _sidebarVm = null;

            ShowLogin();
        }

        private void NavigateToDashboard()
        {
            _dashboardVm = new DashboardViewModel();
            _dashboardVm.NavigateToTasksRequested += OnDashboardNavigateToTasks;

            var dashboardView = new DashboardView();
            dashboardView.DataContext = _dashboardVm;

            MainContent.Content = dashboardView;
        }

        private void OnDashboardNavigateToTasks()
        {
            if (_sidebarVm != null)
                _sidebarVm.ActiveNavItem = "Tasks";

            NavigateToTasks();
        }

        private void NavigateToTasks()
        {
            // Reload tasks to get fresh data
            _taskVm.LoadTasks();

            var taskView = new TaskView();
            taskView.DataContext = _taskVm;

            MainContent.Content = taskView;
        }

        private void NavigateToProfile()
        {
            var profileView = new ProfileView();
            profileView.DataContext = _taskVm;

            MainContent.Content = profileView;
        }

        private void NavigateToAdmin()
        {
            _adminVm = new AdminViewModel();

            var adminView = new AdminView();
            adminView.DataContext = _adminVm;

            MainContent.Content = adminView;
        }

        private void OnSidebarSearchTextChanged(string searchText)
        {
            // Update TaskViewModel search text when the current view is TaskView
            if (_taskVm != null && MainContent.Content is TaskView)
            {
                _taskVm.SearchText = searchText;
            }
        }
    }
}
