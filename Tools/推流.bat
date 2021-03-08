@echo off
chcp 65001
set /p url=流地址:
set /p name=播放名称(只可英文数字):
echo PC地址：https://live.aabv.top/ES2/%name%.flv
echo 手机地址：https://live.aabv.top/ES2/%name%.m3u8
color
echo 请复制上述地址之后按回车键
pause

ffmpeg.exe -re -analyzeduration 8000 -probesize 200000 -headers "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.106 Safari/537.36" -i "%url%" -strict -2 -bsf:a aac_adtstoasc -c copy -flvflags aac_seq_header_detect -f flv rtmp://rmtp.aabv.top/ES2/%name%

pause
