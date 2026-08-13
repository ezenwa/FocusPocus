#define MyAppName "FocusPocus"
#define MyAppVersion "2.1.0"
#define MyAppPublisher "Joshua Ezenwa"
#define MyAppExeName "FocusPocus.exe"
#define MyEngineExeName "FocusPocus.Engine.exe"

[Setup]
AppId={{B6CF717A-A55A-45DC-A247-292411293858}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\FocusPocus
DefaultGroupName=FocusPocus
OutputDir=..\dist
OutputBaseFilename=FocusPocus-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\src\SpotDot\Assets\FocusPocus.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "startup"; Description: "{cm:AutoStartProgram,{#MyAppName}}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish-winui-203\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[InstallDelete]
Type: files; Name: "{app}\SpotDot.exe"
Type: files; Name: "{app}\SpotDot.dll"
Type: files; Name: "{app}\SpotDot.deps.json"
Type: files; Name: "{app}\SpotDot.runtimeconfig.json"
Type: files; Name: "{autoprograms}\SpotDot.lnk"
Type: files; Name: "{userstartup}\SpotDot.lnk"
Type: files; Name: "{userstartup}\FocusPocus.lnk"

[Icons]
Name: "{autoprograms}\FocusPocus"; Filename: "{app}\{#MyAppExeName}"
Name: "{userstartup}\FocusPocus"; Filename: "{app}\{#MyEngineExeName}"; Parameters: "--tray"; WorkingDir: "{app}"; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,FocusPocus}"; Flags: nowait postinstall skipifsilent
