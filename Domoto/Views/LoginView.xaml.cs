using System.Windows;
using System.Windows.Controls;
using Domoto.ViewModels;

namespace Domoto.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LoginViewModel;
            if (vm != null)
                vm.Password = ((PasswordBox)sender).Password;
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
