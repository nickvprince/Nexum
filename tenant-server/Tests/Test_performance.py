"""
Tests the API.py file
"""

import unittest
import sys
import os
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from api import API
from sql import MySqlite
import time
import requests

APIKEY = "testing"

MSP_ACCEPTED_TIME = 5

TENANT_PORT = 5002
TENANT_SERVER = f"http://localhost:{TENANT_PORT}"
@staticmethod
def test_route(route,method=requests.post)->bool:
    """
    Tests a route, measures the time and returns if it passed or not
    """
    accepted_time = MSP_ACCEPTED_TIME
    # get current time
    start_time = time.time()
    # make post request to MSP to get the URL's
    try:
        headers = {
            'apikey': APIKEY,
            'content-type': 'application/json'
        }
        body = {
            'client_id':'0'
        }
        response = method(route, headers=headers,timeout=accepted_time,json=body)
    except Exception as e:
        print(e)
        return False
    # get current time
    stop_time = time.time()
    # calculate time taken
    time_taken = stop_time - start_time
    if time_taken < accepted_time:
        return True
    else:
        return False

class TestPerformance(unittest.TestCase):
    """
    Tests the API.py file
    """

    def test_get_server_files(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_files"

        # assert time is less than accepted time
        assert test_route(route,method=requests.get) is True

    def test_start_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/start_job"
        assert test_route(route) is True
    def test_stop_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/stop_job"
        assert test_route(route) is True
    def test_kill_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/kill_job"
        assert test_route(route) is True
    def test_enable_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/enable_job"
        assert test_route(route) is True
    def test_modify_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/modify_job"
        assert test_route(route) is True
    def test_log_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/log"
        assert test_route(route) is True
    def test_get_nexum_file(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/nexum"
        assert test_route(route,method=requests.get) is True
    def test_get_nexum_service_url(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/nexumservice"
        assert test_route(route,method=requests.get) is True
    def test_get_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_job"
        assert test_route(route,method=requests.get) is True
    def test_force_checkin_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/force_checkin"
        assert test_route(route) is True
    def test_restore_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/restore"
        assert test_route(route) is True
    def test_get_status(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_status"
        assert test_route(route,method=requests.get) is True
    def test_get_job_status(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_job_status"
        assert test_route(route,method=requests.get) is True
    def test_force_update(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/force_update"
        assert test_route(route) is True
    def test_get_version(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_version"
        assert test_route(route,method=requests.get) is True
    def test_get_jobs(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_jobs"
        assert test_route(route,method=requests.get) is True
    def test_beat(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/beat"
        assert test_route(route) is True
    def test_get_urls_from_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/urls"
        assert test_route(route,method=requests.get) is True
    def test_make_smb(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/make_smb"
        assert test_route(route) is True
    def test_get_smb(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_smb"
        assert test_route(route,method=requests.get) is True
    def test_delete_smb(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/delete_smb"
        assert test_route(route,method=requests.delete) is True
    def test_verify(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/verify"
        assert test_route(route,method=requests.get) is True
    def test_check_installer(self):
        """
        Tests the testing function
        """
        route = f"{TENANT_SERVER}/check-installer"
        assert test_route(route,method=requests.get) is True
    def test_uninstall(self):
        """
        Tests the testing function
        """
        route = f"{TENANT_SERVER}/uninstall"
        assert test_route(route,method=requests.get) is True
if __name__ == '__main__':
    unittest.main()
