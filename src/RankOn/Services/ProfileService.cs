using System.IO;
using System.Text.Json;
using RankOn.Models;

namespace RankOn.Services;

public sealed class ProfileService
{
    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RankOn");

    private string ProfilePath => Path.Combine(_directory, "profile.json");

    public async Task<PlayerProfile?> LoadAsync()
    {
        if (!File.Exists(ProfilePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(ProfilePath);
            var profile = JsonSerializer.Deserialize<PlayerProfile>(json);

            if (profile is null ||
                string.IsNullOrWhiteSpace(profile.Nickname) ||
                string.IsNullOrWhiteSpace(profile.Uid))
            {
                return null;
            }

            return profile;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveAsync(PlayerProfile profile)
    {
        Directory.CreateDirectory(_directory);

        var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(ProfilePath, json);
    }
}
