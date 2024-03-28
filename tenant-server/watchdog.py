"""
Information
"""
import os
import subprocess
import psutil


if __name__ == '__main__':

  while True:
            # check if nexum.exe is running
    if 'nexum.exe' not in (p.name() for p in psutil.process_iter()):
      subprocess.Popen(["c:\\program files\\nexum\\nexum.exe"], creationflags=subprocess.CREATE_NO_WINDOW)

