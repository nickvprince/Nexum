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
    clients = MySqlite.load_clients()
    l = Logger()
    H = HeartBeat(Security.get_client_secret(), 10,clients)

    # create the IconManager
    i = IconManager(image_path, IconManager.create_menu(IconManager.get_status(),
    IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum Server",l)
    # run the icon
    i.run()
    # log a message

    l.log("INFO", "Main", "Main has started", "000", time.asctime())
    MySqlite.write_setting("Status","running")
    f = FlaskServer()
    f.run()

if __name__ == "__main__":
    main()
