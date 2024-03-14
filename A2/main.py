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
# pylint: disable= undefined-variable
# pylint: disable= no-name-in-module


import time
import traceback
import threading
import os
import base64
import hashlib
import winreg
import pystray
from PIL import Image
from flask import Flask, request,Response,make_response
from pyuac import main_requires_admin
from api import API
from logger import Logger
from initsql import current_dir, SETTINGS_PATH,InitSql, logdirectory, logpath
from runjob import RunJob, LOCAL_JOB
from helperfunctions import get_client_info, save_client_info, logs, tenant_portal,POLLING_INTERVAL
from security import Security, CLIENT_SECRET
# Global variables

image_path = os.path.join(current_dir, '../Data/n.png') # path to the icon image
CLIENT_ID = -1 # client id
TENANT_ID = -1 # tenant id
TENANT_PORTAL_URL = "https://nexum.com/tenant_portal" # url to the tenant portal
CLIENT_SECRET = None # secret for the client to communicate with A2
RUN_JOB_OBJECT = None











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
    global LOCAL_JOB
    from jobsettings import JobSettings
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

    main()
