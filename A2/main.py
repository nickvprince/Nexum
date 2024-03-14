"""
# Program: A2
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
# pylint: disable=line-too-long
# pylint: disable=no-self-argument
# pylint: disable= no-method-argument
# pylint: disable= unused-argument
# pylint: disable= global-statement
# pylint: disable= pointless-string-statement
# pylint: disable= bare-except
# pylint: disable= relative-beyond-top-level
# pylint: disable= no-member
# pylint: disable= import-error

import subprocess
import time
import traceback
import threading
import os
import sqlite3
import base64
import hashlib
import winreg
import pystray
from PIL import Image
import pandas as pd
from flask import Flask, request,Response,make_response
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import job
from pyuac import main_requires_admin

# Global variables
current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
image_path = os.path.join(current_dir, '../Data/n.png') # path to the icon image
POLLING_INTERVAL = 5 # interval to send the server heartbeats
CLIENT_ID = -1 # client id
TENANT_ID = -1 # tenant id
TENANT_PORTAL_URL = "https://nexum.com/tenant_portal" # url to the tenant portal
logdirectory = os.path.join(current_dir,'../logs') # directory for logs
logpath = os.path.join('/log.db') # path to the log database
settingsDirectory = os.path.join(current_dir, '..\\settings') # directory for settings
SETTINGS_PATH= os.path.join(current_dir, settingsDirectory+'\\settings.db') # path to the settings database
LOCAL_JOB = job.Job() # job assigned to this computer
CLIENT_SECRET = None # secret for the client to communicate with A2
jobFile=os.path.join('/settings.db')
configFile=os.path.join('/settings.db')
job_settingsFile=os.path.join('/settings.db')
RUN_JOB_OBJECT = None


class API():
    """
    Class to interact with the API. Used for local API calls and easy integration when API calls are changed
    Type: Connector
    Relationship: NONE
    """

    def get_tenant_portal_url():
        """
        Gets the tenant portal URL from the tenant server device
        """
        Logger.debug_print("Getting tenant portal url")
        # call the API from tenant server to get the tenant portal URL
        return "https://nexum.com/tenant_portal"
    def get_status():
        """
        Call the API from tenant server to get the status of the client
        """
        Logger.debug_print("Getting status")
        # call the API from tenant server to get the status of the client
        return "running"
    def get_percent():
        """
        call the API from tenant server to get the percent complete of the job
        """
        Logger.debug_print("Getting percent")
        # call the API from tenant server to get the percent complete of the job
        return 70
    def get_version():
        """
        Call the API from tenant server to get the version of the program
        """
        Logger.debug_print("Getting version")
        # call the API from tenant server to get the version of the program
        return "1.2.7"
    def get_job():
        """
        Call the API from tenant server to get the job assigned to this computer
        """
        Logger.debug_print("Getting job")
        # call the API from tenant server to get the job assigned to this computer
        return "backup"
    def get_client_id():
        """
        Call the API from tenant server to get the client id
        """
        Logger.debug_print("Getting client id")
        # call the API from tenant server to get the client id
        return 1
    def get_tenant_id():
        """
        Call the API from tenant server to get the tenant id
        """
        Logger.debug_print("Getting tenant id")
        # call the API from tenant server to get the tenant id
        return 1
    def get_download_key():
        """
        call the API from tenant server to get the download key
        """
        Logger.debug_print("Getting download key")
        # call the API from tenant server to get the download key
        return "1234"
    def send_success_install(client_id,tenant_id,client_secret):
        """
        Call the API from tenant server to send the success install
        """
        Logger.debug_print("Sending success install")
        # call the API from tenant server to send the success install
        return True

class RunJob():
    """
    Class to run the job assigned to this computer and manage the job
    Type: Controller
    Relationship: LOCAL_JOB 1..1
    """
    job_pending = False # Has a job been missed or been queued
    leave = False # triggers internal exit
    thread = None # thread that is running the job
    stop_job_var = False # stop the job
    kill_job_var = False # stop the job
    job_running_var = False

    def run(self):
        """
        Runs the backup job. This is the main function that runs the backup job
        the backup job is run by filling the LOCAL_JOB with the job assigned to this computer
        The LOCAL_JOB may be assigned using the API class to get it from the tenant server API
        @param: self
        """
        while self.leave is False: # As long as the job is not terminated
            if self.kill_job_var is True:
                # stop the job
                self.kill_job_var = False
                self.job_pending = False
                command = "wbadmin stop job -quiet"
                Logger.debug_print("Kill the Job here by running powershell script")
                p = subprocess.Popen(['powershell.exe', command])
                time.sleep(10)
                p.kill()
                self.job_running_var = False
                # set job status to killed

            elif self.job_pending is True and self.stop_job_var is False : # Run the job if a job is pending. If the job is not stopped state
                # run the job
                self.job_pending = False # set job pending to false since it was just run
                command='wbadmin start backup -backupTarget:\\\\192.168.2.201\\backups -include:C: -allCritical -vssFull -quiet -user:tenant\\Danny -password:'
                p=subprocess.Popen(['powershell.exe', command])
                time.sleep(10)
                p.kill()

                # set job status to running
                self.job_running_var = True # set job running to true

            time.sleep(5)
            Logger.debug_print("Check backup status schedule here and run accordingly")
            # check if time has passed since it should have run
            if LOCAL_JOB.settings.start_time is None or LOCAL_JOB.settings.stop_time is None:
                LOCAL_JOB.settings.start_time = ""
                LOCAL_JOB.settings.stop_time = ""
            if (LOCAL_JOB.settings.start_time < time.asctime()) and (LOCAL_JOB.settings.stop_time > time.asctime()):
                Logger.debug_print("Job Triggered by time")
                command='wbadmin start backup -backupTarget:\\\\192.168.2.201\\backups -include:C: -allCritical -vssFull -quiet -user:tenant\\Danny -password:'
                p = subprocess.Popen(['powershell.exe', command])

                time.sleep(10)
                p.kill()
                # Run the Job


    def __init__(self):
        self.thread = threading.Thread(target=self.run)
        self.thread.daemon = True
        self.thread.start()
    def trigger_job(self):
        """
        Triggers the job to run
        """
        self.job_pending = True
    def enable_job(self):
        """
        Enables the job to run
        """
        self.stop_job_var = False
    def stop_job(self):
        """
        Sets the job_stop state to True thus stopping the job.
        This does not stop active jobs. It will only prevent new jobs from being started.
        """
        self.stop_job_var = True
    def kill_job(self):
        """
        Stops the currently running job
        """
        # stop the job
        self.kill_job_var = True

class InitSql():
    """
    Initialized SQL information files. This includes
    Type: File IO
    Relationship: NONE
    
    1. job_settings
    2. config
    3. job
    4. logs
    5. settings

    Use this at the beginning of the program to ensure all tables
    are created when at the beginning rather then runtime

    """

    def job_settings():
        """
        ensure job settings file exists and the table is created
        """
        create_db_file(settingsDirectory,job_settingsFile)
        # create settings table
        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS job_settings
                            (ID TEXT, schedule TEXT, startTime TEXT, stopTime TEXT, retryCount TEXT, sampling TEXT, retention TEXT, lastJob TEXT, notifyEmail TEXT, heartbeatInterval TEXT)''')
        # Close connection
        conn.commit()
        conn.close()

    def config_files():
        """ 
        Ensure config file exists and the table is created
        """
        create_db_file(settingsDirectory,configFile)
           # create log table
        conn = sqlite3.connect(settingsDirectory+configFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS config
                                    (ID TEXT, tenantSecret TEXT, Address TEXT)''')
        # close connection
        conn.commit()
        conn.close()

    def job_files():
        """
        Ensure job file exists and the table is created
        """
        create_db_file(settingsDirectory,jobFile)
           # create log table
        conn = sqlite3.connect(settingsDirectory+jobFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS job
                                    (ID TEXT, Title TEXT, created TEXT, configID TEXT, settingsID TEXT)''')
        # close connection
        conn.commit()
        conn.close()

    def log_files():
        """
        Ensure log file exists and the table is created
        """
        create_db_file(logdirectory,logpath)
           # create log table
        conn = sqlite3.connect(logdirectory+logpath)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS logs
                                    (severity TEXT, subject TEXT, message TEXT, code TEXT, date TEXT)''')
        # close connection
        conn.commit()
        conn.close()

    def settings():
        """
        Ensure settings file exists and the table is created
        """
        create_db_file(settingsDirectory,"\\Settings.db")
        # create settings table
        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS settings
                            (setting TEXT, value TEXT)''')
        # Close connection
        conn.commit()
        conn.close()


    def __init__(self):
        # initialize all tables when the object is created
        InitSql.log_files()
        InitSql.settings()
        InitSql.job_files()
        InitSql.config_files()
        InitSql.job_settings()

def create_db_file(directory,path):
    """
    create the database file if it does not exist and the folder for it
    """
    # ensure ../ logs directory exists
    if not os.path.exists(os.path.join(directory)):
        os.makedirs(os.path.join(directory))
    # if file does not exist create it
    if not os.path.exists(directory+path):
        if path=="":
            conn = sqlite3.connect(directory)
        elif directory=="" :
            conn = sqlite3.connect(path)
        else:
            conn = sqlite3.connect(directory+path)
        conn.close()

def get_client_info():
    """
    Used to pull information from database and pull remaining information from the server
    """
    global CLIENT_ID
    global TENANT_ID
    global TENANT_PORTAL_URL
    client_id_set = False
    tenant_id_set = False
    tenant_portal_url_set = False
    settings_dict = {}
    logger = Logger()
    #ensure settings files are ready
    InitSql.settings()

    #create connection
    try:
        conn = sqlite3.connect(SETTINGS_PATH)
        settings_df = pd.read_sql_query("SELECT * FROM settings", conn)
        conn.close()
    except FileNotFoundError:
        logger.log("ERROR", "get_client_info", "Settings file not found", "1003", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        logger.log("ERROR", "get_client_info", "General Error getting settings", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

    # Append each setting and value to a dictionary
    for row in settings_df.iterrows():
        setting = row[0]
        value = row[1]
        settings_dict[setting] = value

    # Assign the values to the global variables
    # get client ID
    try:
        CLIENT_ID = settings_dict.get('CLIENT_ID', -1)
        client_id_set = True
    except KeyError:
        CLIENT_ID = -1
        logger.log("ERROR", "get_client_info", "CLIENT_ID not found in settings", "1001", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        CLIENT_ID = -1
        logger.log("ERROR", "get_client_info", "General Error getting CLIENT_ID from settings", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

    # get tenant ID
    try:
        TENANT_ID = settings_dict.get('TENANT_ID', -1)
        tenant_id_set = True
    except KeyError:
        TENANT_ID = -1
        logger.log("ERROR", "get_client_info", "TENANT_ID not found in settings", "1001", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        TENANT_ID = -1
        logger.log("ERROR", "get_client_info", "General Error getting TENANT_ID from settings", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

    # Get tenant portal
    try:
        TENANT_PORTAL_URL = settings_dict.get('TENANT_PORTAL_URL', "https://nexum.com/tenant_portal")
        tenant_portal_url_set = True
    except KeyError:
        TENANT_PORTAL_URL = "https://nexum.com/tenant_portal"
        logger.log("ERROR", "get_client_info", "TENANT_PORTAL_URL not found in settings", "1001", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        TENANT_PORTAL_URL = "https://nexum.com/tenant_portal"
        logger.log("ERROR", "get_client_info", "General Error getting TENANT_PORTAL_URL from settings", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

    API.get_job()

    # call API from A3 to get the rest of the information that wasnt set
    if client_id_set:
        CLIENT_ID = API.get_client_id()
    if tenant_id_set:
        TENANT_ID = API.get_tenant_id()
    if tenant_portal_url_set:
        TENANT_PORTAL_URL = API.get_tenant_portal_url()

def save_client_info():
    """
    Save the client info to the settings database
    """

    #ensure settings files are ready
    InitSql.settings()

    #create connection
    conn = sqlite3.connect(SETTINGS_PATH)
    cursor = conn.cursor()

    # Insert the client_id, TENANT_ID, and TENANT_PORTAL_URL and Polling interval into the settings table
    cursor.execute("INSERT INTO settings (setting, value) VALUES ('client_id', ?)", (CLIENT_ID,))
    cursor.execute("INSERT INTO settings (setting, value) VALUES ('TENANT_ID', ?)", (TENANT_ID,))
    cursor.execute("INSERT INTO settings (setting, value) VALUES ('TENANT_PORTAL_URL', ?)", (TENANT_PORTAL_URL,))
    cursor.execute("INSERT INTO settings (setting, value) VALUES ('POLLING_INTERVAL', ?)", (POLLING_INTERVAL,))

    #write local job to database


    # Close the connection
    conn.commit()
    conn.close()

def logs():
    """ 
    Writes logs to the users downloads folder
    """
    logger = Logger()
    current_user = os.getlogin()
    file_path = f"C:\\Users\\{current_user}\\Downloads\\nexumlog.csv" # download logs to CSV file in users downloads
    InitSql.log_files()
    try:
        # Connect to the logs database
        conn = sqlite3.connect(logdirectory+logpath)
        # Query all logs from the logs table
        query = "SELECT * FROM logs"
        logs_df = pd.read_sql_query(query, conn)

        # Close the database connection
        conn.close()

        # Print the logs to csv
        logs_df.to_csv(file_path, index=False)
    except FileNotFoundError:
        logger.log("ERROR", "logs", "Log file not found", "1003", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        logger.log("ERROR", "logs", "General Error getting logs", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

def tenant_portal():
    """
    Opens the tenant portal in the default web browser
    """
    os.system(f"start {TENANT_PORTAL_URL}")

class Logger():
    """
    Logger class to log messages to the database file
    Type: File IO
    Relationship: NONE
    """
    conn = None
    cursor = None

    # create the database file if it does not exist and connect to it then create the table
    def __init__(self):
        #ensure log files are ready
        InitSql.log_files()

        #create connection
        self.conn = sqlite3.connect(logdirectory+logpath)
        self.cursor = self.conn.cursor()

    # log a message to the database
    def log(self, severity, subject, message, code, date):
        """
        Information
        """
        self.cursor.execute('''INSERT INTO logs (severity, subject, message, code, date)
                    VALUES (?, ?, ?, ?, ?)''', (severity, subject, message, code, date))
        self.conn.commit()

    def debug_print(message):
        """
        Used to print information in debugging to be easily switched during implementation
        @param message: the message to be printed
        """
        print(message)

class IconManager():
    """
    Class to manage the tray icon
    Type: Controller
    @param image: the path to the image to be used for the icon
    @param menu: the menu to be used for the icon
    @param title: the title of the icon
    @param l: the logger
    """
    logger = Logger()
    # change the status of the job every 5 seconds
    def change_status(self):
        """
        Changes the status of the job every 5 seconds
        """
        l = Logger()
        while True :
            time.sleep(POLLING_INTERVAL)
            status = IconManager.get_status()
            percent = IconManager.get_percent()
            version = IconManager.get_version()
            menu = IconManager.create_menu(status,percent,version ,logs, tenant_portal)
            l.log("INFO", "change_status", "Status changed to "+str(status) + ":"+str(percent)+ ":"+str(version), "0", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            self.update_menu(menu)

    # stop the tray icon
    def stop(self):
        """
        Stops the icon
        """
        self.icon.stop()
    # actually run the icon
    def run_icon(self):
        """
        opens a thread and starts the icon
        """
        self.icon.run()
    # update currently running menu
    def update_menu(self,menu):
        """
        updates the menu of the icon
        """
        self.icon.menu = menu
        self.icon.update_menu()

    # open a thread and start the icon
    def run(self):
        """
        runs the icon
        """
        thread = threading.Thread(target=self.run_icon)
        thread2 = threading.Thread(target=self.change_status)
        thread.start()
        thread2.start()
    #change the image
    def set_image(self, image):
        """
        changes the image of the icon
        """
        self.icon.icon = image

    # create a menu from the given parameters
    # pylint: disable=no-self-argument
    def create_menu(status, percent, version, logs_function,tenant_portal_function):
        """
        Creates a menu that can be used from the menu paramaters
        """
        item1 = pystray.MenuItem("Status: "+status, lambda: None)
        item2 = pystray.MenuItem("Percent: "+str(percent), lambda: None)
        item3 = pystray.MenuItem("Version: "+version, lambda: None)
        item4 = pystray.MenuItem("Click to download logs", logs_function)
        item5 = pystray.MenuItem("Tenant Portal", tenant_portal_function)
        return (item1, item2, item3, item4,item5)
    #pylint: enable=no-self-argument

    # initialize the IconManager
    def __init__(self, image, menu, title,l):
        self.logger = l
        image = Image.open(image)
        self.icon = pystray.Icon(title, image, title, menu)

    # These 3 calls should get the status of the job assigned to this computer,
    # the percent complete, and the version of the program
    # pylint: disable=no-method-argument, disable=no-self-argument, disable=missing-function-docstring
    def get_status():
        return API.get_status()
    def get_percent():
        return API.get_percent()
    def get_version():
        return API.get_version()
    # pylint: enable=no-method-argument, enable=no-self-argument, enable=missing-function-docstring

class FlaskServer():
    """
    Class to manage the server
    """
    app = Flask(__name__)
    @staticmethod
    def auth(recieved_client_secret, logger,id):
        """     This is substituted with local clientSecret
        try:
            # open sql connection to 'NEXUM-SQL' and select * from Security where ID = id
            conn = pyodbc.connect('DRIVER={SQL Server};SERVER=NEXUM-SQL;DATABASE=your_database_name;Trusted_Connection=yes;')
            cursor = conn.cursor()
            query = "SELECT * FROM Security WHERE ID = ?"
            cursor.execute(query, (ID,))
            result = cursor.fetchone()

            # set salt to the result[0], pepper to result[1], and salt2 to result[2]
            salt = result[0]
            pepper = result[1]
            salt2 = result[2]

            # close the sql connection
            conn.close()
        except:
            return "405 Incorrect ID"

        """
        # A2 uses client secret unhashed to generate the password used to encrypt the data
        # The data is given salt, pepper, and salt2 then hashed.
        # the hash is then encypted then sent

        # to decrypt would be
        # decrypt the data
        # create a variable with the salt, pepper, and salt2 added to the client secret then hashed
        # compare the hash to the decrypted data
        # if they match the KEY is valid
        recieved_client_secret = Security.decrypt_client_secret(recieved_client_secret)

        temp = Security.sha256_string(CLIENT_SECRET)
        temp = Security.add_salt_pepper(temp, "salt", "pepricart", "salt2")


        if str(recieved_client_secret) != temp:
            logger.log("ERROR", "get_files", "Access denied", "405", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            return 405
        return 200





    # GET ROUTES
    @app.route('/get_files', methods=['GET'], )
    @staticmethod
    def get_files():
        """
        Server requests a path such as C: and returns a list of files and directories in that path
        requirement: json Body that includes 'path', clientSecret hashed with sha256, and a salt, pepper, and salt2. It must also be encrypted with the pre-determined password and the ID for the salt, pepper, and salt2 is required
        returns: a list of files and directories in the path
        if the clientSecret is incorrect returns "401 Access Denied"
        if the ID is incorrect returns "405 Incorrect ID"
        if the path is not a directory returns "404 Path is not a directory"
        if the path is not accessible returns "404 Path is not accessible"
        if the path is empty returns "404 Path is empty"
        if the path is not found returns "404 Path not found"
        else return 500 internal server error
        @param request: the request from the client
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the path from the json body
        path = data.get('path', '')
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        # pylint: enable=unused-variable, enable=invalid-name
        # check if the path exists
        elif not os.path.exists(path):
            logger.log("ERROR", "get_files", "Path not found", "404", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            code=404
            msg="Incorrect path"
        # check if the path is a directory
        elif not os.path.isdir(path):
            logger.log("ERROR", "get_files", "Path is not a directory", "404", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            code=404
            msg="Path is not a directory"
        # check if the path is accessible
        elif not os.access(path, os.R_OK):
            logger.log("ERROR", "get_files", "Path is not accessible", "404", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            code=404
            msg="Path is not accessible"
        # check if the path is empty
        elif not os.listdir(path):
            logger.log("ERROR", "get_files", "Path is empty", "404", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            code=404
            msg="Path is empty"
        # get the files and directories in the path
        # if error is returned, do not get file directories
        if code==0:
            try:
                files = os.listdir(path)
            except PermissionError:
                logger.log("ERROR", "get_files", "Permission error", "1005", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                code = 401
                msg = "Access Denied"
            except:
                logger.log("ERROR", "get_files", "General Error getting files", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                code= 500
                msg = "Internal server error"
        else:
            return make_response(msg, code)
        # <-- Turn into a method

        return files



    # POST ROUTES

    @app.route('/start_job', methods=['POST'], )
    @staticmethod
    def start_job():
        """
        Triggers the RunJob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.trigger_job()
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @app.route('/stop_job', methods=['POST'], )
    @staticmethod
    def stop_job():
        """
        Triggers the stopjob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.stop_job()
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @app.route('/kill_job', methods=['POST'], )
    @staticmethod
    def kill_job():
        """
        Triggers the killjob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()

        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.kill_job()
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @app.route('/enable_job', methods=['POST'], )
    @staticmethod
    def enable_job():
        """
        Triggers the RunJob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.enable_job()
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)



   





    # PUT ROUTES





    # HELPERS







    def run(self):
        """
        Runs the server
        """
        self.app.run()
    def __init__(self):
        Logger.debug_print("flask server started")
        self.run()

class Security():
    """
    Class to manage security
    Type: Security
    Relationship: NONE
    """

    @staticmethod
    def split_string(string):
        """
        split a string in half with right side majority
        """
        length = len(string)
        half_length = length // 2
        right_side = str(string)[half_length:]
        left_side = str(string)[:half_length]
        return left_side, right_side
    #sha256 a string
    @staticmethod
    def sha256_string(string):
        """
        SHA256 a string
        """
        # Convert the string to bytes
        string_bytes = str(string).encode('utf-8')

            # Compute the SHA-256 hash
        sha256_hash = hashlib.sha256(string_bytes).hexdigest()

        return sha256_hash

    # encrypt a string using AES
    @staticmethod
    def encrypt_string(password, string):
        """
        Encrypt a string with AES-256 bit encryption
        """
        # Pad the password to be 16 bytes long
        password_hashed = str(password).ljust(16).encode('utf-8')

        # Create a new AES cipher with the password as the key
        cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
        encryptor = cipher.encryptor()

        # Pad the string to be a multiple of 16 bytes long
        string = string.ljust((len(string) // 16 + 1) * 16).encode('utf-8')

        # Encrypt the string using AES
        encrypted_string = encryptor.update(string) + encryptor.finalize()

        # Encode the encrypted string in base64
        encoded_string = base64.b64encode(encrypted_string)

        return encoded_string.decode('utf-8')

    # decrypt a string using AES
    @staticmethod
    def decrypt_string(password, string):
        """
        Decrypt a string with AES-256 bit decryption
        """
        l = Logger()
        # Pad the password to be 16 bytes long
        password_hashed = str(password).ljust(16).encode('utf-8')

        # Create a new AES cipher with the password as the key
        cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
        decryptor = cipher.decryptor()

        # Decode the string from base64
        decoded_string = base64.b64decode(string)

        # Decrypt the string using AES
        decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()
        try:
            return decrypted_string.decode('utf-8')
        except UnicodeDecodeError:
            l.log("ERROR", "decrypt_string", "Decryption failed", "1004", time.asctime())
            return "Decryption failed"
        except:
            l.log("ERROR", "decrypt_string", "General Error decrypting string", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            return "Decryption failed"

    # add salt, pepper, and salt2 to the string
    @staticmethod
    def add_salt_pepper(string="", salt="", pepper="", salt2=""):
        """
        Adds salt, pepper, and salt2 to the string in a specific way
        """
        salt = salt[-1] + salt[1:-1] + salt[0]  # Swap first and last letter of salt
        pepper = pepper[:-2] + pepper[-1] + pepper[-2]
        temp = salt2[:2]  # Save the first 2 letters of salt2
        salt2 = pepper[:2] + salt2[2:]  # Write the first two letters of pepper to salt2
        pepper = temp[:2] + pepper[2:]  # Copy the temp values to the first 2 letters of pepper

        string_return = Security.split_string(string)


        return salt + string_return[0] + pepper +string_return[1]+ salt2
    # remove salt, pepper, and salt2 from the string
    @staticmethod
    def remove_salt_pepper(string, salt, pepper, salt2):
        """
        Removes salt, pepper, and salt2 from the string in a specific way
        """
        salt = salt[-1] + salt[1:-1] + salt[0]
        pepper = pepper[:-2] + pepper[-1] + pepper[-2]
        temp = salt2[:2]
        salt2 = pepper[:2] + salt2[2:]
        pepper = temp[:2] + pepper[2:]

        # remove salt from front of string
        string2 = str(string)[len(salt):]
        # remove all trailing whitespace
        string3 = str(string2).rstrip()
        #remove salt2 from end of string
        string4 = str(string3)[:-len(salt2)]
        # find pepper in the remaining string and remove it
        pepper_index = str(string4).find(pepper)
        string5 = string4[:pepper_index] + string4[pepper_index+len(pepper):]
        return string5

    # decrypt the clientSecret
    # clientSecret is the global clientSecret
    # given clientSecret is 32 characters long set the password to [0][31][1][30][2][29]...[15][16]
    @staticmethod
    def decrypt_client_secret(client_secret_in):
        """
        Uses a specific sequence to decrypt the client secret
        """
        password = ""
        for i in range(16):
            password += CLIENT_SECRET[i] + CLIENT_SECRET[31-i]
        try:
            return Security.decrypt_string(password, client_secret_in).strip()
        except ValueError:
            l = Logger()
            l.log("ERROR", "decrypt_client_secret", "Decryption failed", "1004", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            return "Decryption failed"
        except:
            return ""
    # uses plaintext clientSecret to encrypt the clientSecret
    # clientSecret is the global clientSecret
    # given clientSecret is 32 characters long set the password to [0][31][1][30][2][29]...[15][16]
    @staticmethod
    def encrypt_client_secret(client_secret_in):
        """
        Uses a specific sequence to encrypt the client secret
        """
        password = ""
        for i in range(16):
            password += CLIENT_SECRET[i] + CLIENT_SECRET[31-i]
        return Security.encrypt_string(password, client_secret_in)

def first_run(arg):
    """
    The first run function checks if this program has been run before. 
    If it has not been run before it will create a registry entry and 
    call the API to send the success install

    :param arg: The download key
    :return: True if the first run is successful, False otherwise
    """

    # call API.get_download_key
    download_key = API.get_download_key()
    if download_key != arg :
        return False
        # create registry entry Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Nexum\A2
    try:
        get_client_info()
        save_client_info()
        key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Nexum")
        winreg.CloseKey(key)
        key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\\Nexum\\Client")
        winreg.CloseKey(key)
        API.send_success_install(CLIENT_ID,TENANT_ID,CLIENT_SECRET)
        return True
    except FileNotFoundError:
        print(traceback.format_exc())
        return False
    except PermissionError:
        print(traceback.format_exc())
        return False
    except:
        print(traceback.format_exc())
        return False

def check_first_run(arg):

    """
    The check_first_run function checks if this program has been run before.
    It is a helper function for the first_run function
    """
    l = Logger()
    # check for registry entry Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Nexum\A2 to see if this is the first run if it exists return, else call first_run()
    try:
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\\Nexum\\Client")
        winreg.CloseKey(key)
        return True
    except FileNotFoundError:
        return first_run(arg)
    except PermissionError:
        l.log("ERROR", "check_first_run", "Permission Error checking registry", "1005", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        l.log("ERROR", "check_first_run", "General Error checking registry", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    return False

@main_requires_admin
def main():
    """
    Main method of the program for testing and starting the program
    """
    global CLIENT_SECRET
    CLIENT_SECRET ="ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD" # used to ensure proper secret testing would be given from a USB install then setting files

    # check if this is the first run
    check_first_run("1234")
    # create a Logger
    l = Logger()
    # init databases
    InitSql()
    # get client info
    get_client_info()
    # create the IconManager
    i = IconManager(image_path, IconManager.create_menu(IconManager.get_status(), IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum",l)
    # run the icon
    i.run()
    # log a message
    l.log("INFO", "Main", "Main has started", "000", time.asctime())
    # run the job
    global RUN_JOB_OBJECT
    RUN_JOB_OBJECT = RunJob()

    # run server to listen for requests
    f = FlaskServer()

if __name__ == "__main__":
    print(time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    main()
