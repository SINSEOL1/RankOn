using System.Windows;
using System.Windows.Controls;

namespace RankOn.Services;

public sealed class LocalizationService
{
    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "ko-KR", "en-US", "ja-JP", "zh-CN"
    };

    private static readonly Dictionary<string, string[]> Strings = new()
    {
        ["홈"] = ["Home", "ホーム", "主页"],
        ["방송 출력"] = ["Broadcast", "配信出力", "直播输出"],
        ["오버레이"] = ["Overlay", "オーバーレイ", "悬浮层"],
        ["디자인"] = ["Design", "デザイン", "设计"],
        ["프로필"] = ["Profile", "プロフィール", "资料"],
        ["설정"] = ["Settings", "設定", "设置"],
        ["정보"] = ["About", "情報", "关于"],
        ["프로필 미등록"] = ["No profile", "プロフィール未登録", "未注册资料"],
        ["닉네임을 등록해 시작하세요"] = ["Register a nickname to begin", "ニックネームを登録してください", "注册昵称后开始"],
        ["이번 세션"] = ["This session", "今回のセッション", "本次会话"],
        ["새 세션"] = ["New session", "新しいセッション", "新会话"],
        ["직접 입력"] = ["Manual input", "手動入力", "手动输入"],
        ["PC 오버레이"] = ["PC Overlay", "PCオーバーレイ", "PC 悬浮层"],
        ["새로고침"] = ["Refresh", "更新", "刷新"],
        ["불러오는 중..."] = ["Loading...", "読み込み中...", "加载中..."],
        ["자동 갱신 60초"] = ["Auto refresh: 60 sec", "自動更新: 60秒", "自动刷新：60秒"],
        ["시즌 종료"] = ["Season ended", "シーズン終了", "赛季已结束"],
        ["OBS Browser Source"] = ["OBS Browser Source", "OBSブラウザソース", "OBS 浏览器源"],
        ["방송 출력 실행 중"] = ["Broadcast output running", "配信出力 実行中", "直播输出运行中"],
        ["방송 출력 꺼짐"] = ["Broadcast output off", "配信出力 オフ", "直播输出已关闭"],
        ["출력 끄기"] = ["Turn off", "出力をオフ", "关闭输出"],
        ["출력 켜기"] = ["Turn on", "出力をオン", "开启输出"],
        ["주소"] = ["Address", "アドレス", "地址"],
        ["복사"] = ["Copy", "コピー", "复制"],
        ["미리보기"] = ["Preview", "プレビュー", "预览"],
        ["권장 크기 600 × 180 · 투명 배경"] = ["Recommended: 600 × 180 · Transparent background", "推奨サイズ 600 × 180 · 透明背景", "推荐尺寸 600 × 180 · 透明背景"],
        ["OBS 등록 방법"] = ["OBS setup", "OBS設定方法", "OBS 设置方法"],
        ["1. OBS에서 브라우저 소스를 추가합니다."] = ["1. Add a Browser Source in OBS.", "1. OBSでブラウザソースを追加します。", "1. 在 OBS 中添加浏览器源。"],
        ["2. 위 주소를 URL에 입력합니다."] = ["2. Paste the address above into URL.", "2. 上のアドレスをURLに入力します。", "2. 将上面的地址填入 URL。"],
        ["3. 너비 600, 높이 180으로 설정합니다."] = ["3. Set width to 600 and height to 180.", "3. 幅600、高さ180に設定します。", "3. 设置宽度 600、高度 180。"],
        ["PC 오버레이와 방송 출력은 서로 독립적으로 켜고 끌 수 있습니다."] = ["PC overlay and broadcast output can be toggled independently.", "PCオーバーレイと配信出力は個別に切り替えられます。", "PC 悬浮层与直播输出可独立开关。"],
        ["방송 출력과 독립적으로 Windows 화면에 표시됩니다."] = ["Displays on Windows independently from broadcast output.", "配信出力とは独立してWindows画面に表示されます。", "独立于直播输出显示在 Windows 桌面。"],
        ["사용"] = ["Enable", "使用", "启用"],
        ["위치 조정"] = ["Adjust position", "位置調整", "调整位置"],
        ["위치 조정 완료"] = ["Finish positioning", "位置調整完了", "完成位置调整"],
        ["위치 초기화"] = ["Reset position", "位置をリセット", "重置位置"],
        ["기본 위치"] = ["Default position", "デフォルト位置", "默认位置"],
        ["표시 설정"] = ["Display settings", "表示設定", "显示设置"],
        ["크기"] = ["Size", "サイズ", "大小"],
        ["투명도"] = ["Opacity", "不透明度", "透明度"],
        ["항상 위"] = ["Always on top", "常に手前に表示", "始终置顶"],
        ["클릭 통과"] = ["Click-through", "クリック透過", "点击穿透"],
        ["오버레이 잠금"] = ["Lock overlay", "オーバーレイをロック", "锁定悬浮层"],
        ["표시/숨기기 단축키  Ctrl + Shift + R"] = ["Show/hide hotkey  Ctrl + Shift + R", "表示/非表示  Ctrl + Shift + R", "显示/隐藏快捷键  Ctrl + Shift + R"],
        ["작업표시줄과 Alt+Tab에는 일반 창처럼 표시됩니다."] = ["Appears in the taskbar and Alt+Tab like a normal window.", "タスクバーとAlt+Tabには通常のウィンドウとして表示されます。", "会像普通窗口一样显示在任务栏和 Alt+Tab 中。"],
        ["프리셋"] = ["Presets", "プリセット", "预设"],
        ["표시 항목"] = ["Visible items", "表示項目", "显示项目"],
        ["목표 RP 표시"] = ["Target RP", "目標RP表示", "目标 RP"],
        ["자동"] = ["Auto", "自動", "自动"],
        ["다음 티어"] = ["Next tier", "次のティア", "下一段位"],
        ["데미갓"] = ["Demigod", "デミゴッド", "半神"],
        ["이터니티"] = ["Eternity", "エタニティ", "永恒"],
        ["숨김"] = ["Hidden", "非表示", "隐藏"],
        ["닉네임"] = ["Nickname", "ニックネーム", "昵称"],
        ["티어"] = ["Tier", "ティア", "段位"],
        ["순위"] = ["Rank", "順位", "排名"],
        ["세션 RP"] = ["Session RP", "セッションRP", "会话 RP"],
        ["시즌 남은 시간"] = ["Season time remaining", "シーズン残り時間", "赛季剩余时间"],
        ["목표 RP"] = ["Target RP", "目標RP", "目标 RP"],
        ["스타일"] = ["Style", "スタイル", "样式"],
        ["배경 표시"] = ["Show background", "背景を表示", "显示背景"],
        ["배경 투명도"] = ["Background opacity", "背景の不透明度", "背景透明度"],
        ["모서리 둥글기"] = ["Corner radius", "角丸", "圆角"],
        ["글자/요소 크기"] = ["Text / element scale", "文字/要素サイズ", "文字/元素大小"],
        ["실시간 미리보기"] = ["Live preview", "リアルタイムプレビュー", "实时预览"],
        ["설정은 PC 오버레이와 OBS 방송 출력에 함께 적용됩니다."] = ["Settings apply to both PC and OBS overlays.", "設定はPCとOBSの両方に適用されます。", "设置会同时应用于 PC 和 OBS 悬浮层。"],
        ["이터널 리턴 프로필"] = ["Eternal Return Profile", "Eternal Return プロフィール", "Eternal Return 资料"],
        ["현재 인게임 닉네임을 입력하면 랭크온이 UID를 저장하고 이후 60초마다 랭크를 자동 갱신합니다."] = ["Enter your in-game nickname. RankOn stores the UID and refreshes rank data every 60 seconds.", "ゲーム内ニックネームを入力するとUIDを保存し、60秒ごとにランクを更新します。", "输入游戏昵称后，RankOn 会保存 UID，并每 60 秒刷新排名。"],
        ["등록"] = ["Register", "登録", "注册"],
        ["현재 프로필"] = ["Current profile", "現在のプロフィール", "当前资料"],
        ["등록된 프로필 없음"] = ["No registered profile", "登録済みプロフィールなし", "没有已注册资料"],
        ["일반"] = ["General", "一般", "常规"],
        ["Windows 시작 시 실행"] = ["Run at Windows startup", "Windows起動時に実行", "Windows 启动时运行"],
        ["실행 시 트레이에서 시작"] = ["Start minimized to tray", "トレイで起動", "启动时最小化到托盘"],
        ["X 버튼을 누르면 트레이로 이동"] = ["Close button minimizes to tray", "閉じるボタンでトレイへ", "关闭按钮最小化到托盘"],
        ["새 버전 자동 확인"] = ["Check for updates automatically", "更新を自動確認", "自动检查更新"],
        ["테마"] = ["Theme", "テーマ", "主题"],
        ["시스템"] = ["System", "システム", "系统"],
        ["언어"] = ["Language", "言語", "语言"],
        ["언어 설정은 저장되며 다음 실행부터 적용됩니다."] = ["Language selection is saved.", "言語設定は保存されます。", "语言设置会被保存。"],
        ["데이터"] = ["Data", "データ", "数据"],
        ["자동 랭크 갱신 60초 · 수동 새로고침 쿨다운 10초"] = ["Rank refresh: 60 sec · Manual cooldown: 10 sec", "自動更新60秒 · 手動更新クールダウン10秒", "自动刷新 60 秒 · 手动刷新冷却 10 秒"],
        ["이터널 리턴을 위한 비공식 랭크 오버레이 프로그램입니다."] = ["An unofficial rank overlay for Eternal Return.", "Eternal Return向けの非公式ランクオーバーレイです。", "Eternal Return 非官方排名悬浮层工具。"],
        ["개발자 문의"] = ["Developer contact", "開発者連絡先", "开发者联系"],
        ["업데이트 확인"] = ["Check for updates", "更新を確認", "检查更新"],
        ["GitHub 열기"] = ["Open GitHub", "GitHubを開く", "打开 GitHub"],
        ["세션 시작 RP"] = ["Session start RP", "セッション開始RP", "会话起始 RP"],
        ["세션 시작 RP 설정"] = ["Set session start RP", "セッション開始RP設定", "设置会话起始 RP"],
        ["시작 RP"] = ["Starting RP", "開始RP", "起始 RP"],
        ["취소"] = ["Cancel", "キャンセル", "取消"],
        ["적용"] = ["Apply", "適用", "应用"],
        ["위치 조정 중 · 드래그해서 이동"] = ["Positioning · Drag to move", "位置調整中 · ドラッグで移動", "正在调整位置 · 拖动移动"],
        ["프로필 페이지에서 닉네임을 등록하면 시작됩니다."] = ["Register your nickname on the Profile page to begin.", "プロフィールでニックネームを登録してください。", "请先在资料页面注册昵称。"],
        ["랭크 정보를 불러오는 중입니다."] = ["Loading rank data.", "ランク情報を読み込み中です。", "正在加载排名数据。"],
        ["순위 없음"] = ["Unranked", "順位なし", "暂无排名"],
        ["프로필을 등록해주세요."] = ["Please register a profile.", "プロフィールを登録してください。", "请注册资料。"],
        ["닉네임을 입력해주세요."] = ["Please enter a nickname.", "ニックネームを入力してください。", "请输入昵称。"],
        ["프로필을 확인하는 중입니다."] = ["Checking profile.", "プロフィールを確認中です。", "正在检查资料。"],
        ["프로필을 등록했습니다."] = ["Profile registered.", "プロフィールを登録しました。", "资料已注册。"],
        ["입력한 닉네임을 찾지 못했습니다."] = ["Nickname not found.", "ニックネームが見つかりません。", "未找到该昵称。"],
        ["프로필을 불러오지 못했습니다. 잠시 후 다시 시도해주세요."] = ["Could not load the profile. Try again shortly.", "プロフィールを読み込めませんでした。しばらくしてから再試行してください。", "无法加载资料，请稍后重试。"],
        ["현재 최신 버전입니다."] = ["You are up to date.", "最新バージョンです。", "当前已是最新版本。"],
        ["업데이트를 확인하는 중입니다."] = ["Checking for updates.", "更新を確認中です。", "正在检查更新。"],
        ["업데이트 정보를 확인하지 못했습니다."] = ["Could not check for updates.", "更新情報を確認できませんでした。", "无法检查更新信息。"]
    };

    private static readonly Dictionary<string, string[]> Tiers = new()
    {
        ["iron"] = ["아이언", "Iron", "アイアン", "铁"],
        ["bronze"] = ["브론즈", "Bronze", "ブロンズ", "青铜"],
        ["silver"] = ["실버", "Silver", "シルバー", "白银"],
        ["gold"] = ["골드", "Gold", "ゴールド", "黄金"],
        ["platinum"] = ["플래티넘", "Platinum", "プラチナ", "铂金"],
        ["diamond"] = ["다이아몬드", "Diamond", "ダイヤモンド", "钻石"],
        ["meteorite"] = ["메테오라이트", "Meteorite", "メテオライト", "陨石"],
        ["mythril"] = ["미스릴", "Mythril", "ミスリル", "秘银"],
        ["demigod"] = ["데미갓", "Demigod", "デミゴッド", "半神"],
        ["eternity"] = ["이터니티", "Eternity", "エタニティ", "永恒"]
    };

    public string Current { get; private set; } = "ko-KR";

    public void Apply(string language)
    {
        Current = Supported.Contains(language) ? language : "ko-KR";
    }

    public string T(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        foreach (var pair in Strings)
        {
            if (string.Equals(pair.Key, value, StringComparison.Ordinal) ||
                pair.Value.Any(x => string.Equals(x, value, StringComparison.Ordinal)))
            {
                return Get(pair.Key, pair.Value);
            }
        }

        return value;
    }

    public string Tier(string tierKey, int? division)
    {
        if (!Tiers.TryGetValue(tierKey, out var names))
        {
            return tierKey;
        }

        var name = Current switch
        {
            "en-US" => names[1],
            "ja-JP" => names[2],
            "zh-CN" => names[3],
            _ => names[0]
        };

        return division is > 0 ? $"{name} {division}" : name;
    }

    public string SeasonRemaining(DateTimeOffset? seasonEnd)
    {
        if (seasonEnd is null) return "";

        var remaining = seasonEnd.Value - DateTimeOffset.Now;
        if (remaining <= TimeSpan.Zero) return T("시즌 종료");

        var days = (int)remaining.TotalDays;

        return Current switch
        {
            "en-US" => days >= 1
                ? $"Season ends in {days}d {remaining.Hours}h"
                : $"Season ends in {remaining.Hours}h {remaining.Minutes}m",
            "ja-JP" => days >= 1
                ? $"シーズン終了まで {days}日 {remaining.Hours}時間"
                : $"シーズン終了まで {remaining.Hours}時間 {remaining.Minutes}分",
            "zh-CN" => days >= 1
                ? $"赛季结束还有 {days}天 {remaining.Hours}小时"
                : $"赛季结束还有 {remaining.Hours}小时 {remaining.Minutes}分钟",
            _ => days >= 1
                ? $"시즌 종료까지 {days}일 {remaining.Hours}시간"
                : $"시즌 종료까지 {remaining.Hours}시간 {remaining.Minutes}분"
        };
    }

    public string Target(string koreanText)
    {
        if (Current == "ko-KR" || string.IsNullOrWhiteSpace(koreanText)) return koreanText;

        var marker = "까지 ";
        var index = koreanText.IndexOf(marker, StringComparison.Ordinal);
        if (index <= 0) return koreanText;

        var nameKo = koreanText[..index];
        var remainder = koreanText[(index + marker.Length)..];
        var tierKey = Tiers.FirstOrDefault(x => x.Value[0] == nameKo).Key;
        if (string.IsNullOrWhiteSpace(tierKey)) return koreanText;

        var name = Tier(tierKey, null);

        return Current switch
        {
            "en-US" => $"{name}: {remainder}",
            "ja-JP" => $"{name}まで {remainder}",
            "zh-CN" => $"距离{name}还需 {remainder}",
            _ => koreanText
        };
    }

    public void ApplyTo(DependencyObject root)
    {
        TranslateElement(root);

        foreach (var child in LogicalTreeHelper.GetChildren(root))
        {
            if (child is DependencyObject dependencyObject)
            {
                ApplyTo(dependencyObject);
            }
        }
    }

    private void TranslateElement(DependencyObject element)
    {
        if (element is Window window)
        {
            window.Title = T(window.Title);
        }

        if (element is TextBlock textBlock)
        {
            textBlock.Text = T(textBlock.Text);
        }

        if (element is ContentControl contentControl && contentControl.Content is string content)
        {
            contentControl.Content = T(content);
        }

        if (element is HeaderedContentControl headered && headered.Header is string header)
        {
            headered.Header = T(header);
        }
    }

    private string Get(string korean, string[] values)
    {
        return Current switch
        {
            "en-US" => values[0],
            "ja-JP" => values[1],
            "zh-CN" => values[2],
            _ => korean
        };
    }
}
