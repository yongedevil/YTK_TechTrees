
rem @echo off

set H=R:\KSP_1.4.1_dev-YongTech
set GAMEDIR=YongeTech_TechTreesPlugin

echo %H%

copy /Y "%1%2" "GameData\%GAMEDIR%\Plugins"
mkdir "%H%\GameData\%GAMEDIR%"
xcopy  /E /y GameData\%GAMEDIR% "%H%\GameData\%GAMEDIR%"

