@echo off
rem 清空频道
.\Tools\curl.exe -X POST http://api.a8.to/Translate/API_Clean -d "channel=SP.StudioCore"

for /f "delims=" %%a in ('dir /b /a /s .\*.cs .\*.md') do (
	echo %%a
	.\Tools\curl.exe http://api.a8.to/Translate/API_SaveKeyword?Channel=SP.StudioCore -X POST -F file=@%%a
)