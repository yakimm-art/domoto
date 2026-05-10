using System;
using System.Collections.Generic;
using System.Linq;
using Domoto.Helpers;
using Domoto.Models;

namespace Domoto.Services
{
    /// <summary>
    /// In-memory backing store used during UI development.
    /// Replace with a real persistence layer (SQLite, SQL Server, etc.) later.
    /// All data is lost when the process exits — that's intentional for now.
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

        // ------------------------------------------------------------
        // Seeding — hardcoded users so login works out of the box
        // ------------------------------------------------------------
        private void SeedData()
        {
            // Admin account
            var admin = new User
            {
                Id = _nextUserId++,
                Username = "admin",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                Role = "Admin"
            };
            _users.Add(admin);

            // Regular user account
            var user = new User
            {
                Id = _nextUserId++,
                Username = "user",
                PasswordHash = PasswordHelper.HashPassword("user123"),
                Role = "User"
            };
            _users.Add(user);

            // A couple of sample tasks so the dashboard isn't empty
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++,
                UserId = admin.Id,
                Title = "Welcome to Domoto",
                Description = "This is a sample task. Feel free to edit or delete it.",
                DueDate = DateTime.Now.AddDays(1),
                Priority = "Low",
                Category = "Personal",
                IsCompleted = false,
                CreatedDate = DateTime.Now
            });
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++,
                UserId = admin.Id,
                Title = "Review project requirements",
                Description = "Read through the spec and plan your work.",
                DueDate = DateTime.Now.AddDays(3),
                Priority = "High",
                Category = "Work",
                IsCompleted = false,
                CreatedDate = DateTime.Now
            });
            _tasks.Add(new TaskItem
            {
                Id = _nextTaskId++,
                UserId = user.Id,
                Title = "Try out the UI",
                Description = "Explore dashboard, tasks, and profile.",
                DueDate = DateTime.Now.AddHours(6),
                Priority = "Medium",
                Category = "Personal",
                IsCompleted = false,
                CreatedDate = DateTime.Now
            });
        }

        // ------------------------------------------------------------
        // User methods
        // ------------------------------------------------------------
        public User AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || password == null) return null;

            var u = _users.FirstOrDefault(x =>
                string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
            if (u == null) return null;

            return PasswordHelper.VerifyPassword(password, u.PasswordHash) ? u : null;
        }

        public bool RegisterUser(string username, string password, string role = "User")
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            if (_users.Any(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase)))
                return false;

            _users.Add(new User
            {
                Id = _nextUserId++,
                Username = username,
                PasswordHash = PasswordHelper.HashPassword(password),
                Role = string.IsNullOrWhiteSpace(role) ? "User" : role
            });
            return true;
        }

        public List<User> GetAllUsers()
        {
            return _users.OrderBy(u => u.Username).ToList();
        }

        public bool DeleteUser(int userId)
        {
            var u = _users.FirstOrDefault(x => x.Id == userId);
            if (u == null) return false;

            _tasks.RemoveAll(t => t.UserId == userId);
            _users.Remove(u);
            return true;
        }

        public bool UpdateUsername(int userId, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername)) return false;

            // Reject if another user already has the new username
            if (_users.Any(x => x.Id != userId &&
                                string.Equals(x.Username, newUsername, StringComparison.OrdinalIgnoreCase)))
                return false;

            var u = _users.FirstOrDefault(x => x.Id == userId);
            if (u == null) return false;

            u.Username = newUsername;
            return true;
        }

        public bool UpdateUserPassword(int userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword)) return false;

            var u = _users.FirstOrDefault(x => x.Id == userId);
            if (u == null) return false;

            u.PasswordHash = PasswordHelper.HashPassword(newPassword);
            return true;
        }

        // ------------------------------------------------------------
        // Task methods
        // ------------------------------------------------------------
        public List<TaskItem> GetTasks(int userId)
        {
            return _tasks.Where(t => t.UserId == userId)
                         .OrderBy(t => t.DueDate)
                         .ToList();
        }

        public List<TaskItem> GetAllTasks()
        {
            return _tasks.OrderBy(t => t.DueDate).ToList();
        }

        public bool AddTask(TaskItem task)
        {
            if (task == null) return false;
            task.Id = _nextTaskId++;
            _tasks.Add(task);
            return true;
        }

        public bool UpdateTask(TaskItem task)
        {
            if (task == null) return false;

            var existing = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existing == null) return false;

            existing.Title       = task.Title;
            existing.Description = task.Description;
            existing.DueDate     = task.DueDate;
            existing.Priority    = task.Priority;
            existing.Category    = task.Category;
            existing.IsCompleted = task.IsCompleted;
            return true;
        }

        public bool DeleteTask(int taskId)
        {
            var t = _tasks.FirstOrDefault(x => x.Id == taskId);
            if (t == null) return false;
            return _tasks.Remove(t);
        }
    }
}
