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

import time
import requests
from MySqlite import MySqlite

TIMEFORMAT = "%Y-%m-%dT%H:%M:%S.%m"

# SETTINGS
APIKEY_SETTING = "apikey"
CLIENT_ID_SETTING = "CLIENT_ID"
UUID_SETTING = "uuid"
SERVER_ADDRESS_SETTING = "server_address"
SERVER_PORT_SETTING = "server_port"
LOG_ROUTE = "/log"
TENANT_PROTOCOL = "http://"

@staticmethod
def convert_type(log_type):
    """
    Convert a string to a type
    """
    if log_type == "Trace":
        return 0
    elif log_type == "Debug":
        return 1
    elif log_type == "INFO":
        return 2
    elif log_type == "warning":
        return 3
    elif log_type == "Error":
        return 4
    elif log_type == "Critical":
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
        date = time.strftime(TIMEFORMAT, time.localtime())
        MySqlite.write_log(severity, subject, message, code, date)
        header ={
            "Content-Type": "application/json",
            "apikey": MySqlite.read_setting(APIKEY_SETTING),
        }
        content = {
            "client_id": MySqlite.read_setting(CLIENT_ID_SETTING),
            "uuid": MySqlite.read_setting(UUID_SETTING),
            "severity": severity,
            "function": subject,
            "message": message,
            "code": code,
            "time": date,
            "file": filename,
            "alert": alert
        }
        try:
            response = requests.post(f"{TENANT_PROTOCOL}{MySqlite.read_setting(SERVER_ADDRESS_SETTING)}:{MySqlite.read_setting(SERVER_PORT_SETTING)}{LOG_ROUTE}", headers=header, json=content,timeout=5,verify=False)
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
