@echo off
set name=main.py
@echo %name%
cd
pip install -U pyinstaller
pyinstaller --hiddenimport win32timezone, logger --clean -F %name%
pause






pystray
pandas
crpyptography
sqlite3
win32timezone
Image