using System.Windows;
using System.Windows.Controls;
using Domoto.ViewModels;

namespace Domoto.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private void PwdNew_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TaskViewModel;
            if (vm != null)
                vm.NewPassword = ((PasswordBox)sender).Password;
        }

        private void PwdConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TaskViewModel;
            if (vm != null)
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
        }
    }
}
