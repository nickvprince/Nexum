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

class HeartBeat:
    """
    Heartbeat class containing the heartbeat logic
    """
    server_address = None
    server_port = None
    tenant_secret = None
    interval = None
    last_heartbeat = None

    def __init__(self):
        # open thread to check all checkins indefinitely
        pass

    def check_all_checkins(self):
        """
        --Thread Function--
        Checks all checkins from clients from the sqlite database
        """
        while True:
        # check all DB and see if any clients have
        # not checked in for 3x the interval
        # if so, set them to inactive
        # notify the msp-server of all inactive clients
        # set notified to true in the table
        # Note. Sending the above counts as this server checking in
            time.sleep(self.interval)
