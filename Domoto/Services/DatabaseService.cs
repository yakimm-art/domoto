using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;
using Domoto.Helpers;
using Domoto.Models;

namespace Domoto.Services
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        private const string DefaultDatabaseName = "taskmanager";
        private const string DefaultServerConnectionString = "Server=localhost;Port=3306;Uid=taskmanager_user;Pwd=manager;SslMode=none;";
        private readonly string _serverConnectionString;
        private readonly string _databaseConnectionString;
        private readonly string _databaseName;

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
            _serverConnectionString = ReadSetting("MySql.ServerConnectionString", DefaultServerConnectionString);
            _databaseName = ReadSetting("MySql.DatabaseName", DefaultDatabaseName);
            _databaseConnectionString = _serverConnectionString + "Database=" + _databaseName + ";";
            InitializeSchema();
            SeedIfEmpty();
        }

        private static string ReadSetting(string key, string fallback)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private MySqlConnection OpenServer()
        {
            var conn = new MySqlConnection(_serverConnectionString);
            conn.Open();
            return conn;
        }

        private MySqlConnection OpenDatabase()
        {
            var conn = new MySqlConnection(_databaseConnectionString);
            conn.Open();
            return conn;
        }

        private void InitializeSchema()
        {
            using (var conn = OpenServer())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE DATABASE IF NOT EXISTS `" + _databaseName + "`;";
                cmd.ExecuteNonQuery();
            }

            using (var conn = OpenDatabase())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id           INT AUTO_INCREMENT PRIMARY KEY,
                        Username     VARCHAR(255) NOT NULL UNIQUE,
                        PasswordHash VARCHAR(255) NOT NULL,
                        Role         VARCHAR(50) NOT NULL DEFAULT 'User'
                    ) ENGINE=InnoDB;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        UserId      INT NOT NULL,
                        Title       VARCHAR(255) NOT NULL,
                        Description TEXT,
                        DueDate     DATETIME NOT NULL,
                        Priority    VARCHAR(20) NOT NULL DEFAULT 'Medium',
                        Category    VARCHAR(50) NOT NULL DEFAULT 'Work',
                        IsCompleted TINYINT(1) NOT NULL DEFAULT 0,
                        CreatedDate DATETIME NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                    ) ENGINE=InnoDB;";
                cmd.ExecuteNonQuery();
            }
        }

        private void SeedIfEmpty()
        {
            using (var conn = OpenDatabase())
            {
                int userCount;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users;";
                    userCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int adminId = 0;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id FROM Users WHERE Username = @u LIMIT 1;";
                    cmd.Parameters.AddWithValue("@u", "admin");
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        adminId = Convert.ToInt32(result);
                }

                if (adminId == 0)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Users (Username, PasswordHash, Role)
                            VALUES (@u, @p, @r);
                            SELECT LAST_INSERT_ID();";
                        cmd.Parameters.AddWithValue("@u", "admin");
                        cmd.Parameters.AddWithValue("@p", PasswordHelper.HashPassword("admin123"));
                        cmd.Parameters.AddWithValue("@r", "Admin");
                        adminId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                bool defaultUserExists;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @u;";
                    cmd.Parameters.AddWithValue("@u", "user");
                    defaultUserExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

                if (!defaultUserExists)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@u, @p, @r);";
                        cmd.Parameters.AddWithValue("@u", "user");
                        cmd.Parameters.AddWithValue("@p", PasswordHelper.HashPassword("user123"));
                        cmd.Parameters.AddWithValue("@r", "User");
                        cmd.ExecuteNonQuery();
                    }
                }

                int taskCount;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Tasks;";
                    taskCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (taskCount == 0 && adminId > 0)
                {
                    var now = DateTime.Now;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Tasks
                            (UserId, Title, Description, DueDate, Priority, Category, IsCompleted, CreatedDate)
                            VALUES
                            (@uid, 'Welcome to Domoto', 'This is a sample task. Feel free to edit or delete it.', @due1, 'Low', 'Personal', 0, @now),
                            (@uid, 'Review project requirements', 'Read through the spec and plan your work.', @due2, 'High', 'Work', 0, @now);";
                        cmd.Parameters.AddWithValue("@uid", adminId);
                        cmd.Parameters.AddWithValue("@due1", now.AddDays(1));
                        cmd.Parameters.AddWithValue("@due2", now.AddDays(3));
                        cmd.Parameters.AddWithValue("@now", now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public User AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || password == null)
                return null;

            using (var conn = OpenDatabase())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users WHERE Username = @u;";
                cmd.Parameters.AddWithValue("@u", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    var storedHash = reader["PasswordHash"].ToString();
                    if (!PasswordHelper.VerifyPassword(password, storedHash))
                        return null;

                    var user = ReadUser(reader);
                    if (PasswordHelper.NeedsRehash(storedHash))
                    {
                        reader.Close();
                        using (var update = conn.CreateCommand())
                        {
                            update.CommandText = "UPDATE Users SET PasswordHash = @p WHERE Id = @id;";
                            update.Parameters.AddWithValue("@p", PasswordHelper.HashPassword(password));
                            update.Parameters.AddWithValue("@id", user.Id);
                            update.ExecuteNonQuery();
                        }
                    }

                    return user;
                }
            }
        }

        public bool RegisterUser(string username, string password, string role = "User")
        {
            try
            {
                using (var conn = OpenDatabase())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@u, @p, @r);";
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", PasswordHelper.HashPassword(password));
                    cmd.Parameters.AddWithValue("@r", role);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<User> GetAllUsers()
        {
            var list = new List<User>();
            using (var conn = OpenDatabase())
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
            using (var conn = OpenDatabase())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
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
                using (var conn = OpenDatabase())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Users SET Username = @u WHERE Id = @id;";
                    cmd.Parameters.AddWithValue("@u", newUsername);
                    cmd.Parameters.AddWithValue("@id", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateUserPassword(int userId, string newPassword)
        {
            using (var conn = OpenDatabase())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Users SET PasswordHash = @p WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@p", PasswordHelper.HashPassword(newPassword));
                cmd.Parameters.AddWithValue("@id", userId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public List<TaskItem> GetTasks(int userId)
        {
            var list = new List<TaskItem>();
            using (var conn = OpenDatabase())
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
            using (var conn = OpenDatabase())
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
            using (var conn = OpenDatabase())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Tasks
                    (UserId, Title, Description, DueDate, Priority, Category, IsCompleted, CreatedDate)
                    VALUES (@uid, @title, @desc, @due, @prio, @cat, @done, @created);";
                cmd.Parameters.AddWithValue("@uid", task.UserId);
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@desc", task.Description ?? "");
                cmd.Parameters.AddWithValue("@due", task.DueDate);
                cmd.Parameters.AddWithValue("@prio", task.Priority ?? "Medium");
                cmd.Parameters.AddWithValue("@cat", task.Category ?? "Work");
                cmd.Parameters.AddWithValue("@done", task.IsCompleted ? 1 : 0);
                cmd.Parameters.AddWithValue("@created", task.CreatedDate);
                if (cmd.ExecuteNonQuery() <= 0)
                    return false;
                task.Id = (int)cmd.LastInsertedId;
                return true;
            }
        }

        public bool UpdateTask(TaskItem task)
        {
            using (var conn = OpenDatabase())
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
                cmd.Parameters.AddWithValue("@desc", task.Description ?? "");
                cmd.Parameters.AddWithValue("@due", task.DueDate);
                cmd.Parameters.AddWithValue("@prio", task.Priority ?? "Medium");
                cmd.Parameters.AddWithValue("@cat", task.Category ?? "Work");
                cmd.Parameters.AddWithValue("@done", task.IsCompleted ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", task.Id);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteTask(int taskId)
        {
            using (var conn = OpenDatabase())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Tasks WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", taskId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private static User ReadUser(MySqlDataReader r)
        {
            return new User
            {
                Id = Convert.ToInt32(r["Id"]),
                Username = r["Username"].ToString(),
                Role = r["Role"].ToString()
            };
        }

        private static TaskItem ReadTask(MySqlDataReader r)
        {
            return new TaskItem
            {
                Id = Convert.ToInt32(r["Id"]),
                UserId = Convert.ToInt32(r["UserId"]),
                Title = r["Title"].ToString(),
                Description = r["Description"].ToString(),
                DueDate = Convert.ToDateTime(r["DueDate"]),
                Priority = r["Priority"].ToString(),
                Category = r["Category"].ToString(),
                IsCompleted = Convert.ToInt32(r["IsCompleted"]) == 1,
                CreatedDate = Convert.ToDateTime(r["CreatedDate"])
            };
        }
    }
}
