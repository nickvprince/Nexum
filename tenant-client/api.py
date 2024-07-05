"""
# Program: Tenant-Client
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
# pylint: disable=import-error, unused-argument
from logger import Logger
from MySqlite import MySqlite
import requests
from security import Security
import re
import datetime
from job import Job
import helperfunctions

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
        return MySqlite.read_setting("TENANT_PORTAL_URL")
    @staticmethod
    def get_status():
        """
        Call the API from tenant server to get the status of the client
        """
        return MySqlite.read_setting("Status")
    @staticmethod
    def get_percent():
        """
        call the API from tenant server to get the percent complete of the job
        """
        # call 127.0.0.1:5004/get_status
        apikey = MySqlite.read_setting("apikey")
        CLIENT_ID = MySqlite.read_setting("CLIENT_ID")

        url = MySqlite.read_setting("service_address")+'/get_status'

        headers = {
            "Content-Type": "application/json",
            "apikey": str(apikey),
            "id": str(CLIENT_ID)
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
                                # set job status to idle
                new_client =MySqlite.write_setting("status","idle")
                return "0%"
        except Exception:
            MySqlite.write_log("ERROR","API","Error getting percent","0",datetime.datetime.now())
            new_client = MySqlite.read_setting("CLIENT_ID")
            if new_client == None:
                MySqlite.write_log("ERROR","API","Client not found","0",datetime.datetime.now())
                return "0%"
            else:
                MySqlite.write_setting("status","service --offline")
                return "0%"

    @staticmethod
    def get_version():
        """
        Call the API from tenant server to get the version of the program
        """
        return MySqlite.read_setting("version")
    
    @staticmethod
    def get_job():
        """
        Call the API from tenant server to get the job assigned to this computer
        """
        jb:Job = Job()
        jb.load(MySqlite.read_setting("CLIENT_ID"))
        # call the API from tenant server to get the job assigned to this computer
        return jb
    @staticmethod
    def get_client_id():
        """
        Call the API from tenant server to get the client id
        """
        return MySqlite.read_setting("CLIENT_ID")

        url = MySqlite.read_setting("server_address")+'/get_id'
        client_secret = MySqlite.read_setting("client_secret")
        client_id = MySqlite.read_setting("CLIENT_ID")
        temp = Security.sha256_string(client_secret)
        temp = Security.add_salt_pepper(temp, MySqlite.read_setting("salt"), MySqlite.read_setting("pepper"), MySqlite.read_setting("salt2"))
        temp = Security.encrypt_client_secret(temp)
        headers = {
            "Content-Type": "application/json",
            "secret": str(temp),
            "id": str(client_id)
        }
        body = {
            "uuid":helperfunctions.get_uuid()
        }
        try:
            response = requests.get(url, headers=headers,timeout=40)
            data = response.json()
            result = data["id"]
            return result
        except Exception:
            MySqlite.write_log("ERROR","API","Error getting client id","0",datetime.datetime.now())
            return None


    @staticmethod
    def send_success_install(client_id,tenant_id,client_secret):
        """
        Call the API from tenant server to send the success install
        """
        Logger.debug_print("Sending success install")
        # call the API from tenant server to send the success install
        return True

    @staticmethod
    def get_update_available():
        """
        Checks the server for a new update
        """
        Logger.debug_print("Checking Update Status")
        # call the API from tenant server to send the success install
        return True

    @staticmethod
    def get_update_path():
        """
        Checks the server for the update path
        """
        Logger.debug_print("Getting Update Path")
        # call the API from tenant server to send the success install
        return True

    @staticmethod
    def get_polling_interval():
        """
        Checks the server for a polling interval
        """
        Logger.debug_print("Checking Polling Interval")
        # call the API from tenant server to send the success install
        return True
