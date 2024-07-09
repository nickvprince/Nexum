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
import requests


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

    init()
    MySqlite.write_setting("apikey","922ef041-a5ef-4473-90c8-46038b052a28")# 01ee3ece-7976-4cda-b4f4-00d5f68d1cbd
    MySqlite.write_setting("msp_api","a109ef4c-b611-4aff-ac26-07b86a7161aa")
    MySqlite.write_setting("version","1.0.0")
    MySqlite.write_setting("versiontag","alpha")
    MySqlite.write_setting("Status","Online")
    MySqlite.write_setting("job_status","NotStarted")
    clients = MySqlite.load_clients()
    l = Logger()
    _ = HeartBeat(MySqlite.read_setting("apikey"), 10,clients)

    # create the IconManager
    i = IconManager(image_path, IconManager.create_menu(IconManager.get_status(),
    IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum Server",l)
    # run the icon
    i.run()
    # log a message

    l.log("INFO", "Main", "Main has started", "000", time.asctime())
    f = FlaskServer()
    f.run()

if __name__ == "__main__":
    main()
