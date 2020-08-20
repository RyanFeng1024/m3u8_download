:: 说明：合并当前目录下所有ts文件生成新的一个以当前目录名称命名的ts文件

@echo off

:: 设置页码编码 防止中文显示乱码
chcp 65001

:: echo now disk "%~d0"
echo 当前目录："%~dp0"
echo 开始合并
:: echo now full path "%~f0"
:: echo cmd default path "%cd%"

for %%i in ("%cd%") do set filename=%%~ni
:: echo "copy /b %~dp0*.ts %~dp0%filename%.ts"
call copy /b "%~dp0*.ts" "%~dp0%filename%.ts"

echo.
echo 合并完成
echo 文件已合并到 "%~dp0%filename%.ts"
pause