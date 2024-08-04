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
# pylint: disable= line-too-long,global-statement, bare-except
import datetime
import time
import threading
from sql import InitSql, MySqlite
from api import API


DELAY = 5
DEFAULT_CHECKIN = 15
TIMEFORMAT = "%Y-%m-%d %H:%M:%S"

class HeartBeat:
    """
    Heartbeat class containing the heartbeat logic
    """
    tenant_secret = None
    interval = None
    my_clients = []

    def __init__(self,secret,interval,clients):

        # Ensure DB Is setups
        InitSql.heartbeat()
        # set default info
        self.tenant_secret = secret
        self.interval = interval
        self.my_clients = clients
        # start the daemon thread to check beats
        t1 = threading.Thread(target=self.check_all_checkins)
        t1.daemon = True
        t1.start()


    def check_all_checkins(self):
        """
        --Thread Function--
        Checks all checkins from clients from the sqlite database
        """
        while True:
            # try to prevent thread from crashing on an unknown failure
            try:
                # refresh clients from db
                self.my_clients=MySqlite.load_clients()

                # check all the clients have checked in
                for client in self.my_clients:
                    accepted_time = self.interval * int(MySqlite.get_heartbeat_missed_tolerance(client[0]))
                    # if accepted time is not set default to 15 seconds
                    if int(accepted_time) <= 0 :
                        accepted_time = DEFAULT_CHECKIN
                    # get last checkin and current time
                    last_checkin = MySqlite.get_last_checkin(client[0])
                    current_time = datetime.datetime.now()

                    # if a previous checkin exists
                    if last_checkin:
                        # Parse as a proper time object
                        last_checkin_time = datetime.datetime.strptime(last_checkin.split('.')[0], TIMEFORMAT)

                        # set the time they were supposed to check in by
                        target_time = last_checkin_time + datetime.timedelta(seconds=int(accepted_time))

                        # if they havent checked in by the target time post a missing heartbeat
                        if current_time > target_time:
                            API.post_missing_heartbeat(client[0])
                    else:
                        pass
                time.sleep(DELAY)
            except:
                continue
