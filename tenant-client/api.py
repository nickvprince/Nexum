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
import re
import datetime
import requests
from MySqlite import MySqlite
from job import Job

# SETTINGS
CLIENT_ID_SETTING = "CLIENT_ID"
TENANT_PORTAL_URL_SETTING = "TENANT_PORTAL_URL"
STATUS_SETTING = "Status"
APIKEY_SETTING = "apikey"
SERVICE_ADDRESS_SETTING = "service_address"
JOB_STATUS_SETTING = "job_status"
VERSION_SETTING = "version"

#GENERAL
TIMEOUT = 5
STATUS_URL = "/get_status"


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
        return MySqlite.read_setting(TENANT_PORTAL_URL_SETTING)
    @staticmethod
    def get_status():
        """
        Call the API from tenant server to get the status of the client
        """
        return MySqlite.read_setting(STATUS_SETTING)
    @staticmethod
    def get_percent():
        """
        call the API from tenant server to get the percent complete of the job
        """
        # call 127.0.0.1:5004/get_status
        apikey = MySqlite.read_setting(APIKEY_SETTING)
        
        headers = {
            "Content-Type": "application/json",
            "apikey": str(apikey),
        }
        try:
            response = requests.get("http://127.0.0.1:5004/get_status", headers=headers,timeout=15)
            data = response.json()
            result = data["result"]
            if "copied" in result:
                # Find the copied (xxx%) in the result string
                match = re.search(r'copied \((\d+)%\)', result)
                if match:
                    percent = int(match.group(1))

                    return str(percent) + str("%")
                else:
                    return "0%"
            else:
                                # set job status to idle
                new_client =MySqlite.write_setting(JOB_STATUS_SETTING,"NotRunning")
                return "0%"
        except Exception as e:
            _ = e
            MySqlite.write_log("ERROR","API","Error getting percent","0",datetime.datetime.now())
            new_client = MySqlite.read_setting("CLIENT_ID")
            if new_client is None:
                MySqlite.write_log("ERROR","API","Client not found","0",datetime.datetime.now())
                return "0%"
            else:
                #MySqlite.write_setting(STATUS_SETTING,"service --offline")
                return "0%"

    @staticmethod
    def get_version():
        """
        Call the API from tenant server to get the version of the program
        """
        return MySqlite.read_setting(VERSION_SETTING)

    @staticmethod
    def get_job():
        """
        Call the API from tenant server to get the job assigned to this computer
        """
        jb:Job = Job()
        jb.load(MySqlite.read_setting(CLIENT_ID_SETTING))
        # call the API from tenant server to get the job assigned to this computer
        return jb
    @staticmethod
    def get_client_id():
        """
        Call the API from tenant server to get the client id
        """
        return MySqlite.read_setting(CLIENT_ID_SETTING)
