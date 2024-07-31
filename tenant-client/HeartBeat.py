"""
# Program: Tenant-Client
# File: HeartBeat.py
# Authors: 1. Danny Smith
#
# Date: 3/26/2024
# purpose: 
# This file controls the heartbeats for the tenant-client


# Class Types: 
#               1. HeartBeat - Controller

"""
import threading
import time
from MySqlite import MySqlite
import requests
from logger import Logger

#pylint: disable=bare-except,global-statement

#SETTINGS
APIKEY_SETTING = "apikey"
CLIENT_ID_SETTING = "CLIENT_ID"
TENANT_PORTAL_PROTOCOL = "http://"
BEAT_PATH = "/beat"
SERVER_ADDRESS_SETTING = "server_address"
SERVER_PORT_SETTING = "server_port"
HEARTBEAT_INTERVAL_SETTING = "heartbeat_interval"
LAST_HEARTBEAT_SETTING = "last_heartbeat"

class HeartBeat:
    """
    Heartbeat class containing the heartbeat logic
    """
    server_address = None
    server_port = None
    interval = None
    last_heartbeat = None




    def checkin(self):
        """
        Sends a checkin to the server
        """

        client_secret = MySqlite.read_setting(APIKEY_SETTING)
        client_id = MySqlite.read_setting(CLIENT_ID_SETTING)
        headers = {
            "secret": str(client_secret),
            "id": str(client_id)
        }
        url = TENANT_PORTAL_PROTOCOL+self.server_address+":"+self.server_port+BEAT_PATH
        Logger.debug_print("Sending heartbeat to: "+url)
        Logger.debug_print("Headers: "+headers.keys().__str__()+" "+headers.values().__str__() )
        try:
            response = requests.post(url, headers=headers,timeout=15)
            if response.status_code == 200:
                # Handle success
                Logger.debug_print("Success: Heartbeat sent")
            else:
                # Handle error
                Logger.debug_print("Error: Failed to send heartbeat")
        except:
            logger = Logger()
            logger.log("High","HeartBeat","Failed to send heartbeat to server","1006",
                    "heartbeat.py")




    def __init__(self):
        # open thread to ping server indefinitely
        self.server_address = MySqlite.read_setting(SERVER_ADDRESS_SETTING)
        self.server_port = MySqlite.read_setting(SERVER_PORT_SETTING)
        try:
            self.interval = MySqlite.read_setting(HEARTBEAT_INTERVAL_SETTING)
        except:
            self.interval = 5
            MySqlite.write_setting(HEARTBEAT_INTERVAL_SETTING,self.interval)
        if self.interval is None or self.interval == '':
            self.interval = 5
            MySqlite.write_setting(HEARTBEAT_INTERVAL_SETTING,self.interval)
        try:
            self.last_heartbeat = MySqlite.read_setting(LAST_HEARTBEAT_SETTING)
        except:
            pass
        # Create a daemon thread to run the ping_server method
        thread = threading.Thread(target=self.ping_server)
        thread.daemon = True
        thread.start()
    def ping_server(self):
        """
        Thread Function
        Sends a ping to the server on the server_address and server_port 
        on the specified interval
        """

        while True:
            self.checkin()
            time.sleep(int(self.interval))
