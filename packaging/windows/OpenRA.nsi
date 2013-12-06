; Copyright 2007-2013 OpenRA developers (see AUTHORS)
; This file is part of OpenRA.
; 
;  OpenRA is free software: you can redistribute it and/or modify
;  it under the terms of the GNU General Public License as published by
;  the Free Software Foundation, either version 3 of the License, or
;  (at your option) any later version.
; 
;  OpenRA is distributed in the hope that it will be useful,
;  but WITHOUT ANY WARRANTY; without even the implied warranty of
;  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;  GNU General Public License for more details.
; 
;  You should have received a copy of the GNU General Public License
;  along with OpenRA.  If not, see <http://www.gnu.org/licenses/>.


!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "WordFunc.nsh"

Name "OpenRA"
OutFile "OpenRA.exe"

InstallDir $PROGRAMFILES\OpenRA
InstallDirRegKey HKLM "Software\OpenRA" "InstallDir"

SetCompressor lzma

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${SRCDIR}\COPYING"
!insertmacro MUI_PAGE_DIRECTORY

!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKLM"
!define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\OpenRA"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "OpenRA"

Var StartMenuFolder
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

;***************************
;Section Definitions
;***************************
Section "-Reg" Reg
	WriteRegStr HKLM "Software\OpenRA" "InstallDir" $INSTDIR
SectionEnd

Section "Game" GAME
	RMDir /r "$INSTDIR\mods"
	SetOutPath "$INSTDIR\mods"
	File /r "${SRCDIR}\mods\cnc"
	File /r "${SRCDIR}\mods\d2k"
	File /r "${SRCDIR}\mods\ra"
	SetOutPath "$INSTDIR"
	File "${SRCDIR}\OpenRA.Game.exe"
	File "${SRCDIR}\OpenRA.Utility.exe"
	File "${SRCDIR}\OpenRA.FileFormats.dll"
	File "${SRCDIR}\OpenRA.Renderer.Sdl2.dll"
	File "${SRCDIR}\OpenRA.Renderer.SdlCommon.dll"
	File "${SRCDIR}\OpenRA.Renderer.Gl.dll"
	File "${SRCDIR}\OpenRA.Renderer.Cg.dll"
	File "${SRCDIR}\OpenRA.Renderer.Null.dll"
	File "${SRCDIR}\OpenRA.Irc.dll"
	File "${SRCDIR}\ICSharpCode.SharpZipLib.dll"
	File "${SRCDIR}\FuzzyLogicLibrary.dll"
	File "${SRCDIR}\Mono.Nat.dll"
	File "${SRCDIR}\AUTHORS"
	File "${SRCDIR}\CHANGELOG"
	File "${SRCDIR}\COPYING"
	File "${SRCDIR}\*.ttf"
	File "${SRCDIR}\OpenRA.ico"
	File "${SRCDIR}\Tao.*.dll"
	File "${SRCDIR}\SharpFont.dll"
	File "${SRCDIR}\global mix database.dat"
	File "${SRCDIR}\GeoIP.dll"
	File "${SRCDIR}\GeoIP.dat"
	File "${SRCDIR}\KopiLua.dll"
	File "${SRCDIR}\NLua.dll"
	File OpenAL32.dll
	File SDL.dll
	File SDL2.dll
	File freetype6.dll
	File zlib1.dll

	!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
		CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
		CreateShortCut "$SMPROGRAMS\$StartMenuFolder\OpenRA.lnk" $OUTDIR\OpenRA.Game.exe "" \
			"$OUTDIR\OpenRA.Game.exe" "" "" "" ""
	!insertmacro MUI_STARTMENU_WRITE_END

	SetOutPath "$INSTDIR\cg"
	File "${SRCDIR}\cg\*.fx"
	SetOutPath "$INSTDIR\glsl"
	File "${SRCDIR}\glsl\*.frag"
	File "${SRCDIR}\glsl\*.vert"
SectionEnd

Section "Editor" EDITOR
	SetOutPath "$INSTDIR"
	File "${SRCDIR}\OpenRA.Editor.exe"

	!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
		CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
		CreateShortCut "$SMPROGRAMS\$StartMenuFolder\OpenRA Editor.lnk" $OUTDIR\OpenRA.Editor.exe "" \
			"$OUTDIR\OpenRA.Editor.exe" "" "" "" ""
	!insertmacro MUI_STARTMENU_WRITE_END
SectionEnd

SectionGroup /e "Settings"
	Section "Portable Install" PORTABLE
		CreateDirectory $INSTDIR\Support
	SectionEnd
	Section "Desktop Shortcut" DESKTOPSHORTCUT
		SetOutPath "$INSTDIR"
		CreateShortCut "$DESKTOP\OpenRA.lnk" $INSTDIR\OpenRA.Game.exe "" \
			"$INSTDIR\OpenRA.Game.exe" "" "" "" ""
	SectionEnd
SectionGroupEnd

Function .onInit
	SectionSetFlags ${PORTABLE} 0
FunctionEnd

;***************************
;Dependency Sections
;***************************
Section "-DotNet" DotNet
	ClearErrors
	ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5" "Install"
	IfErrors error 0
	IntCmp $0 1 0 error 0
	ClearErrors
	ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5" "SP"
	IfErrors error 0
	IntCmp $0 1 done error done
	error: 
		MessageBox MB_YESNO ".NET Framework v3.5 SP1 or later is required to run OpenRA. $\n \
		Do you wish for the installer to launch your web browser in order to download and install it?" \
		IDYES download IDNO error2
	download:
		ExecShell "open" "http://www.microsoft.com/downloads/en/details.aspx?familyid=ab99342f-5d1a-413d-8319-81da479ab0d7"
		Goto done
	error2:
		MessageBox MB_OK "Installation will continue but be aware that OpenRA will not run unless .NET v3.5 SP1 \
		or later is installed."
	done:
SectionEnd

;***************************
;Uninstaller Sections
;***************************
Section "-Uninstaller"
	WriteUninstaller $INSTDIR\uninstaller.exe
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA" "DisplayName" "OpenRA"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA" "UninstallString" "$INSTDIR\uninstaller.exe"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA" "InstallLocation" "$INSTDIR"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA" "DisplayIcon" "$INSTDIR\OpenRA.ico"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA" "Publisher" "OpenRA developers"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA" "URLInfoAbout" "http://open-ra.org"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA" "NoModify" "1"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA" "NoRepair" "1"
	
	!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
		CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Uninstall.lnk" "$INSTDIR\uninstaller.exe" "" \
			"" "" "" "" "Uninstall OpenRA"
	!insertmacro MUI_STARTMENU_WRITE_END
SectionEnd

!macro Clean UN
Function ${UN}Clean
	RMDir /r $INSTDIR\mods
	RMDir /r $INSTDIR\maps
	RMDir /r $INSTDIR\cg
	RMDir /r $INSTDIR\glsl
	Delete $INSTDIR\OpenRA.Launcher.exe
	Delete $INSTDIR\OpenRA.Game.exe
	Delete $INSTDIR\OpenRA.Utility.exe
	Delete $INSTDIR\OpenRA.Editor.exe
	Delete $INSTDIR\OpenRA.FileFormats.dll
	Delete $INSTDIR\OpenRA.Renderer.Gl.dll
	Delete $INSTDIR\OpenRA.Renderer.Cg.dll
	Delete $INSTDIR\OpenRA.Renderer.Null.dll
	Delete $INSTDIR\OpenRA.Renderer.SdlCommon.dll
	Delete $INSTDIR\OpenRA.Irc.dll
	Delete $INSTDIR\ICSharpCode.SharpZipLib.dll
	Delete $INSTDIR\FuzzyLogicLibrary.dll
	Delete $INSTDIR\Mono.Nat.dll
	Delete $INSTDIR\Tao.*.dll
	Delete $INSTDIR\SharpFont.dll
	Delete $INSTDIR\AUTHORS
	Delete $INSTDIR\CHANGELOG
	Delete $INSTDIR\COPYING
	Delete $INSTDIR\OpenRA.ico
	Delete $INSTDIR\*.ttf
	Delete "$INSTDIR\global mix database.dat"
	Delete $INSTDIR\GeoIP.dat
	Delete $INSTDIR\GeoIP.dll
	Delete $INSTDIR\KopiLua.dll
	Delete $INSTDIR\NLua.dll
	Delete $INSTDIR\OpenAL32.dll
	Delete $INSTDIR\SDL.dll
	Delete $INSTDIR\freetype6.dll
	Delete $INSTDIR\zlib1.dll
	RMDir /r $INSTDIR\Support
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenRA"
	Delete $INSTDIR\uninstaller.exe
	RMDir $INSTDIR
	
	!insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
	RMDir /r "$SMPROGRAMS\$StartMenuFolder"
	Delete $DESKTOP\OpenRA.lnk
	DeleteRegKey HKLM "Software\OpenRA"
FunctionEnd
!macroend

!insertmacro Clean ""
!insertmacro Clean "un."

Section "Uninstall"
	Call un.Clean
SectionEnd

;***************************
;Section Descriptions
;***************************
LangString DESC_GAME ${LANG_ENGLISH} "OpenRA engine, official mods and dependencies"
LangString DESC_EDITOR ${LANG_ENGLISH} "OpenRA map editor"
LangString DESC_PORTABLE ${LANG_ENGLISH} "Store support files in the install directory."
LangString DESC_DESKTOPSHORTCUT ${LANG_ENGLISH} "Place shortcut on the Desktop."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${GAME} $(DESC_GAME)
	!insertmacro MUI_DESCRIPTION_TEXT ${EDITOR} $(DESC_EDITOR)
	!insertmacro MUI_DESCRIPTION_TEXT ${PORTABLE} $(DESC_PORTABLE)
	!insertmacro MUI_DESCRIPTION_TEXT ${DESKTOPSHORTCUT} $(DESC_DESKTOPSHORTCUT)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;***************************
;Callbacks
;***************************

Function .onInstFailed
	Call Clean
FunctionEnd
