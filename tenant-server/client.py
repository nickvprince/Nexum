"""
# Program: Tenant-server
# File: client.py
# Authors: 1. Danny Smith
#
# Date: 4/6/2024
# purpose: 
# This file contains the client class. This class is used
# to store client data

# Class Types: 
#               1. client - Struct

"""


class Client():
    """
    Class structure for client data
    """
    mac = ""
    ip = ""
    name = ""
    status = ""
    id = -1

    def __init__(self, mac, ip, name, status, id):
        """
        Constructor for the client class
        """
        self.mac = mac
        self.ip = ip
        self.name = name
        self.status = status
        self.id = id
    # getters
    def get_mac(self):
        """
        Get the mac address
        """
        return self.mac

    def get_ip(self):
        """
        Get the ip address
        """
        return self.ip

    def get_name(self):
        """
        Get the name
        """
        return self.name

    def get_status(self):
        """
        Get the status
        """
        return self.status

    # setters

    def set_mac(self, mac):
        """
        Set the mac address
        """
        self.mac = mac

    def set_ip(self, ip):
        """
        Set the ip address
        """
        self.ip = ip

    def set_name(self, name):
        """
        Set the name
        """
        self.name = name

    def set_status(self, status):
        """
        Set the status
        """
        self.status = status
