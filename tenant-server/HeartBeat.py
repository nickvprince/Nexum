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
# pylint: disable= line-too-long,global-statement
import time
import threading
from sql import InitSql, MySqlite
from api import API
import datetime


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
        global MY_CLIENTS
        while True:
            print("Loading clients")
            MY_CLIENTS=MySqlite.load_clients()
            print("Checking all checkins")
            for client in MY_CLIENTS:
                accepted_time = self.interval * int(MySqlite.get_heartbeat_missed_tolerance(client[0]))
                if int(accepted_time) <= 0 :
                    accepted_time = 15
                print("Client: ",client[1])
                print("Cline ID: ",client[0])
                print("Accepted Time: ",accepted_time)
                last_checkin = MySqlite.get_last_checkin(client[0])
                current_time = datetime.datetime.now()
                if last_checkin:
                    last_checkin_time = datetime.datetime.strptime(last_checkin.split('.')[0], "%Y-%m-%d %H:%M:%S")
                    target_time = last_checkin_time + datetime.timedelta(seconds=int(accepted_time))
                    print("Current time:", current_time)
                    print("Last checkin:", last_checkin_time)
                    print("Target time:", target_time)
                    if current_time > target_time:
                        API.post_missing_heartbeat(client[0],self.tenant_secret)
                        print("Heartbeat missed for client:", client[1])
                else:
                    print("No checkin found for client:", client[1])
            # beat to MSP here
            API.server_beat()
            time.sleep(self.interval)
            print(MY_CLIENTS)
