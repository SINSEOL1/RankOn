using System.Windows;

namespace RankOn.Services;

public sealed class ThemeService
{
    public string Current { get; private set; } = "Dark";

    public void Apply(string theme)
    {
        var resolved = string.Equals(theme, "System", StringComparison.OrdinalIgnoreCase)
            ? (IsSystemDarkMode() ? "Dark" : "Light")
            : theme;

        if (!string.Equals(resolved, "Light", StringComparison.OrdinalIgnoreCase))
        {
            resolved = "Dark";
        }

        var resources = System.Windows.Application.Current.Resources.MergedDictionaries;
        var current = resources.FirstOrDefault(x =>
            x.Source?.OriginalString.Contains("Themes/Theme.", StringComparison.OrdinalIgnoreCase) == true);

        if (current is not null)
        {
            resources.Remove(current);
        }

        resources.Insert(0, new ResourceDictionary
        {
            Source = new Uri($"Themes/Theme.{resolved}.xaml", UriKind.Relative)
        });

        Current = resolved;
    }

    private static bool IsSystemDarkMode()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            return key?.GetValue("AppsUseLightTheme") is int mode && mode == 0;
        }
        catch
        {
            return true;
        }
    }
}
