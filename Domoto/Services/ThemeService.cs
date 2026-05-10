using System;
using System.Windows;

namespace Domoto.Services
{
    /// <summary>
    /// Represents available application themes.
    /// </summary>
    public enum AppTheme
    {
        Light,
        Dark
    }

    /// <summary>
    /// Lightweight theme service used by the UI to track and toggle the current theme.
    /// Only manages an in-memory theme state to satisfy existing callers in the project.
    /// If you have theme resource dictionaries, extend `Apply` to swap resource dictionaries
    /// on `Application.Current.Resources`.
    /// </summary>
    public static class ThemeService
    {
        private const string LightPath = "Themes/PaletteLight.xaml";
        private const string DarkPath = "Themes/PaletteDark.xaml";

        private static AppTheme _current = AppTheme.Light;

        public static AppTheme Current
        {
            get { return _current; }
        }

        public static event Action<AppTheme> ThemeChanged;

        public static void Apply(AppTheme theme)
        {
            var app = Application.Current;
            if (app == null) return;

            string targetPath = theme == AppTheme.Dark ? DarkPath : LightPath;
            var newDict = new ResourceDictionary
            {
                Source = new Uri(targetPath, UriKind.Relative)
            };

            var merged = app.Resources.MergedDictionaries;

            // Remove any previously-applied palette dictionary
            for (int i = merged.Count - 1; i >= 0; i--)
            {
                var src = merged[i].Source;
                if (src != null)
                {
                    string s = src.OriginalString;
                    if (s.EndsWith("PaletteLight.xaml", StringComparison.OrdinalIgnoreCase) ||
                        s.EndsWith("PaletteDark.xaml", StringComparison.OrdinalIgnoreCase))
                    {
                        merged.RemoveAt(i);
                    }
                }
            }

            // Append so palette wins over base theme
            merged.Add(newDict);
            _current = theme;

            var handler = ThemeChanged;
            if (handler != null) handler(theme);
        }

        public static void Toggle()
        {
            Apply(_current == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
        }
    }
}
