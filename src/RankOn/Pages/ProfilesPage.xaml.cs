using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RankOn.Pages;

public partial class ProfilesPage : UserControl
{
    public ProfilesPage()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        App.LocalizationService.ApplyTo(this);
        UpdateCurrentProfile();
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await RegisterAsync();
    }

    private async void NicknameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await RegisterAsync();
        }
    }

    private async Task RegisterAsync()
    {
        var nickname = NicknameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(nickname))
        {
            RegistrationStatusText.Text = App.LocalizationService.T("닉네임을 입력해주세요.");
            return;
        }

        RegisterButton.IsEnabled = false;
        NicknameTextBox.IsEnabled = false;
        RegistrationStatusText.Text = App.LocalizationService.T("프로필을 확인하는 중입니다.");

        try
        {
            await App.RankPollingService.SetProfileByNicknameAsync(nickname);
            RegistrationStatusText.Text = App.LocalizationService.T("프로필을 등록했습니다.");
            UpdateCurrentProfile();
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            RegistrationStatusText.Text = App.LocalizationService.T("입력한 닉네임을 찾지 못했습니다.");
        }
        catch
        {
            RegistrationStatusText.Text = App.LocalizationService.T("프로필을 불러오지 못했습니다. 잠시 후 다시 시도해주세요.");
        }
        finally
        {
            RegisterButton.IsEnabled = true;
            NicknameTextBox.IsEnabled = true;
        }
    }

    private void UpdateCurrentProfile()
    {
        var profile = App.RankPollingService.CurrentProfile;
        var snapshot = App.RankPollingService.Current;

        if (profile is null)
        {
            CurrentNicknameText.Text = App.LocalizationService.T("등록된 프로필 없음");
            CurrentRankText.Text = "";
            return;
        }

        CurrentNicknameText.Text = profile.Nickname;

        CurrentRankText.Text = snapshot is null
            ? App.LocalizationService.T(App.RankPollingService.LastError ?? "랭크 정보를 불러오는 중입니다.")
            : $"{App.LocalizationService.Tier(snapshot.TierKey, snapshot.Division)} · {snapshot.Rp:N0} RP · #{snapshot.Rank:N0}";
    }
}
