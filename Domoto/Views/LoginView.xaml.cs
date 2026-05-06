using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Domoto.ViewModels;

namespace Domoto.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            
            // Focus the username field when the view loads
            this.Loaded += (s, e) => 
            {
                if (UsernameBox != null) Keyboard.Focus(UsernameBox);
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Using a traditional 'as' cast instead of pattern matching
            LoginViewModel vm = DataContext as LoginViewModel;
            if (vm != null)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }

        private void RegPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LoginViewModel vm = DataContext as LoginViewModel;
            if (vm != null)
            {
                vm.RegisterPassword = ((PasswordBox)sender).Password;
            }
        }

        private void RegConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LoginViewModel vm = DataContext as LoginViewModel;
            if (vm != null)
            {
                vm.RegisterConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}