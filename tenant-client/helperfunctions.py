"""
# Program: Tenant-Client
# File: helperfunctions.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains functions to assist with the main program

# Functions
# ---------- Version 1.0
# get_client_info - Pulls information from database and pulls remaining information from the server
# save_client_info - Saves the client info to the settings database
# logs - Writes logs to the users downloads folder
# tenant_portal - Opens the tenant portal in the default web browser
# first_run - The first run function checks if this program has been run before.
# check_first_run - The check_first_run function checks if this program has been run before.

"""
# pylint: disable= import-error, global-statement,unused-argument

import os
import time
import winreg
import traceback
import pandas as pd
from security import CLIENT_SECRET
from api import API
from logger import Logger
from initsql import InitSql,sqlite3,logdirectory,logpath,SETTINGS_PATH
POLLING_INTERVAL = 5 # interval to send the server heartbeats
CLIENT_ID = -1 # client id
TENANT_ID = -1 # tenant id
TENANT_PORTAL_URL = "https://nexum.com/tenant_portal" # url to the tenant portal

# pylint: disable= bare-except
# pylint: disable= global-statement

def check_install_key(key, secret, server, port):
    """
    Check the install key with the server to see 
    if the install is valid
    """
    # check the install key
    return True


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
        logger.log("ERROR", "get_client_info", "Settings file not found",
        "1003", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        logger.log("ERROR", "get_client_info", "General Error getting settings",
        "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

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
        logger.log("ERROR", "get_client_info", "CLIENT_ID not found in settings",
        "1001", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        CLIENT_ID = -1
        logger.log("ERROR", "get_client_info", "General Error getting CLIENT_ID from settings",
        "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

    # get tenant ID
    try:
        TENANT_ID = settings_dict.get('TENANT_ID', -1)
        tenant_id_set = True
    except KeyError:
        TENANT_ID = -1
        logger.log("ERROR", "get_client_info", "TENANT_ID not found in settings",
        "1001", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        TENANT_ID = -1
        logger.log("ERROR", "get_client_info", "General Error getting TENANT_ID from settings",
        "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

    # Get tenant portal
    try:
        TENANT_PORTAL_URL = settings_dict.get('TENANT_PORTAL_URL',
        "https://nexum.com/tenant_portal")
        tenant_portal_url_set = True
    except KeyError:
        TENANT_PORTAL_URL = "https://nexum.com/tenant_portal"
        logger.log("ERROR", "get_client_info", "TENANT_PORTAL_URL not found in settings",
        "1001", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        TENANT_PORTAL_URL = "https://nexum.com/tenant_portal"
        logger.log("ERROR", "get_client_info",
        "General Error getting TENANT_PORTAL_URL from settings",
        "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

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

    # Insert the client_id, TENANT_ID, and TENANT_PORTAL_URL
    # and Polling interval  into the settings table
    cursor.execute("INSERT INTO settings (setting, value) VALUES ('client_id', ?)", (CLIENT_ID,))
    cursor.execute("INSERT INTO settings (setting, value) VALUES ('TENANT_ID', ?)", (TENANT_ID,))
    cursor.execute("INSERT INTO settings (setting, value) VALUES ('TENANT_PORTAL_URL', ?)",
    (TENANT_PORTAL_URL,))
    cursor.execute("INSERT INTO settings (setting, value) VALUES ('POLLING_INTERVAL', ?)",
    (POLLING_INTERVAL,))

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
        logger.log("ERROR", "logs", "Log file not found",
        "1003", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        logger.log("ERROR", "logs", "General Error getting logs",
        "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))

def tenant_portal():
    """
    Opens the tenant portal in the default web browser
    """
    # add check here to ensure starts with https://
    os.system(f"start {TENANT_PORTAL_URL}")

