"""
# Program: Tenant-server
# File: HeartBeat.py
# Authors: 1. Danny Smith
#
# Date: 3/26/2024
# purpose: 
# This file controls the heartbeats for the tenant-server


# Class Types: 
#               1. HeartBeat - Controller

"""
import time
import threading
from sql import InitSql
MY_CLIENTS = []
class HeartBeat:
    """
    Heartbeat class containing the heartbeat logic
    """
    tenant_secret = None
    interval = None


    def __init__(self,secret,interval,clients):
        # open thread to check all checkins indefinitely
        InitSql.heartbeat()
        global MY_CLIENTS
        self.tenant_secret = secret
        self.interval = interval
        MY_CLIENTS = clients
        t1 = threading.Thread(target=self.check_all_checkins)
        t1.daemon = True
        t1.start()


    def check_all_checkins(self):
        """
        --Thread Function--
        Checks all checkins from clients from the sqlite database
        """
        while True:
            print("Checking all checkins")
            t = time.time()
            for client in MY_CLIENTS:
                # accepted time = self.interval * get_from_db(interval)
                # if get_from_db last checkin time+accpted time > current time
                # set client to inactive
                pass   
            time.sleep(self.interval)
            print(MY_CLIENTS)
