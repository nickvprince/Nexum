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

import subprocess
from iconmanager import IconManager, image_path
from helperfunctions import logs, tenant_portal,load
from logger import Logger

from sql import InitSql
from runjob import LOCAL_JOB
from HeartBeat import HeartBeat
from sql import MySqlite
from flaskserver import FlaskServer



# Global variables
def init():
    """
    Initializes the program
    """
    InitSql()
    global LOCAL_JOB
    LOCAL_JOB.load(0)
    load()
    #start watchdog.exe

def main():
    """
    Main method of the program for testing and starting the program
    """
    processes = str(subprocess.check_output("tasklist", shell=True))
    # if nexserv.exe is running exit
    if len(processes) > 0:
        count = processes.count("nexserv.exe") + processes.count("NexumServer.exe")
        if count >=3:
            return

    # initialize databases, settings, and global variables
    init()

    # set the status to online
    MySqlite.write_setting("Status","Online")
    clients = MySqlite.load_clients()

    # logger
    l = Logger()

    # heartbeat monitor
    _ = HeartBeat(MySqlite.read_setting("apikey"), 10,clients)

    # create the IconManager
    i = IconManager(image_path, IconManager.create_menu(IconManager.get_status(),
    IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum Server",l)

    # run the icon
    i.run()

    # log a message
    l.log("INFO", "Main", "Main has started", "000","main.py")

    # Start the server
    f = FlaskServer()
    f.run()

if __name__ == "__main__":
    main()
