using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using Domoto.Models;
using Domoto.Helpers;

namespace Domoto.Services
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        private readonly string _connectionString;

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
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TaskManager");
            Directory.CreateDirectory(dir);
            string dbPath = Path.Combine(dir, "taskmanager.db");
            _connectionString = "Data Source=" + dbPath + ";Version=3;";

            InitializeSchema();
            SeedIfEmpty();
        }

        // Opens a connection with foreign keys enabled
        private SQLiteConnection Open()
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using (var pragma = conn.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }
            return conn;
        }

        private void InitializeSchema()
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username     TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        Role         TEXT NOT NULL DEFAULT 'User'
                    );
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId      INTEGER NOT NULL,
                        Title       TEXT NOT NULL,
                        Description TEXT,
                        DueDate     TEXT NOT NULL,
                        Priority    TEXT NOT NULL DEFAULT 'Medium',
                        Category    TEXT NOT NULL DEFAULT 'Work',
                        IsCompleted INTEGER NOT NULL DEFAULT 0,
                        CreatedDate TEXT NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        private void SeedIfEmpty()
        {
            using (var conn = Open())
            {
                // Seed admin user if no users exist
                int userCount;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users;";
                    userCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (userCount == 0)
                {
                    int adminId;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Users (Username, PasswordHash, Role)
                                            VALUES (@u, @p, @r);
                                            SELECT last_insert_rowid();";
                        cmd.Parameters.AddWithValue("@u", "admin");
                        cmd.Parameters.AddWithValue("@p", PasswordHelper.HashPassword("admin123"));
                        cmd.Parameters.AddWithValue("@r", "Admin");
                        adminId = (int)(long)cmd.ExecuteScalar();
                    }

                    // Seed a couple of sample tasks for first-run convenience
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                    string nextWeek = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss");

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Tasks
                            (UserId, Title, Description, DueDate, Priority, Category, IsCompleted, CreatedDate)
                            VALUES
                            (@uid, 'Welcome to TaskManager', 'This is a sample task. Feel free to delete it.', @due1, 'Low', 'Personal', 0, @now),
                            (@uid, 'Review project requirements', 'Read through the project spec and plan your work.', @due2, 'High', 'Work', 0, @now);";
                        cmd.Parameters.AddWithValue("@uid", adminId);
                        cmd.Parameters.AddWithValue("@due1", tomorrow);
                        cmd.Parameters.AddWithValue("@due2", nextWeek);
                        cmd.Parameters.AddWithValue("@now", now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // ---- User Methods ----

        public User AuthenticateUser(string username, string password)
        {
            string hash = PasswordHelper.HashPassword(password);
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users WHERE Username = @u AND PasswordHash = @p;";
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", hash);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return ReadUser(reader);
                }
            }
            return null;
        }

        public bool RegisterUser(string username, string password, string role = "User")
        {
            try
            {
                using (var conn = Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@u, @p, @r);";
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", PasswordHelper.HashPassword(password));
                    cmd.Parameters.AddWithValue("@r", role);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch { return false; }
        }

        public List<User> GetAllUsers()
        {
            var list = new List<User>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users ORDER BY Username;";
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(ReadUser(reader));
            }
            return list;
        }

        public bool DeleteUser(int userId)
        {
            using (var conn = Open())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    // Tasks are removed by ON DELETE CASCADE, but we do it
                    // explicitly too for safety in case FK pragma was off
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "DELETE FROM Tasks WHERE UserId = @id;";
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "DELETE FROM Users WHERE Id = @id;";
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
            }
        }

        public bool UpdateUsername(int userId, string newUsername)
        {
            try
            {
                using (var conn = Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET Username = @u WHERE Id = @id;";
                    cmd.Parameters.AddWithValue("@u", newUsername);
                    cmd.Parameters.AddWithValue("@id", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch { return false; }
        }

        public bool UpdateUserPassword(int userId, string newPassword)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Users SET PasswordHash = @p WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@p", PasswordHelper.HashPassword(newPassword));
                cmd.Parameters.AddWithValue("@id", userId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ---- Task Methods ----

        public List<TaskItem> GetTasks(int userId)
        {
            var list = new List<TaskItem>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Tasks WHERE UserId = @uid ORDER BY DueDate;";
                cmd.Parameters.AddWithValue("@uid", userId);
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(ReadTask(reader));
            }
            return list;
        }

        public List<TaskItem> GetAllTasks()
        {
            var list = new List<TaskItem>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Tasks ORDER BY DueDate;";
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(ReadTask(reader));
            }
            return list;
        }

        public bool AddTask(TaskItem task)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Tasks
                    (UserId, Title, Description, DueDate, Priority, Category, IsCompleted, CreatedDate)
                    VALUES (@uid, @title, @desc, @due, @prio, @cat, @done, @created);
                    SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@uid",     task.UserId);
                cmd.Parameters.AddWithValue("@title",   task.Title);
                cmd.Parameters.AddWithValue("@desc",    task.Description ?? "");
                cmd.Parameters.AddWithValue("@due",     task.DueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@prio",    task.Priority ?? "Medium");
                cmd.Parameters.AddWithValue("@cat",     task.Category ?? "Work");
                cmd.Parameters.AddWithValue("@done",    task.IsCompleted ? 1 : 0);
                cmd.Parameters.AddWithValue("@created", task.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                task.Id = (int)(long)cmd.ExecuteScalar();
                return true;
            }
        }

        public bool UpdateTask(TaskItem task)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE Tasks SET
                    Title       = @title,
                    Description = @desc,
                    DueDate     = @due,
                    Priority    = @prio,
                    Category    = @cat,
                    IsCompleted = @done
                    WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@desc",  task.Description ?? "");
                cmd.Parameters.AddWithValue("@due",   task.DueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@prio",  task.Priority ?? "Medium");
                cmd.Parameters.AddWithValue("@cat",   task.Category ?? "Work");
                cmd.Parameters.AddWithValue("@done",  task.IsCompleted ? 1 : 0);
                cmd.Parameters.AddWithValue("@id",    task.Id);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteTask(int taskId)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Tasks WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", taskId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ---- Helpers ----

        private static User ReadUser(SQLiteDataReader r)
        {
            return new User
            {
                Id       = Convert.ToInt32(r["Id"]),
                Username = r["Username"].ToString(),
                Role     = r["Role"].ToString()
            };
        }

        private static TaskItem ReadTask(SQLiteDataReader r)
        {
            return new TaskItem
            {
                Id          = Convert.ToInt32(r["Id"]),
                UserId      = Convert.ToInt32(r["UserId"]),
                Title       = r["Title"].ToString(),
                Description = r["Description"].ToString(),
                DueDate     = DateTime.Parse(r["DueDate"].ToString()),
                Priority    = r["Priority"].ToString(),
                Category    = r["Category"].ToString(),
                IsCompleted = Convert.ToInt32(r["IsCompleted"]) == 1,
                CreatedDate = DateTime.Parse(r["CreatedDate"].ToString())
            };
        }
    }
}
