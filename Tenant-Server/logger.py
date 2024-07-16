"""
# Program: Tenant-server
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
import time
from sql import MySqlite,InitSql
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

    # create the database file if it does not exist and connect to it then create the table
    def __init__(self):

        #ensure log files are ready
        InitSql.log_files()

    # log a message to the database
    def log(self, severity, Function, message, code,filename,alert=False):
        """
        Information
        """
        date = time.strftime("%Y-%m-%dT%H:%M:%S.%m", time.localtime())
        # post to https://{Mysqlite.read_setting("msp_server_address")}:{Mysqlite.read_setting("msp-port")}/api/DataLink/Log
        header ={
            "Content-Type": "application/json",
            "apikey": MySqlite.read_setting("apikey"),
        }
        content = {
            "client_id": MySqlite.read_setting("CLIENT_ID"),
            "uuid": MySqlite.read_setting("uuid"),
            "type": convert_type(severity),
            "Function": Function,
            "message": message,
            "code": code,
            "time": date,
            "filename": filename
        }
        try:
            response = requests.post(f"https://{MySqlite.read_setting('msp_server_address')}:{MySqlite.read_setting('msp-port')}/api/DataLink/Log", headers=header, json=content,timeout=5,verify=False)
            print(response.status_code)
        except Exception as e:
            print(e)
        

        if alert is True:
            # post to https://{Mysqlite.read_setting("msp_server_address")}:{Mysqlite.read_setting("msp-port")}/api/DataLink/Alert
            header ={
                "Content-Type": "application/json",
                "apikey": MySqlite.read_setting("apikey"),
            }
            content = {
                "client_id": MySqlite.read_setting("CLIENT_ID"),
                "uuid": MySqlite.read_setting("uuid"),
                "type": convert_type(severity),
                "message": message,
                "time": date,
            }
            try:
                response = requests.post(f"https://{MySqlite.read_setting('msp_server_address')}:{MySqlite.read_setting('msp-port')}/api/DataLink/Alert", headers=header, json=content,timeout=5,verify=False)
                print(response.status_code)
            except Exception as e:
                print(e)
        MySqlite.write_log(severity, Function, message, code, date)
    @staticmethod
    def debug_print(message):
        """
        Used to print information in debugging to be easily switched during implementation
        @param message: the message to be printed
        """
        MySqlite.write_log("DEBUG", "DEBUG", message, "0", "0")
        print(message)
