using System.Windows;

namespace RankOn.Services;

public sealed class LocalizationService
{
    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "ko-KR", "en-US", "ja-JP", "zh-CN"
    };

    public string Current { get; private set; } = "ko-KR";

    public void Apply(string language)
    {
        if (!Supported.Contains(language))
        {
            language = "ko-KR";
        }

        var resources = System.Windows.Application.Current.Resources.MergedDictionaries;
        var current = resources.FirstOrDefault(x =>
            x.Source?.OriginalString.Contains("Localization/Strings.", StringComparison.OrdinalIgnoreCase) == true);

        if (current is not null)
        {
            resources.Remove(current);
        }

        resources.Add(new ResourceDictionary
        {
            Source = new Uri($"Localization/Strings.{language}.xaml", UriKind.Relative)
        });

        Current = language;
    }

    public string T(string key)
    {
        return System.Windows.Application.Current.TryFindResource(key)?.ToString() ?? key;
    }
}
