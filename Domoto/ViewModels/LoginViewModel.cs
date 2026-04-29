using System;
using System.Windows;
using System.Windows.Input;
using Domoto.Helpers;
using Domoto.Services;

namespace Domoto.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isRegistering;
        private string _registerUsername;
        private string _registerPassword;
        private string _registerConfirmPassword;

        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged("Username"); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged("Password"); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged("ErrorMessage"); }
        }

        public bool IsRegistering
        {
            get { return _isRegistering; }
            set { _isRegistering = value; OnPropertyChanged("IsRegistering"); OnPropertyChanged("IsNotRegistering"); }
        }

        public bool IsNotRegistering
        {
            get { return !_isRegistering; }
        }

        public string RegisterUsername
        {
            get { return _registerUsername; }
            set { _registerUsername = value; OnPropertyChanged("RegisterUsername"); }
        }

        public string RegisterPassword
        {
            get { return _registerPassword; }
            set { _registerPassword = value; OnPropertyChanged("RegisterPassword"); }
        }

        public string RegisterConfirmPassword
        {
            get { return _registerConfirmPassword; }
            set { _registerConfirmPassword = value; OnPropertyChanged("RegisterConfirmPassword"); }
        }

        public event Action<bool> LoginCompleted;

        public ICommand LoginCommand { get; private set; }
        public ICommand ShowRegisterCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand BackToLoginCommand { get; private set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            ShowRegisterCommand = new RelayCommand(o => { IsRegistering = true; ErrorMessage = ""; });
            RegisterCommand = new RelayCommand(ExecuteRegister);
            BackToLoginCommand = new RelayCommand(o => { IsRegistering = false; ErrorMessage = ""; });
        }

        private void ExecuteLogin(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter username and password.";
                return;
            }

            var user = DatabaseService.Instance.AuthenticateUser(Username, Password);
            if (user != null)
            {
                SessionService.CurrentUser = user;
                ErrorMessage = "";
                if (LoginCompleted != null)
                    LoginCompleted(true);
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }
        }

        private void ExecuteRegister(object parameter)
        {
            if (string.IsNullOrWhiteSpace(RegisterUsername) || string.IsNullOrWhiteSpace(RegisterPassword))
            {
                ErrorMessage = "Please fill in all fields.";
                return;
            }

            if (RegisterPassword != RegisterConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            if (RegisterPassword.Length < 4)
            {
                ErrorMessage = "Password must be at least 4 characters.";
                return;
            }

            bool success = DatabaseService.Instance.RegisterUser(RegisterUsername, RegisterPassword);
            if (success)
            {
                ErrorMessage = "";
                MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                IsRegistering = false;
            }
            else
            {
                ErrorMessage = "Username already exists.";
            }
        }
    }
}
