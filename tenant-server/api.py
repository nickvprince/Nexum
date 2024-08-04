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
# pylint: disable= import-error, unused-argument, broad-except
import re
import datetime
import requests
from logger import Logger
from client import Client
from sql import MySqlite

#ROUTES
DEVICE_STATUS_ROUTE = "/api/DataLink/Update-Device-Status"
SERVICE_STATUS_ROUTE = "http://127.0.0.1:5004/get_status"

#SETTINGS
APIKEY = "apikey"
CLIENT_ID = "CLIENT_ID"
MSP_SERVER_ADDRESS = "msp_server_address"
MSP_PORT = "msp-port"
VERSION = "version"
STATUS = "Status"

# values
MSP_PROTOCOL = "https://"


client:Client = MySqlite.get_client(1)



class API():
    """
    Class to interact with the API. Used for local API calls and 
    easy integration when API calls are changed
    Type: Connector
    Relationship: NONE
    """
    @staticmethod
    def get_status()->str:
        """
        Call the API from tenant server to get the status of the client
        """
        MySqlite.write_log("INFO","API","Getting status","0",datetime.datetime.now())
        return MySqlite.read_setting(STATUS)

    @staticmethod
    def get_percent()->str:
        """
        call the API from tenant server to get the percent complete of the job
        """
        MySqlite.write_log("INFO","API","Getting percent","0",datetime.datetime.now())

        url = SERVICE_STATUS_ROUTE

        headers = {
            "apikey":MySqlite.read_setting("apikey"),
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
                    return str(percent) + str("%")
                else:
                    return "0%"
            else:
                                # set job status to killed
                new_client = MySqlite.get_client(MySqlite.read_setting(CLIENT_ID))
                new_client = list(new_client)
                new_client.remove(new_client[4])
                new_client.insert(4,"idle")
                MySqlite.update_client(new_client)
                return "0%"
        except Exception:
            MySqlite.write_log("ERROR","API","Error getting percent","0",datetime.datetime.now())
            new_client = MySqlite.get_client(MySqlite.read_setting("CLIENT_ID"))
            if new_client is None:
                MySqlite.write_log("ERROR","API","Client not found","0",datetime.datetime.now())
                return "0%"
            else:
                new_client = list(new_client)
                new_client.remove(new_client[4])
                new_client.insert(4,"service --offline")
                MySqlite.update_client(new_client)
                return "0%"
    @staticmethod
    def get_version()->str:
        """
        Call the API from tenant server to get the version of the program
        """
        MySqlite.write_log("INFO","API","Getting status","0",datetime.datetime.now())
        version = MySqlite.read_setting(VERSION)
        if version is None:
            MySqlite.write_log("ERROR","API","Version not found","0",datetime.datetime.now())
            return "n/a"
        else:
            return version


    @staticmethod
    def post_missing_heartbeat(client_id)->bool:
        """
        Call the API from tenant server to post the missing heartbeat
        """
        MySqlite.write_log("INFO","API","Posting missing heartbeat","0",datetime.datetime.now())
        header ={
            "Content-Type":"application/json",
            "apikey":MySqlite.read_setting(APIKEY),
        }
        content = {
            "client_id": int(client_id),
            "uuid": MySqlite.get_client_uuid(client_id),
            "status": 0# offline device status

        }

        try:
            server_address = MySqlite.read_setting(MSP_SERVER_ADDRESS)
            msp_port = MySqlite.read_setting(MSP_PORT)


            _ = requests.put(f"{MSP_PROTOCOL}{server_address}:{msp_port}{DEVICE_STATUS_ROUTE}", headers=header, json=content,timeout=5,verify=False)
        except Exception:
            return False
        Logger.debug_print("Posting missing heartbeat")
        # call the API from tenant server to post the missing heartbeat
        return True

    @staticmethod
    def get_update_available()->bool:
        """
        Call the API from tenant server to get the update available
        """
        Logger.debug_print("Getting update available")
        # call the API from tenant server to get the update available
        return True
