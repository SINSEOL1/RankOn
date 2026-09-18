using System.IO;
using System.Net.Http;
using System.Windows.Resources;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RankOn.Services;

public static class TierIconService
{
    private const string SpriteUrl = "https://support.playeternalreturn.com/hc/article_attachments/45305347324825";
    private static readonly HttpClient Client = CreateClient();
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly Dictionary<string, ImageSource> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static byte[]? _spriteBytes;
    private static BitmapSource? _sprite;

    private static readonly Dictionary<string, int> TierIndexes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["iron"] = 0,
        ["bronze"] = 1,
        ["silver"] = 2,
        ["gold"] = 3,
        ["platinum"] = 4,
        ["diamond"] = 5,
        ["meteorite"] = 6,
        ["mythril"] = 7,
        ["demigod"] = 8,
        ["eternity"] = 9
    };

    public static async Task WarmupAsync()
    {
        try
        {
            await EnsureSpriteAsync();
        }
        catch
        {
        }
    }

    public static async Task<byte[]?> GetSpriteBytesAsync()
    {
        try
        {
            await EnsureSpriteAsync();
            return _spriteBytes;
        }
        catch
        {
            return null;
        }
    }

    public static ImageSource? GetImage(string tierKey)
    {
        if (Cache.TryGetValue(tierKey, out var cached))
        {
            return cached;
        }

        if (!TierIndexes.TryGetValue(tierKey, out var index))
        {
            return null;
        }

        try
        {
            EnsureSpriteAsync().GetAwaiter().GetResult();

            if (_sprite is null)
            {
                return null;
            }

            var columnWidth = Math.Max(1, _sprite.PixelWidth / 10);
            var cropSize = Math.Min(
                Math.Max(1, (int)Math.Round(columnWidth * 0.84)),
                _sprite.PixelHeight);
            var x = Math.Clamp(
                index * columnWidth + (columnWidth - cropSize) / 2,
                0,
                Math.Max(0, _sprite.PixelWidth - cropSize));
            var y = Math.Clamp(
                (_sprite.PixelHeight - cropSize) / 2,
                0,
                Math.Max(0, _sprite.PixelHeight - cropSize));

            var cropped = new CroppedBitmap(
                _sprite,
                new Int32Rect(x, y, cropSize, cropSize));
            cropped.Freeze();
            Cache[tierKey] = cropped;
            return cropped;
        }
        catch
        {
            return null;
        }
    }

    private static async Task EnsureSpriteAsync()
    {
        if (_sprite is not null && _spriteBytes is not null)
        {
            return;
        }

        await Gate.WaitAsync();

        try
        {
            if (_sprite is not null && _spriteBytes is not null)
            {
                return;
            }

            var cachePath = GetCachePath();
            var bytes = TryReadBundledSprite();

            if (bytes is null && File.Exists(cachePath))
            {
                try
                {
                    bytes = await File.ReadAllBytesAsync(cachePath);
                }
                catch
                {
                }
            }

            if (bytes is null || bytes.Length == 0)
            {
                bytes = await Client.GetByteArrayAsync(SpriteUrl);

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
                    await File.WriteAllBytesAsync(cachePath, bytes);
                }
                catch
                {
                }
            }

            using var stream = new MemoryStream(bytes, false);
            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);
            var frame = decoder.Frames[0];
            frame.Freeze();

            _spriteBytes = bytes;
            _sprite = frame;
        }
        finally
        {
            Gate.Release();
        }
    }

    private static byte[]? TryReadBundledSprite()
    {
        try
        {
            StreamResourceInfo? resource = System.Windows.Application.GetResourceStream(
                new Uri("pack://application:,,,/Resources/rank-tiers.png", UriKind.Absolute));

            if (resource?.Stream is null)
            {
                return null;
            }

            using var stream = resource.Stream;
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static string GetCachePath()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RankOn",
            "cache");

        return Path.Combine(root, "rank-tiers.png");
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("RankOn/1.0");
        return client;
    }
}
