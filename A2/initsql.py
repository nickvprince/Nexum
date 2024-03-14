"""
Information
"""

import os
import sqlite3
current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
settingsDirectory = os.path.join(current_dir, '..\\settings') # directory for settings
SETTINGS_PATH= os.path.join(current_dir, 
    settingsDirectory+'\\settings.db') # path to the settings database
jobFile=os.path.join('/settings.db')
configFile=os.path.join('/settings.db')
job_settingsFile=os.path.join('/settings.db')
logdirectory = os.path.join(current_dir,'../logs') # directory for logs
logpath = os.path.join('/log.db') # path to the log database
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
    @staticmethod
    def job_settings():
        """
        ensure job settings file exists and the table is created
        """
        create_db_file(settingsDirectory,job_settingsFile)
        # create settings table
        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS job_settings
            (ID TEXT, schedule TEXT, startTime TEXT, stopTime TEXT, 
                retryCount TEXT, sampling TEXT, retention TEXT, lastJob TEXT, 
                    notifyEmail TEXT, heartbeatInterval TEXT)''')
        # Close connection
        conn.commit()
        conn.close()
    @staticmethod
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

    @staticmethod
    def job_files():
        """
        Ensure job file exists and the table is created
        """
        create_db_file(settingsDirectory,jobFile)
           # create log table
        conn = sqlite3.connect(settingsDirectory+jobFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS job
             (ID TEXT, Title TEXT, created TEXT, 
                configID TEXT, settingsID TEXT)''')
        # close connection
        conn.commit()
        conn.close()

    @staticmethod
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

    @staticmethod
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
