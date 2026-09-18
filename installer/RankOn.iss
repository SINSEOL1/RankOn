#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

[Setup]
AppId={{A8A5C3C2-1A40-4F1E-9B77-4C60507A6453}
AppName=랭크온
AppVersion={#MyAppVersion}
AppPublisher=SINSEOL
DefaultDirName={localappdata}\Programs\RankOn
DefaultGroupName=랭크온
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=output
OutputBaseFilename=RankOn-Setup-v{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName=랭크온

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\랭크온"; Filename: "{app}\RankOn.exe"
Name: "{autodesktop}\랭크온"; Filename: "{app}\RankOn.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "바탕 화면 바로가기 만들기"; GroupDescription: "추가 아이콘:"; Flags: unchecked

[Run]
Filename: "{app}\RankOn.exe"; Description: "랭크온 실행"; Flags: nowait postinstall skipifsilent
