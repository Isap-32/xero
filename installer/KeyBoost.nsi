Unicode true
!include "MUI2.nsh"
!include "x64.nsh"
!include "WinVer.nsh"
Name "KeyBoost"
OutFile "../artifacts/KeyBoost-1.0.0-Setup.exe"
InstallDir "$LOCALAPPDATA\Programs\KeyBoost"
InstallDirRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "InstallLocation"
VIProductVersion "1.0.0.0"
VIAddVersionKey /LANG=1033 "ProductName" "KeyBoost"
VIAddVersionKey /LANG=1033 "FileDescription" "KeyBoost per-user installer"
VIAddVersionKey /LANG=1033 "FileVersion" "1.0.0"
VIAddVersionKey /LANG=1033 "ProductVersion" "1.0.0"
VIAddVersionKey /LANG=1033 "LegalCopyright" "KeyBoost contributors"
RequestExecutionLevel user
SetCompressor /SOLID lzma
!define MUI_ICON "../src/KeyBoost/Resources/KeyBoost.ico"
!define MUI_UNICON "../src/KeyBoost/Resources/KeyBoost.ico"
!define MUI_ABORTWARNING
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\KeyBoost.exe"
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"
Section "KeyBoost"
  SetShellVarContext current
  SetOutPath "$INSTDIR"
  File "../artifacts/win-x64/KeyBoost.exe"
  File "../THIRD-PARTY-NOTICES.txt"
  SetOutPath "$INSTDIR\licenses"
  File "../licenses/*.txt"
  SetOutPath "$INSTDIR"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  CreateDirectory "$SMPROGRAMS\KeyBoost"
  CreateShortcut "$SMPROGRAMS\KeyBoost\KeyBoost.lnk" "$INSTDIR\KeyBoost.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "InstallLocation" "$INSTDIR"
  ReadRegStr $1 HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "KeyBoost"
  ${If} $1 != ""
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "KeyBoost" '$\"$INSTDIR\KeyBoost.exe$\"'
  ${EndIf}
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "DisplayName" "KeyBoost"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "DisplayVersion" "1.0.0"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "Publisher" "KeyBoost"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "UninstallString" '$\"$INSTDIR\Uninstall.exe$\"'
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "DisplayIcon" "$INSTDIR\KeyBoost.exe"
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "NoModify" 1
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost" "NoRepair" 1
SectionEnd
Section "Uninstall"
  SetShellVarContext current
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "KeyBoost"
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KeyBoost"
  Delete "$SMPROGRAMS\KeyBoost\KeyBoost.lnk"
  RMDir "$SMPROGRAMS\KeyBoost"
  Delete "$INSTDIR\KeyBoost.exe"
  Delete "$INSTDIR\THIRD-PARTY-NOTICES.txt"
  Delete "$INSTDIR\licenses\DOTNET-NOTICES.txt"
  Delete "$INSTDIR\licenses\WINDOWS-DESKTOP-LICENSE.txt"
  Delete "$INSTDIR\licenses\NSIS-LICENSES.txt"
  Delete "$INSTDIR\licenses\WPF-UI-LICENSE.txt"
  RMDir "$INSTDIR\licenses"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  ; User profiles and the previous Windows settings snapshot are intentionally retained.
SectionEnd

Function .onInit
  ${IfNot} ${RunningX64}
    MessageBox MB_OK "KeyBoost requires 64-bit Windows 10 or Windows 11."
    Abort
  ${EndIf}
  ${IfNot} ${AtLeastWin10}
    MessageBox MB_OK "KeyBoost requires Windows 10 or Windows 11."
    Abort
  ${EndIf}
  System::Call 'kernel32::OpenMutexW(i 0x100000, i 0, w "Local\KeyBoost.Desktop.1") p.r0'
  ${If} $0 <> 0
    System::Call 'kernel32::CloseHandle(p r0)'
    MessageBox MB_OK "Close KeyBoost, including its tray window, before installing."
    Abort
  ${EndIf}
FunctionEnd

Function un.onInit
  System::Call 'kernel32::OpenMutexW(i 0x100000, i 0, w "Local\KeyBoost.Desktop.1") p.r0'
  ${If} $0 <> 0
    System::Call 'kernel32::CloseHandle(p r0)'
    MessageBox MB_OK "Close KeyBoost before uninstalling."
    Abort
  ${EndIf}
FunctionEnd
