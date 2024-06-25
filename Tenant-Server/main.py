"""
# Program: tenant-server
# File: main.py
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
    """
    """
    MySqlite.write_setting("client_secret", "ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD")
    MySqlite.write_setting("TENANT_ID","1")
    MySqlite.write_setting("CLIENT_ID","1")
    MySqlite.write_setting("POLLING_INTERVAL","10")
    MySqlite.write_setting("apikey","01ee3ece-7976-4cda-b4f4-00d5f68d1cbd")
    MySqlite.write_setting("server_address","127.0.0.1")
    MySqlite.write_setting("server_port","5002")
    MySqlite.write_setting("msp-port","7101")
    MySqlite.write_setting("tenant_secret","ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD")
    MySqlite.write_setting("heartbeat_interval","5")
    MySqlite.write_setting("Master-Uninstall","LJA;HFLASBFOIASH[jfnW.FJPIH")
    MySqlite.add_install_key("JBQDPYQ7310712631DHLSAU8AWY]")
    MySqlite.add_install_key("LJA;HFLASBFOIASH[jfnW.FJPIH")
    MySqlite.write_setting("TENANT_PORTAL_URL","http://127.0.0.1:5000/index")
    MySqlite.write_setting("version","1")
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
    f = FlaskServer()
    f.run()

if __name__ == "__main__":

    main()
