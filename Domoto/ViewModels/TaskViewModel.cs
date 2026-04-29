using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Domoto.Helpers;
using Domoto.Models;
using Domoto.Services;

namespace Domoto.ViewModels
{
    public class TaskViewModel : BaseViewModel
    {
        private ObservableCollection<TaskItem> _allTasks;
        private ICollectionView _tasksView;
        private TaskItem _selectedTask;
        private string _searchText;
        private string _filterPriority;
        private string _filterCategory;
        private string _filterStatus;
        private string _sortBy;

        // Task form fields
        private string _taskTitle;
        private string _taskDescription;
        private DateTime _taskDueDate;
        private string _taskPriority;
        private string _taskCategory;
        private bool _isEditing;
        private int _editingTaskId;

        // Summary
        private int _totalTasks;
        private int _completedTasks;
        private int _overdueTasks;
        private int _pendingTasks;

        // Profile
        private string _newUsername;
        private string _newPassword;
        private string _confirmPassword;
        private string _profileMessage;

        public ObservableCollection<TaskItem> AllTasks
        {
            get { return _allTasks; }
            set { _allTasks = value; OnPropertyChanged("AllTasks"); }
        }

        public ICollectionView TasksView
        {
            get { return _tasksView; }
            set { _tasksView = value; OnPropertyChanged("TasksView"); }
        }

        public TaskItem SelectedTask
        {
            get { return _selectedTask; }
            set { _selectedTask = value; OnPropertyChanged("SelectedTask"); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; OnPropertyChanged("SearchText"); ApplyFilter(); }
        }

        public string FilterPriority
        {
            get { return _filterPriority; }
            set { _filterPriority = value; OnPropertyChanged("FilterPriority"); ApplyFilter(); }
        }

        public string FilterCategory
        {
            get { return _filterCategory; }
            set { _filterCategory = value; OnPropertyChanged("FilterCategory"); ApplyFilter(); }
        }

        public string FilterStatus
        {
            get { return _filterStatus; }
            set { _filterStatus = value; OnPropertyChanged("FilterStatus"); ApplyFilter(); }
        }

        public string SortBy
        {
            get { return _sortBy; }
            set { _sortBy = value; OnPropertyChanged("SortBy"); ApplySort(); }
        }

        public string TaskTitle
        {
            get { return _taskTitle; }
            set { _taskTitle = value; OnPropertyChanged("TaskTitle"); }
        }

        public string TaskDescription
        {
            get { return _taskDescription; }
            set { _taskDescription = value; OnPropertyChanged("TaskDescription"); }
        }

        public DateTime TaskDueDate
        {
            get { return _taskDueDate; }
            set { _taskDueDate = value; OnPropertyChanged("TaskDueDate"); }
        }

        public string TaskPriority
        {
            get { return _taskPriority; }
            set { _taskPriority = value; OnPropertyChanged("TaskPriority"); }
        }

        public string TaskCategory
        {
            get { return _taskCategory; }
            set { _taskCategory = value; OnPropertyChanged("TaskCategory"); }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set { _isEditing = value; OnPropertyChanged("IsEditing"); OnPropertyChanged("FormTitle"); }
        }

        public string FormTitle
        {
            get { return _isEditing ? "Edit Task" : "New Task"; }
        }

        public int TotalTasks
        {
            get { return _totalTasks; }
            set { _totalTasks = value; OnPropertyChanged("TotalTasks"); }
        }

        public int CompletedTasks
        {
            get { return _completedTasks; }
            set { _completedTasks = value; OnPropertyChanged("CompletedTasks"); }
        }

        public int OverdueTasks
        {
            get { return _overdueTasks; }
            set { _overdueTasks = value; OnPropertyChanged("OverdueTasks"); }
        }

        public int PendingTasks
        {
            get { return _pendingTasks; }
            set { _pendingTasks = value; OnPropertyChanged("PendingTasks"); }
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

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { _confirmPassword = value; OnPropertyChanged("ConfirmPassword"); }
        }

        public string ProfileMessage
        {
            get { return _profileMessage; }
            set { _profileMessage = value; OnPropertyChanged("ProfileMessage"); }
        }

        public bool IsAdmin
        {
            get { return SessionService.IsAdmin; }
        }

        public string CurrentUsername
        {
            get { return SessionService.CurrentUser != null ? SessionService.CurrentUser.Username : ""; }
        }

        public string CurrentRole
        {
            get { return SessionService.CurrentUser != null ? SessionService.CurrentUser.Role : ""; }
        }

        public List<string> Priorities { get { return new List<string> { "Low", "Medium", "High" }; } }
        public List<string> Categories { get { return new List<string> { "Work", "Personal", "Other" }; } }
        public List<string> FilterPriorities { get { return new List<string> { "All", "Low", "Medium", "High" }; } }
        public List<string> FilterCategories { get { return new List<string> { "All", "Work", "Personal", "Other" }; } }
        public List<string> FilterStatuses { get { return new List<string> { "All", "Incomplete", "Complete", "Overdue" }; } }
        public List<string> SortOptions { get { return new List<string> { "Due Date", "Priority", "Category", "Title" }; } }

        // Commands
        public ICommand AddTaskCommand { get; private set; }
        public ICommand SaveTaskCommand { get; private set; }
        public ICommand DeleteTaskCommand { get; private set; }
        public ICommand EditTaskCommand { get; private set; }
        public ICommand ToggleCompleteCommand { get; private set; }
        public ICommand ClearFormCommand { get; private set; }
        public ICommand ExportCsvCommand { get; private set; }
        public ICommand UpdateProfileCommand { get; private set; }
        public ICommand ChangePasswordCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand ClearFiltersCommand { get; private set; }

        public event Action LogoutRequested;

        public TaskViewModel()
        {
            AllTasks = new ObservableCollection<TaskItem>();
            FilterPriority = "All";
            FilterCategory = "All";
            FilterStatus = "All";
            SortBy = "Due Date";
            TaskDueDate = DateTime.Now.AddDays(1);
            TaskPriority = "Medium";
            TaskCategory = "Work";

            AddTaskCommand = new RelayCommand(ExecuteAddTask);
            SaveTaskCommand = new RelayCommand(ExecuteSaveTask);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask);
            EditTaskCommand = new RelayCommand(ExecuteEditTask);
            ToggleCompleteCommand = new RelayCommand(ExecuteToggleComplete);
            ClearFormCommand = new RelayCommand(o => ClearForm());
            ExportCsvCommand = new RelayCommand(ExecuteExportCsv);
            UpdateProfileCommand = new RelayCommand(ExecuteUpdateProfile);
            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);

            LoadTasks();
        }

        public void LoadTasks()
        {
            List<TaskItem> tasks;
            if (SessionService.IsAdmin)
                tasks = DatabaseService.Instance.GetAllTasks();
            else
                tasks = DatabaseService.Instance.GetTasks(SessionService.CurrentUser.Id);

            AllTasks.Clear();
            foreach (var t in tasks)
                AllTasks.Add(t);

            TasksView = CollectionViewSource.GetDefaultView(AllTasks);
            TasksView.Filter = TaskFilter;
            UpdateSummary();
            CheckDueSoonNotifications();
        }

        private bool TaskFilter(object obj)
        {
            var task = obj as TaskItem;
            if (task == null) return false;

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.ToLower();
                bool match = (task.Title != null && task.Title.ToLower().Contains(search)) ||
                             (task.Description != null && task.Description.ToLower().Contains(search)) ||
                             (task.Category != null && task.Category.ToLower().Contains(search));
                if (!match) return false;
            }

            // Priority filter
            if (FilterPriority != "All" && task.Priority != FilterPriority)
                return false;

            // Category filter
            if (FilterCategory != "All" && task.Category != FilterCategory)
                return false;

            // Status filter
            if (FilterStatus == "Incomplete" && task.IsCompleted) return false;
            if (FilterStatus == "Complete" && !task.IsCompleted) return false;
            if (FilterStatus == "Overdue" && !task.IsOverdue) return false;

            return true;
        }

        private void ApplyFilter()
        {
            if (TasksView != null)
                TasksView.Refresh();
        }

        private void ApplySort()
        {
            if (TasksView == null) return;
            TasksView.SortDescriptions.Clear();
            switch (SortBy)
            {
                case "Due Date":
                    TasksView.SortDescriptions.Add(new SortDescription("DueDate", ListSortDirection.Ascending));
                    break;
                case "Priority":
                    TasksView.SortDescriptions.Add(new SortDescription("Priority", ListSortDirection.Descending));
                    break;
                case "Category":
                    TasksView.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
                    break;
                case "Title":
                    TasksView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
                    break;
            }
        }

        private void ExecuteAddTask(object parameter)
        {
            IsEditing = false;
            ClearForm();
        }

        private void ExecuteSaveTask(object parameter)
        {
            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                MessageBox.Show("Please enter a task title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEditing)
            {
                var task = AllTasks.FirstOrDefault(t => t.Id == _editingTaskId);
                if (task != null)
                {
                    task.Title = TaskTitle;
                    task.Description = TaskDescription;
                    task.DueDate = TaskDueDate;
                    task.Priority = TaskPriority;
                    task.Category = TaskCategory;
                    DatabaseService.Instance.UpdateTask(task);
                }
            }
            else
            {
                var task = new TaskItem
                {
                    UserId = SessionService.CurrentUser.Id,
                    Title = TaskTitle,
                    Description = TaskDescription ?? "",
                    DueDate = TaskDueDate,
                    Priority = TaskPriority,
                    Category = TaskCategory,
                    CreatedDate = DateTime.Now
                };
                DatabaseService.Instance.AddTask(task);
            }

            ClearForm();
            LoadTasks();
        }

        private void ExecuteDeleteTask(object parameter)
        {
            var task = parameter as TaskItem;
            if (task == null && SelectedTask != null)
                task = SelectedTask;
            if (task == null) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this task?\n\n" + task.Title,
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DatabaseService.Instance.DeleteTask(task.Id);
                LoadTasks();
            }
        }

        private void ExecuteEditTask(object parameter)
        {
            var task = parameter as TaskItem;
            if (task == null) return;

            IsEditing = true;
            _editingTaskId = task.Id;
            TaskTitle = task.Title;
            TaskDescription = task.Description;
            TaskDueDate = task.DueDate;
            TaskPriority = task.Priority;
            TaskCategory = task.Category;
        }

        private void ExecuteToggleComplete(object parameter)
        {
            var task = parameter as TaskItem;
            if (task == null) return;

            task.IsCompleted = !task.IsCompleted;
            DatabaseService.Instance.UpdateTask(task);
            LoadTasks();
        }

        private void ClearForm()
        {
            IsEditing = false;
            _editingTaskId = 0;
            TaskTitle = "";
            TaskDescription = "";
            TaskDueDate = DateTime.Now.AddDays(1);
            TaskPriority = "Medium";
            TaskCategory = "Work";
        }

        private void ExecuteExportCsv(object parameter)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "CSV Files (*.csv)|*.csv";
            dialog.DefaultExt = ".csv";
            dialog.FileName = "tasks_export";

            if (dialog.ShowDialog() == true)
            {
                var tasks = AllTasks.ToList();
                ExportService.ExportToCsv(tasks, dialog.FileName);
                MessageBox.Show("Tasks exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteUpdateProfile(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewUsername))
            {
                ProfileMessage = "Please enter a username.";
                return;
            }

            bool success = DatabaseService.Instance.UpdateUsername(SessionService.CurrentUser.Id, NewUsername);
            if (success)
            {
                SessionService.CurrentUser.Username = NewUsername;
                OnPropertyChanged("CurrentUsername");
                ProfileMessage = "Username updated successfully!";
            }
            else
            {
                ProfileMessage = "Username already taken.";
            }
        }

        private void ExecuteChangePassword(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ProfileMessage = "Please enter a new password.";
                return;
            }
            if (NewPassword != ConfirmPassword)
            {
                ProfileMessage = "Passwords do not match.";
                return;
            }
            if (NewPassword.Length < 4)
            {
                ProfileMessage = "Password must be at least 4 characters.";
                return;
            }

            DatabaseService.Instance.UpdateUserPassword(SessionService.CurrentUser.Id, NewPassword);
            ProfileMessage = "Password changed successfully!";
            NewPassword = "";
            ConfirmPassword = "";
        }

        private void ExecuteLogout(object parameter)
        {
            SessionService.Logout();
            if (LogoutRequested != null)
                LogoutRequested();
        }

        private void ExecuteClearFilters(object parameter)
        {
            SearchText = "";
            FilterPriority = "All";
            FilterCategory = "All";
            FilterStatus = "All";
            SortBy = "Due Date";
        }

        private void UpdateSummary()
        {
            TotalTasks = AllTasks.Count;
            CompletedTasks = AllTasks.Count(t => t.IsCompleted);
            OverdueTasks = AllTasks.Count(t => t.IsOverdue);
            PendingTasks = AllTasks.Count(t => !t.IsCompleted);
        }

        private void CheckDueSoonNotifications()
        {
            var dueSoon = AllTasks.Where(t => t.IsDueSoon).ToList();
            if (dueSoon.Count > 0)
            {
                string msg = "The following tasks are due within 24 hours:\n\n";
                foreach (var t in dueSoon)
                    msg += "• " + t.Title + " (Due: " + t.DueDate.ToString("g") + ")\n";

                MessageBox.Show(msg, "Due Soon Reminder", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
