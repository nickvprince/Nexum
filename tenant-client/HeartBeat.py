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
import time
from MySqlite import MySqlite
import requests
from logger import Logger
import threading
from security import Security
#pylint: disable=bare-except,global-statement
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

        client_secret = MySqlite.read_setting("client_secret")
        client_id = MySqlite.read_setting("CLIENT_ID")
        temp = Security.sha256_string(client_secret)
        temp = Security.add_salt_pepper(temp, MySqlite.read_setting("salt"), MySqlite.read_setting("pepper"), MySqlite.read_setting("salt2"))
        temp = Security.encrypt_client_secret(temp)
        headers = {
            "secret": str(temp),
            "id": str(client_id)
        }
        url = "http://"+self.server_address+":"+self.server_port+"/beat"
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
            logger.log("High","HeartBeat","Failed to send heartbeat to server","1006",time.time())




    def __init__(self):
        # open thread to ping server indefinitely
        self.server_address = MySqlite.read_setting("server_address")
        self.server_port = MySqlite.read_setting("server_port")
        try:
            self.interval = MySqlite.read_setting("heartbeat_interval")
        except:
            self.interval = 5
            MySqlite.write_setting("heartbeat_interval",5)
        if self.interval is None or self.interval == '':
            self.interval = 5
            MySqlite.write_setting("heartbeat_interval",5)
        try:
            self.last_heartbeat = MySqlite.read_setting("last_heartbeat")
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
