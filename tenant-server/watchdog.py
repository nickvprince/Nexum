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
def client():
   """
   This function runs a watchdog program that ensures
   nexum.exe is running. If it is not it starts it
   """
   while True:
            # check if nexum.exe is running
      if 'nexum.exe' not in (str(p.name()).lower() for p in psutil.process_iter()):

        subprocess.Popen(["c:\\program files\\nexum\\nexum.exe"], creationflags=subprocess.CREATE_NO_WINDOW)

def server():
  """
  This function runs a watchdog program that ensures
  nexserv.exe is running. If it is not it starts it
  """
  while True:
            # check if nexum.exe is running
    if 'nexserv.exe' not in (str(p.name()).lower()  for p in psutil.process_iter()):
      subprocess.Popen(["c:\\program files\\nexum\\nexum.exe"], creationflags=subprocess.CREATE_NO_WINDOW)

if __name__ == '__main__':
    for p in psutil.process_iter():
        if 'nexum.exe' in p.name().lower():
            client()
            break
        elif 'nexserv.exe' in p.name().lower():
            server()
            break
    print("nexum.exe not found")


