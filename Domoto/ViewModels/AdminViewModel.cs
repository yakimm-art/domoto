using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Domoto.Helpers;
using Domoto.Models;
using Domoto.Services;

namespace Domoto.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private ObservableCollection<User> _users;
        private User _selectedUser;
        private string _newUsername;
        private string _newPassword;
        private string _newRole;

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set { _users = value; OnPropertyChanged("Users"); }
        }

        public User SelectedUser
        {
            get { return _selectedUser; }
            set { _selectedUser = value; OnPropertyChanged("SelectedUser"); }
        }

        public string NewUsername
        {
            get { return _newUsername; }
            set { _newUsername = value; OnPropertyChanged("NewUsername"); }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set { _newPassword = value; OnPropertyChanged("NewPassword"); }
        }

        public string NewRole
        {
            get { return _newRole; }
            set { _newRole = value; OnPropertyChanged("NewRole"); }
        }

        public ICommand AddUserCommand { get; private set; }
        public ICommand DeleteUserCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        public AdminViewModel()
        {
            Users = new ObservableCollection<User>();
            NewRole = "User";
            AddUserCommand = new RelayCommand(ExecuteAddUser);
            DeleteUserCommand = new RelayCommand(ExecuteDeleteUser);
            RefreshCommand = new RelayCommand(o => LoadUsers());
            LoadUsers();
        }

        public void LoadUsers()
        {
            Users.Clear();
            foreach (var u in DatabaseService.Instance.GetAllUsers())
                Users.Add(u);
        }

        private void ExecuteAddUser(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
            {
                MessageBox.Show("Please enter username and password.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string role = string.IsNullOrWhiteSpace(NewRole) ? "User" : NewRole;
            bool success = DatabaseService.Instance.RegisterUser(NewUsername, NewPassword, role);
            if (success)
            {
                MessageBox.Show("User created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                NewUsername = "";
                NewPassword = "";
                NewRole = "User";
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Username already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteUser(object parameter)
        {
            var user = parameter as User ?? SelectedUser;
            if (user == null) return;

            if (user.Id == SessionService.CurrentUser.Id)
            {
                MessageBox.Show("You cannot delete your own account.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                "Delete user '" + user.Username + "' and all their tasks?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DatabaseService.Instance.DeleteUser(user.Id);
                LoadUsers();
            }
        }
    }
}
