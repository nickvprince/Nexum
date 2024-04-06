"""
# Program: Tenant-server, Tenant-Client
# File: watchdog.py
# Authors: 1. Danny Smith
#
# Date: 3/28/2024
# purpose: 
# This file runs a watchdog program that ensures
# nexum.exe is running. If it is not it starts it

# Class Types: 
#               1. Watchdog

"""
# pylint: disable= import-error
import subprocess
import psutil


if __name__ == '__main__':

  while True:
            # check if nexum.exe is running
    if 'nexum.exe' not in (p.name() for p in psutil.process_iter()):
      subprocess.Popen(["c:\\program files\\nexum\\nexum.exe"], creationflags=subprocess.CREATE_NO_WINDOW)

