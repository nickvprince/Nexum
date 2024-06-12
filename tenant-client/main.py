"""
# Program: tenant-client
# File: main.py
# Authors: 1. Danny Smith
#
# Date: 2/17/2024
# purpose: This is the main file for the program A2. This program ensures connectivity
# to A3, recieves jobs from A3 and executes backups and heartbeats. It has a tray icon
# that the user may not interact with

# Note. Requires admin to run
# Note. Requires the following packages to be installed: cryptography, pystray, 
# PIL, os, sqlite3, pandas, flask, base64, hashlib, winreg, time, traceback, threading

Types
-----

1. Entity - Holds information such as a struct
2. Connector - Connects local calls to an API call
3. Controller - Manages a parts of the program
4. File IO - Provides file IO functionality
5. Security - provides security functionality


Configuration: entity
JobSettings: entity
Job: entity
API: connector
RunJob: controller
InitSql: file IO
FlaskServer: API
IconManager: controller
Logger: file IO
Security: Security

Error Codes
-----------
1001 - Not found in settings
1002 - General Error
1003 - File not found
1004 - Decryption failed
1005 - Permission error
1006 - Heartbeat failed to send
1007 - Encryption Error
1008 - Job not configured or failed to run

404 - Not found
401 - Access denied
503 - Internal server error
"""

# pylint: disable= no-member,no-name-in-module, import-error


import time
from logger import Logger
from MySqlite import MySqlite
from InitSql import InitSql
from runjob import RunJob, LOCAL_JOB
from helperfunctions import get_client_info, logs, tenant_portal,load
from security import Security
from job import Job
from jobsettings import JobSettings
from iconmanager import IconManager, image_path
from flaskserver import FlaskServer
from HeartBeat import HeartBeat

# Global variables

def init():
    """
    Initializes the program
    """
    # pylint: disable= global-variable-not-assigned
    # Disabled since it is used with LOCAL_JOB.load(0)
    global LOCAL_JOB
    # pylint: enable= global-variable-not-assigned
    InitSql()
    LOCAL_JOB.load(0)
    Security.load_client_secret()
    load()


def main():
    """
    Main method of the program for testing and starting the program
    """
    # create a Logger
    l = Logger()
    # init databases
    init()
    # get client info
    get_client_info()
    # create the IconManager
    i = IconManager(image_path, IconManager.create_menu(IconManager.get_status(),
    IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum Client",l)
    # run the icon
    i.run()
    # log a message
    l.log("INFO", "Main", "Main has started", "000", time.asctime())

    FlaskServer.set_run_job_object(RunJob())

    # run server to listen for requests
    h = HeartBeat()
    FlaskServer()





if __name__ == "__main__":
    InitSql()
    MySqlite.write_setting("client_secret", "ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD")
    MySqlite.write_setting("TENANT_ID","1")
    MySqlite.write_setting("CLIENT_ID","1")
    MySqlite.write_setting("TENANT_PORTAL_URL","http://127.0.0.1:5000/index")
    MySqlite.write_setting("POLLING_INTERVAL","10")
    MySqlite.write_setting("server_address","127.0.0.1")
    MySqlite.write_setting("server_port","5000")
    MySqlite.write_setting("tenant_secret","ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD")
    MySqlite.write_setting("heartbeat_interval","5")
    MySqlite.write_setting("service_address","127.0.0.1:5004")
    MySqlite.write_setting("salt","salt")
    MySqlite.write_setting("pepper","pepricart")
    MySqlite.write_setting("salt2","salt2")



    main()
