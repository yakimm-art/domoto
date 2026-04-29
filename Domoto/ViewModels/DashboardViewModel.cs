using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Domoto.Helpers;
using Domoto.Models;
using Domoto.Services;

namespace Domoto.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private int _inProgressCount;
        private int _completedCount;
        private int _overdueCount;
        private ObservableCollection<TaskItem> _dailyTasks;

        public int InProgressCount
        {
            get { return _inProgressCount; }
            set { _inProgressCount = value; OnPropertyChanged("InProgressCount"); }
        }

        public int CompletedCount
        {
            get { return _completedCount; }
            set { _completedCount = value; OnPropertyChanged("CompletedCount"); }
        }

        public int OverdueCount
        {
            get { return _overdueCount; }
            set { _overdueCount = value; OnPropertyChanged("OverdueCount"); }
        }

        public ObservableCollection<TaskItem> DailyTasks
        {
            get { return _dailyTasks; }
            set { _dailyTasks = value; OnPropertyChanged("DailyTasks"); }
        }

        public ICommand NavigateToTasksCommand { get; private set; }

        public event Action NavigateToTasksRequested;

        public DashboardViewModel()
        {
            DailyTasks = new ObservableCollection<TaskItem>();
            NavigateToTasksCommand = new RelayCommand(ExecuteNavigateToTasks);
            Refresh();
        }

        private void ExecuteNavigateToTasks(object parameter)
        {
            var handler = NavigateToTasksRequested;
            if (handler != null)
                handler();
        }

        public void Refresh()
        {
            List<TaskItem> tasks;
            if (SessionService.IsAdmin)
                tasks = DatabaseService.Instance.GetAllTasks();
            else
                tasks = DatabaseService.Instance.GetTasks(SessionService.CurrentUser.Id);

            InProgressCount = tasks.Count(t => !t.IsCompleted);
            CompletedCount = tasks.Count(t => t.IsCompleted);
            OverdueCount = tasks.Count(t => t.IsOverdue);

            DailyTasks.Clear();
            DateTime today = DateTime.Now.Date;
            foreach (var task in tasks.Where(t => t.DueDate.Date == today))
            {
                DailyTasks.Add(task);
            }
        }
    }
}
