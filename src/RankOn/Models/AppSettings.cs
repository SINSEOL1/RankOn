namespace RankOn.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "System";
    public string Language { get; set; } = "ko-KR";
    public bool StartWithWindows { get; set; }
    public bool StartMinimizedToTray { get; set; }
    public bool CloseToTray { get; set; } = true;
    public bool PcOverlayEnabled { get; set; } = true;
    public bool BroadcastEnabled { get; set; } = true;
    public bool PcOverlayTopmost { get; set; } = true;
    public bool PcOverlayClickThrough { get; set; } = true;
    public double PcOverlayScale { get; set; } = 1.0;
    public double PcOverlayOpacity { get; set; } = 1.0;
    public string OverlayHotkey { get; set; } = "Ctrl+Shift+R";
    public int RankRefreshSeconds { get; set; } = 60;
    public int? SessionStartRp { get; set; }
    public DateTimeOffset? SessionStartedAt { get; set; }
}
