"""
Tests the client.py file
"""

import unittest
import sys
import os
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from client import Client

class Testclient(unittest.TestCase):
    """
    Tests the API.py file
    """
    
    def test_get_mac(self):
        """
        Tests the testing function
        """
        client:Client = Client("12:12:12:13","10.10.10.1","Nexum-PC","Online","1")
        test=client.get_mac()
        assert test=="12:12:12:13"

    def test_set_mac(self):
        """
        Tests the testing function
        """
        client:Client = Client("12:12:12:13","10.10.10.1","Nexum-PC","Online","1")
        client.set_mac("12:12:12:14")
        test=client.get_mac()
        assert test=="12:12:12:14"

    def test_get_ip(self):
        """
        Tests the testing function
        """
        client:Client = Client("12:12:12:13","10.10.10.1","Nexum-PC","Online","1")
        test=client.get_ip()
        assert test=="10.10.10.1"

    def test_set_ip(self):
        """
        Tests the testing function
        """
        client:Client = Client("12:12:12:13","10.10.10.1","Nexum-PC","Online","1")
        client.set_ip("10.10.10.2")
        test=client.get_ip()
        assert test=="10.10.10.2"

    def test_get_name(self):
        """
        Tests the testing function
        """
        client:Client = Client("12:12:12:13","10.10.10.1","Nexum-PC","Online","1")
        test=client.get_name()
        assert test=="Nexum-PC"

    def test_set_name(self):
        """
        Tests the testing function
        """
        client:Client = Client("12:12:12:13","10.10.10.1","Nexum-PC","Online","1")
        client.set_name("Nexum-Laptop")
        test=client.get_name()
        assert test=="Nexum-Laptop"

    def test_get_status(self):
        """
        Tests the testing function
        """
        client:Client = Client("12:12:12:13","10.10.10.1","Nexum-PC","Online","1")
        test=client.get_status()
        assert test=="Online"

    def test_set_Status(self):
        """
        Tests the testing function
        """
        client:Client = Client("12:12:12:13","10.10.10.1","Nexum-PC","Online","1")
        client.set_status("Offline")
        test=client.get_status()
        assert test=="Offline"


if __name__ == '__main__':
    unittest.main()
