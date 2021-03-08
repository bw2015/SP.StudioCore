chcp 65001
@echo off
set config=Model.ini
set code=%~1
set folder=%~d1%~p1
set i=0
 
:folder
if NOT EXIST "%folder%%config%" (
	set folder=%folder%..\
	set /a i=%i%+1
	if %i% equ 5 (
		echo CANOT FIND CONFIG FILE %config%
		pause
		goto end
	) else (
		goto folder	
	)
)
 
for /f "skip=1 tokens=1,2 delims==" %%a IN ('call type "%folder%%config%"') do set %%a=%%b
set /p table=Input TableName:
IF NOT EXIST DataQuery.sql .\curl -k http://api.baidu.com/BuildModel > DataQuery.sql
sqlcmd -S"%SERVER%" -U%UID% -P%PWD% -d"%DB%" -s"|" -i"DataQuery.sql" -v TABLE="%table%" -W > query.tmp
.\curl -k http://api.baidu.com/BuildModel -F "query=@query.tmp" -F "code=@%code%" -H "action:XML" -t utf-8  > code.tmp
for /f %%i in ("code.tmp") do set size=%%~zi
if %size% equ 0 (
	echo Build Faild
	del *.tmp
	pause
	exit
) else (
	echo SUCCESS
	move /Y code.tmp "%code%"
	del *.tmp	
)
:end