using System;
using System.Collections.Generic;
using System.Linq;
using Domoto.Models;
using Domoto.Helpers;

namespace Domoto.Services
{
    /// <summary>
    /// In-memory DatabaseService for UI preview. Replace with SQLite version for production.
    /// </summary>
    public class DatabaseService
    {
        private static DatabaseService _instance;
        private readonly List<User> _users = new List<User>();
        private readonly List<TaskItem> _tasks = new List<TaskItem>();
        private int _nextUserId = 1;
        private int _nextTaskId = 1;

        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DatabaseService();
                return _instance;
            }
        }

        private DatabaseService()
        {
            SeedData();
        }

        private void SeedData()
        {
            // Seed users
            _users.Add(new User { Id = _nextUserId++, Username = "admin", PasswordHash = PasswordHelper.HashPassword("admin123"), Role = "Admin" });
            _users.Add(new User { Id = _nextUserId++, Username = "user", PasswordHash = PasswordHelper.HashPassword("user123"), Role = "User" });

            // Seed sample tasks for admin (userId=1)
            var now = DateTime.Now;
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++, UserId = 1, Title = "Design landing page",
                Description = "Create wireframes and mockups for the new landing page",
                DueDate = now.AddDays(2), Priority = "High", Category = "Work",
                IsCompleted = false, CreatedDate = now.AddDays(-3)
            });
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++, UserId = 1, Title = "Buy groceries",
                Description = "Milk, eggs, bread, vegetables",
                DueDate = now.Date, Priority = "Medium", Category = "Personal",
                IsCompleted = false, CreatedDate = now.AddDays(-1)
            });
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++, UserId = 1, Title = "Fix login bug",
                Description = "Users report intermittent login failures on mobile",
                DueDate = now.AddDays(-1), Priority = "High", Category = "Work",
                IsCompleted = false, CreatedDate = now.AddDays(-5)
            });
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++, UserId = 1, Title = "Write unit tests",
                Description = "Add property-based tests for validation logic",
                DueDate = now.AddDays(5), Priority = "Medium", Category = "Work",
                IsCompleted = false, CreatedDate = now.AddDays(-2)
            });
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++, UserId = 1, Title = "Read chapter 5",
                Description = "Finish reading the architecture patterns book",
                DueDate = now.AddDays(1), Priority = "Low", Category = "Personal",
                IsCompleted = true, CreatedDate = now.AddDays(-7)
            });
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++, UserId = 1, Title = "Team standup notes",
                Description = "Prepare notes for tomorrow's standup meeting",
                DueDate = now.Date, Priority = "Low", Category = "Work",
                IsCompleted = true, CreatedDate = now.AddDays(-1)
            });

            // Seed sample tasks for user (userId=2)
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++, UserId = 2, Title = "Update resume",
                Description = "Add recent project experience",
                DueDate = now.AddDays(3), Priority = "Medium", Category = "Personal",
                IsCompleted = false, CreatedDate = now.AddDays(-2)
            });
        }

        // ---- User Methods ----

        public User AuthenticateUser(string username, string password)
        {
            string hash = PasswordHelper.HashPassword(password);
            return _users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash);
        }

        public bool RegisterUser(string username, string password, string role = "User")
        {
            if (_users.Any(u => u.Username == username))
                return false;

            _users.Add(new User
            {
                Id = _nextUserId++,
                Username = username,
                PasswordHash = PasswordHelper.HashPassword(password),
                Role = role
            });
            return true;
        }

        public bool UpdateUserPassword(int userId, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            return true;
        }

        public bool UpdateUsername(int userId, string newUsername)
        {
            if (_users.Any(u => u.Username == newUsername && u.Id != userId))
                return false;
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;
            user.Username = newUsername;
            return true;
        }

        public List<User> GetAllUsers()
        {
            return _users.ToList();
        }

        public bool DeleteUser(int userId)
        {
            _tasks.RemoveAll(t => t.UserId == userId);
            return _users.RemoveAll(u => u.Id == userId) > 0;
        }

        // ---- Task Methods ----

        public List<TaskItem> GetTasks(int userId)
        {
            return _tasks.Where(t => t.UserId == userId).OrderBy(t => t.DueDate).ToList();
        }

        public List<TaskItem> GetAllTasks()
        {
            return _tasks.OrderBy(t => t.DueDate).ToList();
        }

        public bool AddTask(TaskItem task)
        {
            task.Id = _nextTaskId++;
            _tasks.Add(task);
            return true;
        }

        public bool UpdateTask(TaskItem task)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existing == null) return false;
            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.DueDate = task.DueDate;
            existing.Priority = task.Priority;
            existing.Category = task.Category;
            existing.IsCompleted = task.IsCompleted;
            return true;
        }

        public bool DeleteTask(int taskId)
        {
            return _tasks.RemoveAll(t => t.Id == taskId) > 0;
        }
    }
}
