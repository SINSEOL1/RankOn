using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RankOn.Services;

public static class TierIconService
{
    private static readonly Dictionary<string, string> Urls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["iron"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Iron.png",
        ["bronze"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Bronze.png",
        ["silver"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Silver.png",
        ["gold"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Gold.png",
        ["platinum"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Platinum.png",
        ["diamond"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Diamond.png",
        ["meteorite"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Meteorite.png",
        ["mythril"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Mythril.png",
        ["demigod"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Titan.png",
        ["eternity"] = "https://eternalreturn.fandom.com/wiki/Special:Redirect/file/RankedTier_Immortal.png"
    };

    private static readonly Dictionary<string, ImageSource> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static string GetUrl(string tierKey)
    {
        return Urls.TryGetValue(tierKey, out var url) ? url : "";
    }

    public static ImageSource? GetImage(string tierKey)
    {
        if (Cache.TryGetValue(tierKey, out var cached))
        {
            return cached;
        }

        var url = GetUrl(tierKey);
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(url, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.EndInit();
            image.Freeze();
            Cache[tierKey] = image;
            return image;
        }
        catch
        {
            return null;
        }
    }
}
