using System.Windows;
using System.Windows.Input;
using Domoto.Services;
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

        // ── Startup ────────────────────────────────────────────────

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply initial theme (light) and reflect the icon.
            ThemeService.Apply(AppTheme.Light);
            UpdateThemeButtonIcon();
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            // When maximized, flatten the corners and remove the drop shadow
            // so the window sits flush to the work area. When restored, bring
            // the rounded look back.
            if (WindowState == WindowState.Maximized)
            {
                RootShell.CornerRadius = new CornerRadius(0);
                RootShell.BorderThickness = new Thickness(0);
                RootShell.Effect = null;
                TitleBarBorder.CornerRadius = new CornerRadius(0);
                BtnMaximize.Content = "\u2750"; // restore glyph
                BtnMaximize.ToolTip = "Restore Down";
            }
            else
            {
                RootShell.CornerRadius = new CornerRadius(14);
                RootShell.BorderThickness = new Thickness(1);
                RootShell.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 18,
                    ShadowDepth = 0,
                    Opacity = 0.25,
                    Color = System.Windows.Media.Colors.Black
                };
                TitleBarBorder.CornerRadius = new CornerRadius(14, 14, 0, 0);
                BtnMaximize.Content = "\u2610"; // maximize glyph
                BtnMaximize.ToolTip = "Maximize";
            }
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

        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.Toggle();
            UpdateThemeButtonIcon();
        }

        private void UpdateThemeButtonIcon()
        {
            // Moon in light mode (click to go dark), sun in dark mode (click to go light).
            BtnTheme.Content = ThemeService.Current == AppTheme.Dark ? "\u2600" : "\uD83C\uDF19";
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        // ── Navigation (unchanged) ─────────────────────────────────

        private void ShowLogin()
        {
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
                SidebarControl.Visibility = Visibility.Visible;
                SidebarColumn.Width = new GridLength(260);

                _sidebarVm = new SidebarViewModel();
                _sidebarVm.RefreshUserInfo();
                _sidebarVm.NavigationRequested += OnSidebarNavigationRequested;
                _sidebarVm.LogoutRequested += OnLogoutRequested;
                _sidebarVm.SearchTextChanged += OnSidebarSearchTextChanged;
                SidebarControl.DataContext = _sidebarVm;

                _taskVm = new TaskViewModel();

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
            if (_taskVm != null && MainContent.Content is TaskView)
            {
                _taskVm.SearchText = searchText;
            }
        }
    }
}
