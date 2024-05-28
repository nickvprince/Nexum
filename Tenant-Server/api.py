"""
# Program: Tenant-server
# File: api.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the API class. This class is used 
# to interact with the tenant-server REST API it is a connector.

# Class Types: 
#               1. API - Connector

"""
# pylint: disable= import-error, unused-argument
from logger import Logger
from client import Client
import datetime
import requests
from sql import MySqlite
import re
client:Client = MySqlite.get_client(1)
class API():
    """
    Class to interact with the API. Used for local API calls and 
    easy integration when API calls are changed
    Type: Connector
    Relationship: NONE
    """

    @staticmethod
    def get_tenant_portal_url():
        """
        Gets the tenant portal URL from the tenant server device
        """
        Logger.debug_print("Getting tenant portal url")
        # call the API from tenant server to get the tenant portal URL
        return "https://nexum.com/tenant_portal"
    @staticmethod
    def get_status():
        """
        Call the API from tenant server to get the status of the client
        """
        MySqlite.write_log("INFO","API","Getting status","0",datetime.datetime.now())
        client:Client = MySqlite.get_client(MySqlite.read_setting("CLIENT_ID"))
        if client == None:
            MySqlite.write_log("ERROR","API","Client not found","0",datetime.datetime.now())
            return "not running"
        else:
            return client[4]
    @staticmethod
    def get_percent():
        """
        call the API from tenant server to get the percent complete of the job
        """
        MySqlite.write_log("INFO","API","Getting percent","0",datetime.datetime.now())

        url = 'http://127.0.0.1:5004/get_status'

        headers = {
            "Content-Type": "application/json"
        }
        try:
            response = requests.get(url, headers=headers,timeout=40)
            data = response.json()
            result = data["result"]
            if "copied" in result:
                    # Find the copied (xxx%) in the result string
                    match = re.search(r'copied \((\d+)%\)', result)
                    if match:
                        percent = int(match.group(1))
                        return percent
                    else:
                        return "0%"
            else:
                return "0%"
        except Exception as e:
            MySqlite.write_log("ERROR","API","Error getting percent","0",datetime.datetime.now())
            return "0%"
    @staticmethod
    def get_version():
        """
        Call the API from tenant server to get the version of the program
        """
        MySqlite.write_log("INFO","API","Getting status","0",datetime.datetime.now())
        version = MySqlite.read_setting("version")
        if version == None:
            MySqlite.write_log("ERROR","API","Version not found","0",datetime.datetime.now())
            return "n/a"
        else:
            return version
    @staticmethod
    def get_job():
        """
        Call the API from tenant server to get the job assigned to this computer
        """
        Logger.debug_print("Getting job")
        # call the API from tenant server to get the job assigned to this computer
        return "backup"
    @staticmethod
    def get_client_id():
        """
        Call the API from tenant server to get the client id
        """
        Logger.debug_print("Getting client id")
        # call the API from tenant server to get the client id
        return 1
    @staticmethod
    def get_tenant_id():
        """
        Call the API from tenant server to get the tenant id
        """
        Logger.debug_print("Getting tenant id")
        # call the API from tenant server to get the tenant id
        return 1
    @staticmethod
    def get_download_key():
        """
        call the API from tenant server to get the download key
        """
        Logger.debug_print("Getting download key")
        # call the API from tenant server to get the download key
        return "1234"

    @staticmethod
    def send_success_install(client_id,tenant_id,client_secret):
        """
        Call the API from tenant server to send the success install
        """
        Logger.debug_print("Sending success install")
        # call the API from tenant server to send the success install
        return True

    @staticmethod
    def post_missing_heartbeat(client_id,tenant_id):
        """
        Call the API from tenant server to post the missing heartbeat
        """
        Logger.debug_print("Posting missing heartbeat")
        # call the API from tenant server to post the missing heartbeat
        return True

    @staticmethod
    def get_update_available():
        """
        Call the API from tenant server to get the update available
        """
        Logger.debug_print("Getting update available")
        # call the API from tenant server to get the update available
        return True

    @staticmethod
    def get_update_path():
        """
        Call the API from tenant server to get the update path
        """
        Logger.debug_print("Getting update path")
        # call the API from tenant server to get the update path
        return "https://nexum.com/tenant_portal?update=1.27.4"
