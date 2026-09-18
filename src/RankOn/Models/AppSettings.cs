namespace RankOn.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "System";
    public string Language { get; set; } = "ko-KR";
    public bool StartWithWindows { get; set; }
    public bool StartMinimizedToTray { get; set; }
    public bool CloseToTray { get; set; } = true;
    public bool AutoCheckUpdates { get; set; } = true;
    public bool PcOverlayEnabled { get; set; } = true;
    public bool BroadcastEnabled { get; set; } = true;
    public bool PcOverlayTopmost { get; set; } = true;
    public bool PcOverlayClickThrough { get; set; } = true;
    public bool PcOverlayLocked { get; set; } = true;
    public double PcOverlayScale { get; set; } = 1.0;
    public double PcOverlayOpacity { get; set; } = 1.0;
    public double? PcOverlayX { get; set; }
    public double? PcOverlayY { get; set; }
    public string OverlayHotkey { get; set; } = "Ctrl+Shift+R";
    public int RankRefreshSeconds { get; set; } = 60;
    public int? SessionStartRp { get; set; }
    public DateTimeOffset? SessionStartedAt { get; set; }
    public string TargetRpDisplayMode { get; set; } = "Auto";
    public string OverlayPreset { get; set; } = "Standard";
    public bool OverlayBackgroundEnabled { get; set; } = true;
    public double OverlayBackgroundOpacity { get; set; } = 0.88;
    public double OverlayCornerRadius { get; set; } = 14;
    public double OverlayFontScale { get; set; } = 1.0;
    public bool OverlayShowNickname { get; set; } = true;
    public bool OverlayShowTier { get; set; } = true;
    public bool OverlayShowRp { get; set; } = true;
    public bool OverlayShowRank { get; set; } = true;
    public bool OverlayShowSession { get; set; } = true;
    public bool OverlayShowSeason { get; set; } = true;
    public bool OverlayShowTarget { get; set; } = true;
}
