# Domoto — Task Manager

Domoto is a WPF desktop task manager built with C# and MVVM. It supports authentication, task CRUD, filtering, and CSV export with a neobrutalism UI theme.

## Features
- Login and role-based access (Admin/User)
- Task creation, editing, deletion, and completion
- Filtering and sorting by priority, status, category, due date
- Due date notifications
- CSV export

## Tech Stack
- .NET Framework 4.8
- WPF (MVVM)
- MySQL (XAMPP recommended)
- MySql.Data (NuGet)

## Prerequisites
- Windows 7+
- .NET Framework 4.8
- Visual Studio 2010+ (or newer)
- MySQL (XAMPP recommended)

## MySQL Setup (Required)
1. Start MySQL (XAMPP Control Panel).
2. Create the database and user:
   ```sql
   CREATE DATABASE IF NOT EXISTS taskmanager;
   CREATE USER 'taskmanager_user'@'localhost' IDENTIFIED BY 'manager';
   GRANT ALL PRIVILEGES ON taskmanager.* TO 'taskmanager_user'@'localhost';
   FLUSH PRIVILEGES;
   ```
3. Configure `Domoto/app.config`:
   ```xml
   <appSettings>
     <add key="MySql.ServerConnectionString" value="Server=localhost;Port=3306;Uid=taskmanager_user;Pwd=manager;SslMode=none;" />
     <add key="MySql.DatabaseName" value="taskmanager" />
   </appSettings>
   ```

## Build & Run
1. Restore packages:
   ```
   nuget restore Domoto.sln
   ```
2. Build (F6) and run (F5).

## Default Accounts
| Username | Password   | Role  | Notes                        |
|----------|------------|-------|------------------------------|
| admin    | admin123   | Admin | Sees all tasks, admin panel  |
| user     | user123    | User  | Sees only their own tasks    |

## Database Behavior
- Tables are created automatically on first launch.
- Seeded accounts are created only if the `Users` table is empty.
- To reset, drop the `taskmanager` database and relaunch the app.

## Troubleshooting
- `MySqlConnection` not found: restore packages and ensure `MySql.Data.dll` is referenced.
- Tables not created: verify MySQL is running and credentials in `app.config` are correct.