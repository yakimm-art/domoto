using System.Windows;
using System.Windows.Controls;
using Domoto.ViewModels;

namespace Domoto.Views
{
    public partial class AdminView : UserControl
    {
        public AdminView()
        {
            InitializeComponent();
        }

        private void AdminPwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AdminViewModel;
            if (vm != null)
                vm.NewPassword = ((PasswordBox)sender).Password;
        }
    }
}
