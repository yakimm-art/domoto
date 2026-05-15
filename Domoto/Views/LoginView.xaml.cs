using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Domoto.ViewModels;

namespace Domoto.Views
{
    public partial class LoginView : UserControl
    {
        private bool _passwordVisible = false;

        public LoginView()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                if (UsernameBox != null) Keyboard.Focus(UsernameBox);
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LoginViewModel;
            if (vm != null)
                vm.Password = ((PasswordBox)sender).Password;

            // Keep the plain-text box in sync if visible
            if (_passwordVisible)
                PasswordVisible.Text = ((PasswordBox)sender).Password;
        }

        // Toggle show/hide password
        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible;

            if (_passwordVisible)
            {
                PasswordVisible.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordVisible.Visibility = Visibility.Visible;
                EyeIcon.Text = "Hide";
            }
            else
            {
                PasswordBox.Password = PasswordVisible.Text;
                PasswordVisible.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                EyeIcon.Text = "Show";
            }
        }

        // Press Enter on password box to sign in
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = DataContext as LoginViewModel;
                if (vm != null && vm.LoginCommand.CanExecute(null))
                    vm.LoginCommand.Execute(null);
            }
        }

        // Press Enter on username box to move focus to password
        private void UsernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Keyboard.Focus(PasswordBox);
        }

        private void RegPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LoginViewModel;
            if (vm != null)
                vm.RegisterPassword = ((PasswordBox)sender).Password;
        }

        private void RegConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LoginViewModel;
            if (vm != null)
                vm.RegisterConfirmPassword = ((PasswordBox)sender).Password;
        }
    }
}
