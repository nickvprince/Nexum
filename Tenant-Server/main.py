"""
# Program: tenant-server# File: main.py
# Authors: 1. Danny Smith
#
# Date: 3/192024
# purpose: This is the main file for the program A2. This program ensures connectivity
# to A3, recieves jobs from A3 and executes backups and heartbeats. It has a tray icon
# that the user may not interact with


"""

# pylint: disable= no-member,no-name-in-module, import-error,global-variable-not-assigned

import socket
import time
from iconmanager import IconManager, image_path
from helperfunctions import logs, tenant_portal,load
from logger import Logger
from flaskserver import FlaskServer
from sql import InitSql
from runjob import LOCAL_JOB
from security import Security
from HeartBeat import HeartBeat
from sql import MySqlite
import subprocess


# Global variables
def init():
    """
    Initializes the program
    """
    InitSql()
    global LOCAL_JOB
    LOCAL_JOB.load(0)
    Security.load_tenant_secret()
    load()
    #start watchdog.exe

def main():
    """
    Main method of the program for testing and starting the program
    """
    MySqlite.write_setting("uuid","a14df31c-07fc-4d0b-9ddf-0f59b16db611")
    MySqlite.write_setting("msp_server_address","127.0.0.1")
    MySqlite.write_setting("msp-port","7101")
    MySqlite.write_setting("CLIENT_ID","0")
    MySqlite.write_setting("apikey","fb1a0811-1637-4f4d-8da9-44243a37cd66")# 01ee3ece-7976-4cda-b4f4-00d5f68d1cbd
    MySqlite.write_setting("msp_api","33c224ec-d1f0-4845-8964-6fac7ae231ae")
    MySqlite.write_setting("version","1.0.0")
    MySqlite.write_setting("versiontag","alpha")
    MySqlite.write_setting("Status","Online")
    MySqlite.write_setting("job_status","NotStarted")
    MySqlite.write_setting("msp_server_address","127.0.0.1")
    MySqlite.write_setting("msp-port","7101")
    init()

    result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
    capture_output=True, text=True,check=True,shell=True)
    output = result.stdout.strip()
    output = output.split('\n\n', 1)[-1]
    

    clients = MySqlite.load_clients()
    l = Logger()
    _ = HeartBeat(MySqlite.read_setting("apikey"), 10,clients)

    # create the IconManager
    i = IconManager(image_path, IconManager.create_menu(IconManager.get_status(),
    IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum Server",l)
    # run the icon
    i.run()
    # log a message

    l.log("INFO", "Main", "Main has started", "000","main.py")
    f = FlaskServer()
    f.run()

if __name__ == "__main__":
    main()
