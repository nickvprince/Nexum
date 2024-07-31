"""
Tests the API.py file
"""

import unittest
import sys
import os
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from api import API
from sql import MySqlite

MSP_ONLINE = False

class TestApi(unittest.TestCase):
    """
    Tests the API.py file
    """

    def test_post_missing_heartbeat_server(self):
        """
        Tests the testing function
        """
        assert API.post_missing_heartbeat(0) is MSP_ONLINE


    def test_post_missing_heartbeat_client_zero(self):
        """
        Tests the testing function
        """
        assert API.post_missing_heartbeat(1) is MSP_ONLINE


    def test_get_version_1_0_0(self):
        """
        Tests the testing function
        """

        MySqlite.write_setting("version", "1.0.0")
        assert API.get_version() == "1.0.0"

    def test_get_version_2_0_0(self):
        """
        Tests the testing function
        """

        MySqlite.write_setting("version", "2.0.0")
        assert API.get_version() == "2.0.0"

    def test_get_percentage(self):
        """
        Tests the testing function
        """
        assert API.get_percent() == "0%"

    def test_get_status_Online(self):
        """
        Tests the testing function
        """
        MySqlite.write_setting("Status", "Online")
        assert API.get_status() == "Online"
    def test_get_status_ServiceOffline(self):
        """
        Tests the testing function
        """
        MySqlite.write_setting("Status", "ServiceOffline")
        assert API.get_status() == "ServiceOffline"
    

if __name__ == '__main__':
    unittest.main()
