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
from sql import MySqlite
from security import CLIENT_SECRET

class HeartBeat:
    """
    Heartbeat class containing the heartbeat logic
    """
    server_address = None
    server_port = None
    interval = None
    last_heartbeat = None



    @staticmethod
    def checkin():
        """
        Sends a checkin to the server
        """
        # send heartbeat to server
        # set last heartbeat to now


    def __init__(self):
        # open thread to ping server indefinitely
        self.server_address = MySqlite.read_setting("server_address")
        self.server_port = MySqlite.read_setting("server_port")
        self.interval = MySqlite.read_setting("heartbeat_interval")
        self.last_heartbeat = MySqlite.read_setting("last_heartbeat")

    def ping_server(self):
        """
        --Thread Function--
        Sends a ping to the server on the server_address and server_port 
        on the specified interval
        """
        while True:
            self.checkin()
            time.sleep(self.interval)
