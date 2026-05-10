# Notification System Implementation

## What's Been Done

✅ Created `Domoto/Services/NotificationService.cs` with:
- Background timer that checks every 15 minutes for due-soon tasks
- HashSet tracking to prevent duplicate notifications
- Balloon notification display for tasks due within 24 hours
- Window restoration when balloon is clicked
- Proper disposal and reset on logout

✅ Updated `Domoto/App.xaml.cs`:
- Initializes NotificationService on startup
- Disposes NotificationService on exit

✅ Updated `Domoto/ViewModels/TaskViewModel.cs`:
- Removed blocking MessageBox.Show call
- Left commented code for optional "summary on open" feature

✅ Updated `Domoto/Services/SessionService.cs`:
- Resets notification tracking on logout

✅ Updated `Domoto/packages.config`:
- Added Hardcodet.NotifyIcon.Wpf package reference

✅ Updated `Domoto/Domoto.csproj`:
- Added NotificationService.cs to compilation
- Added Hardcodet.Wpf.TaskbarNotification.dll reference
- Added Assets/tray.ico as a resource

## What Still Needs to Be Done

### 1. Download the Hardcodet.NotifyIcon.Wpf NuGet Package

In Visual Studio:
- Right-click on the solution → "Restore NuGet Packages"
- Or manually download from: https://www.nuget.org/packages/Hardcodet.NotifyIcon.Wpf/1.1.0

### 2. Create the Tray Icon

Create a folder `Domoto/Assets/` and add a file `tray.ico`:
- Use a 16x16 or 32x32 icon file
- You can create one online at https://www.favicon-generator.org/
- Or use any existing .ico file
- Place it at `Domoto/Assets/tray.ico`

### 3. Add Context Menu (Optional Enhancement)

In `NotificationService.cs`, uncomment and implement the context menu:

```csharp
private ContextMenu CreateContextMenu()
{
    var menu = new ContextMenu();
    
    var openItem = new MenuItem { Header = "Open Domoto" };
    openItem.Click += (s, e) => {
        var mainWindow = Application.Current.MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }
    };
    
    var exitItem = new MenuItem { Header = "Exit" };
    exitItem.Click += (s, e) => Application.Current.Shutdown();
    
    menu.Items.Add(openItem);
    menu.Items.Add(new Separator());
    menu.Items.Add(exitItem);
    
    return menu;
}
```

Then in the constructor, uncomment:
```csharp
_tray.ContextMenu = CreateContextMenu();
```

### 4. Navigate to Task View on Balloon Click (Optional Enhancement)

To navigate to Task_View when the balloon is clicked, you'll need to add navigation logic in `OnBalloonClicked`. This requires access to Window1's navigation system.

## Testing

1. Build and run the application in Visual Studio
2. Log in as a user
3. Create a task with due date 1 hour from now
4. Wait 15 minutes (or temporarily change CHECK_INTERVAL_MINUTES to 1 for testing)
5. A balloon notification should appear in the system tray
6. Click the balloon - the main window should restore and focus
7. Create another due-soon task and wait - only the new task should be notified

## Files Modified

- `Domoto/Services/NotificationService.cs` (NEW)
- `Domoto/App.xaml.cs`
- `Domoto/ViewModels/TaskViewModel.cs`
- `Domoto/Services/SessionService.cs`
- `Domoto/packages.config`
- `Domoto/Domoto.csproj`

## Notes

- The notification service runs in the background even when the app is minimized
- Each task is only notified once per session
- Notifications reset when the user logs out
- The blocking MessageBox has been removed to prevent UI freezing
