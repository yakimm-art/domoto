# Domoto — Task Manager

A WPF desktop task management application built with C#, MVVM pattern, and a neobrutalism UI theme. Manage tasks with priorities, categories, due dates, filtering, and CSV export.

## Screenshots

The app features a dark sidebar, colorful dashboard cards, and bold neobrutalism styling with hard shadows and vivid accents.

## Prerequisites

- **Windows** (7 or later)
- **.NET Framework 4.0** (pre-installed on Windows 8+; [download for Windows 7](https://dotnet.microsoft.com/download/dotnet-framework/net40))
- **SharpDevelop 5.1** or **Visual Studio 2010+** (any edition)

## Quick Start (In-Memory Mode)

The app currently runs with an **in-memory database** — no SQLite setup needed. Just build and run.

1. Open `Domoto.sln` in SharpDevelop or Visual Studio
2. Build the solution (F6 or Build > Build Solution)
3. Run (F5)
4. Log in with one of the pre-loaded accounts:

| Username | Password   | Role  | Notes                        |
|----------|------------|-------|------------------------------|
| admin    | admin123   | Admin | Sees all tasks, admin panel  |
| user     | user123    | User  | Sees only their own tasks    |

Data resets each time you restart the app since it's stored in memory.

## Setting Up SQLite (Persistent Database)

To switch from in-memory to persistent SQLite storage:

### Step 1: Install the SQLite NuGet Package

The project already references `System.Data.SQLite.Core 1.0.118.0` in `packages.config`. Restore it:

**Option A — SharpDevelop:**
- Right-click the solution > Manage NuGet Packages > Restore

**Option B — Command line:**
1. Download [nuget.exe](https://www.nuget.org/downloads) and place it in the solution folder
2. If this is your first time, add the NuGet source:
   ```
   nuget sources add -Name "nuget.org" -Source "https://api.nuget.org/v3/index.json"
   ```
3. Restore packages:
   ```
   nuget restore Domoto.sln
   ```

The SQLite DLL will be downloaded to `packages\System.Data.SQLite.Core.1.0.118.0\lib\net40\`.

### Step 2: Copy the Native SQLite DLL

After restoring, copy the native interop DLL to your build output:

1. Find `SQLite.Interop.dll` in:
   ```
   packages\System.Data.SQLite.Core.1.0.118.0\build\net40\x86\SQLite.Interop.dll
   ```
2. Copy it to your build output folder:
   ```
   Domoto\bin\Debug\SQLite.Interop.dll
   ```

Without this file you'll get a "Unable to load DLL 'SQLite.Interop.dll'" error at runtime.

### Step 3: Replace the DatabaseService

Replace `Domoto/Services/DatabaseService.cs` with the SQLite version. The full SQLite implementation uses `DbProviderFactories` and stores data at:

```
%AppData%\TaskManager\taskmanager.db
```

Here's the SQLite DatabaseService to replace the in-memory one:

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using Domoto.Models;
using Domoto.Helpers;

namespace Domoto.Services
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        private readonly string _connectionString;
        private readonly DbProviderFactory _factory;

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
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TaskManager", "taskmanager.db");

            string dir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _connectionString = "Data Source=" + dbPath + ";Version=3;";
            _factory = DbProviderFactories.GetFactory("System.Data.SQLite");

            InitializeDatabase();
        }

        private DbConnection CreateConnection()
        {
            var conn = _factory.CreateConnection();
            conn.ConnectionString = _connectionString;
            return conn;
        }

        private void InitializeDatabase()
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL UNIQUE,
                            PasswordHash TEXT NOT NULL,
                            Role TEXT NOT NULL DEFAULT 'User'
                        );
                        CREATE TABLE IF NOT EXISTS Tasks (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            UserId INTEGER NOT NULL,
                            Title TEXT NOT NULL,
                            Description TEXT,
                            DueDate TEXT NOT NULL,
                            Priority TEXT NOT NULL DEFAULT 'Medium',
                            Category TEXT NOT NULL DEFAULT 'Work',
                            IsCompleted INTEGER NOT NULL DEFAULT 0,
                            CreatedDate TEXT NOT NULL,
                            FOREIGN KEY (UserId) REFERENCES Users(Id)
                        );";
                    cmd.ExecuteNonQuery();
                }

                // Seed default admin if none exists
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Role='Admin'";
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        cmd.CommandText = "INSERT INTO Users (Username, PasswordHash, Role) "
                            + "VALUES (@u, @p, 'Admin')";
                        var pUser = cmd.CreateParameter();
                        pUser.ParameterName = "@u";
                        pUser.Value = "admin";
                        cmd.Parameters.Add(pUser);
                        var pPass = cmd.CreateParameter();
                        pPass.ParameterName = "@p";
                        pPass.Value = PasswordHelper.HashPassword("admin123");
                        cmd.Parameters.Add(pPass);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // ... (rest of CRUD methods — see git history for full implementation)
    }
}
```

On first launch, the database file and tables are created automatically. A default admin account (`admin` / `admin123`) is seeded if no admin exists.

### Step 4: Verify app.config

The `Domoto/app.config` file must register the SQLite provider. This is already configured:

```xml
<configuration>
  <system.data>
    <DbProviderFactories>
      <remove invariant="System.Data.SQLite" />
      <add name="SQLite Data Provider"
           invariant="System.Data.SQLite"
           description=".NET Framework Data Provider for SQLite"
           type="System.Data.SQLite.SQLiteFactory, System.Data.SQLite" />
    </DbProviderFactories>
  </system.data>
</configuration>
```

### Troubleshooting SQLite

| Error | Fix |
|-------|-----|
| "Failed to find or load the registered .Net Framework Data Provider" | NuGet package not restored. Run `nuget restore Domoto.sln` |
| "Unable to load DLL 'SQLite.Interop.dll'" | Copy `SQLite.Interop.dll` from the NuGet package to `bin\Debug\` |
| "database is locked" | Close any other app or DB browser that has the file open |

### Database Location

The SQLite database is stored at:
```
C:\Users\<YourName>\AppData\Roaming\TaskManager\taskmanager.db
```

To reset the database, delete this file. It will be recreated on next launch.

## Project Structure

```
Domoto.sln
Domoto/
  App.xaml / App.xaml.cs          — App entry point, resource dictionaries
  Window1.xaml / Window1.xaml.cs  — Main window shell (custom title bar + sidebar + content)
  Helpers/
    BaseViewModel.cs              — INotifyPropertyChanged base class
    RelayCommand.cs               — ICommand implementation
    PasswordHelper.cs             — SHA-256 password hashing
    BoolToStrikethroughConverter.cs
    FirstLetterConverter.cs       — Extracts first letter for avatar circles
  Models/
    TaskItem.cs                   — Task data model (with IsOverdue, IsDueSoon)
    User.cs                       — User data model (Id, Username, PasswordHash, Role)
  Services/
    DatabaseService.cs            — Data access (in-memory or SQLite)
    SessionService.cs             — Current logged-in user state
    ExportService.cs              — CSV export
  ViewModels/
    LoginViewModel.cs             — Login/registration logic
    DashboardViewModel.cs         — Dashboard stats and daily tasks
    SidebarViewModel.cs           — Navigation, search, user info
    TaskViewModel.cs              — Task CRUD, filtering, sorting, profile
    AdminViewModel.cs             — Admin user management
  Views/
    LoginView.xaml                — Sign in / register screen
    DashboardView.xaml            — Welcome banner, stats, projects, daily tasks
    SidebarView.xaml              — Persistent navigation sidebar
    TaskView.xaml                 — Task list with filter bar and form panel
    ProfileView.xaml              — Username and password update
    AdminView.xaml                — User management (admin only)
  Themes/
    NeobrutalistTheme.xaml        — Global neobrutalism styles and colors
Domoto.Tests/                     — Property-based test project (FsCheck + NUnit)
```

## Features

- **Authentication** — Login and registration with SHA-256 hashed passwords
- **Dashboard** — Welcome banner, task stats (in progress, completed, overdue), project cards, daily tasks
- **Task Management** — Create, edit, delete tasks with title, description, due date, priority, and category
- **Filtering & Sorting** — Search by text, filter by priority/category/status, sort by multiple fields
- **Completion Tracking** — Toggle task completion with strikethrough styling
- **Due Date Alerts** — Notification for tasks due within 24 hours
- **Profile Management** — Update username and password
- **Admin Panel** — Create and delete user accounts (admin role only)
- **CSV Export** — Export tasks to CSV with proper escaping
- **Responsive Layout** — Form panel collapses below 1000px width, cards reflow on resize
- **Custom Window Chrome** — No default Windows frame; custom title bar with minimize, maximize, close
- **Neobrutalism Theme** — Bold borders, vivid accents, hard shadows, clean typography

## Architecture

The app follows the **MVVM** (Model-View-ViewModel) pattern:

- **Views** contain only XAML layout and minimal code-behind for UI events (password boxes, navigation)
- **ViewModels** contain all business logic, expose data via `INotifyPropertyChanged`, and use `RelayCommand` for actions
- **Models** are plain data classes
- **Services** handle data access and session state

Navigation is managed by `Window1.xaml.cs` which swaps the `ContentControl` content based on sidebar events, keeping the sidebar persistent across all post-login views.

## Test Project

The `Domoto.Tests` project uses **FsCheck 2.16.6** and **NUnit 3.13.3** for property-based testing. It targets .NET Framework 4.5.2 (higher than the main project to support FsCheck).

To restore test packages:
```
nuget restore Domoto.sln
```

## License

This project is for educational and personal use.
