"""
# Program: Tenant-Client
# File: logger.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the Logger class. This class is used
# to log messages to the database file

# Class Types: 
#               1. Logger - File IO

"""
# pylint: disable= import-error, unused-argument

from MySqlite import MySqlite
import time
import requests
@staticmethod
def convert_type(type):
    """
    Convert a string to a type
    """
    if type == "Trace":
        return 0
    elif type == "Debug":
        return 1
    elif type == "INFO":
        return 2
    elif type == "warning":
        return 3
    elif type == "Error":
        return 4
    elif type == "Critical":
        return 5
    else:
        return -1
class Logger():

    """
    Logger class to log messages to the database file
    Type: File IO
    Relationship: NONE
    """


    # log a message to the database
    @staticmethod
    def log(severity, subject, message, code, filename,alert=False):
        """
        Information
        """
        date = time.strftime("%Y-%m-%dT%H:%M:%S.%m", time.localtime())
        MySqlite.write_log(severity, subject, message, code, date)
        header ={
            "Content-Type": "application/json",
            "apikey": MySqlite.read_setting("apikey"),
        }
        content = {
            "client_id": MySqlite.read_setting("CLIENT_ID"),
            "uuid": MySqlite.read_setting("uuid"),
            "severity": severity,
            "function": subject,
            "message": message,
            "code": code,
            "time": date,
            "file": filename,
            "alert": alert
        }
        try:
            response = requests.post(f"http://{MySqlite.read_setting('server_address')}:{MySqlite.read_setting('server_port')}/log", headers=header, json=content,timeout=5,verify=False)
            print(response.status_code)
        except Exception as e:
            print(e)

    @staticmethod
    def debug_print(message):
        """
        Used to print information in debugging to be easily switched during implementation
        @param message: the message to be printed
        """
        MySqlite.write_log("DEBUG", "DEBUG", message, "0", "0")
        print(message)
