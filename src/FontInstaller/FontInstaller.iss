; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Roboto Fonts"
#define MyAppVersion "2.137"
#define MyAppPublisher "Purple Pen Software"
#define MyAppURL "https://purple-pen.org"
#define BuildDir "..\PurplePen\bin\Release"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{0B065142-CE34-464F-86E6-BB5C747F6F3C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
CreateAppDir=no
; Uncomment the following line to run in non administrative install mode (install for current user only.)
PrivilegesRequired=lowest
OutputDir=output
OutputBaseFilename=Roboto Font Installer
SetupIconFile={#BuildDir}\..\..\Images\PurplePen.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
Uninstallable=no
CreateUninstallRegKey=no

DisableWelcomePage=yes
ShowLanguageDialog=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "bulgarian"; MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Files]
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-Regular.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-Bold.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Bold"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-Italic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-BoldItalic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Bold Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-Black.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Black"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-BlackItalic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Black Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-Light.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Light"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-LightItalic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Light Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-Medium.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Medium"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-MediumItalic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Medium Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-Thin.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Thin"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\Roboto-ThinItalic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Thin Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\RobotoCondensed-Regular.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Condensed"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\RobotoCondensed-Bold.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Condensed Bold"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\RobotoCondensed-Italic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Condensed Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\RobotoCondensed-BoldItalic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Condensed Bold Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\RobotoCondensed-Light.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Condensed Light"; Flags: onlyifdoesntexist uninsneveruninstall 
Source: "{#BuildDir}\..\..\..\RobotoFont\RobotoCondensed-LightItalic.ttf"; DestDir: "{commonfonts}"; FontInstall: "Roboto Condensed Light Italic"; Flags: onlyifdoesntexist uninsneveruninstall 
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

