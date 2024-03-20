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
404 - Not found
401 - Access denied
500 - Internal server error
"""

# pylint: disable= no-member,no-name-in-module, import-error


import time
from pyuac import main_requires_admin
from logger import Logger
from initsql import InitSql
from runjob import RunJob, LOCAL_JOB
from helperfunctions import get_client_info, logs, tenant_portal,check_first_run
from security import Security
from jobsettings import JobSettings
from iconmanager import IconManager, image_path
from flaskserver import FlaskServer
# Global variables


@main_requires_admin
def main():
    """
    Main method of the program for testing and starting the program
    """

    t = JobSettings()
    t.backup_path = "\\\\192.168.2.201\\Backups"
    t.user = "tenant\\Backup"
    t.password = "Test123"
    LOCAL_JOB.set_settings(t)
    Security.set_client_secret("ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD")
    # check if this is the first run
    check_first_run("1234")
    # create a Logger
    l = Logger()
    # init databases
    InitSql()
    # get client info
    get_client_info()
    # create the IconManager
    i = IconManager(image_path, IconManager.create_menu(IconManager.get_status(),
    IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum Client",l)
    # run the icon
    i.run()
    # log a message
    l.log("INFO", "Main", "Main has started", "000", time.asctime())
    # run the job
    temp = Security.sha256_string("ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD")
    temp = Security.add_salt_pepper(temp, "salt", "pepricart", "salt2")
    print(Security.encrypt_client_secret(temp))
    FlaskServer.set_run_job_object(RunJob())

    # run server to listen for requests
    FlaskServer()

if __name__ == "__main__":

    main()
