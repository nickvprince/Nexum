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
import threading
APIKEY = "testing"
THREADS = 50
MSP_ACCEPTED_TIME = 20

TENANT_PORT = 5002
TENANT_SERVER = f"http://localhost:{TENANT_PORT}"
@staticmethod 
def load_route(route,method=requests.post)->bool:
    # Make list of threads
    threads = []
    # Start the threads
    from multiprocessing.pool import ThreadPool
    pool = ThreadPool(processes=THREADS)
    async_results = pool.apply_async(test_route, (route,method))
    results = async_results.get()
    if results is False:
        return False
    else:
        return True
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
        _ = method(route, headers=headers,timeout=accepted_time,json=body)
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

class TestLoad(unittest.TestCase):
    """
    Tests the API.py file
    """

    def test_get_server_files(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_files"
        assert load_route(route,method=requests.get) is True

    def test_start_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/start_job"
        assert load_route(route) is True
    def test_stop_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/stop_job"
        assert load_route(route) is True
    def test_kill_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/kill_job"
        assert load_route(route) is True
    def test_enable_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/enable_job"
        assert load_route(route) is True
    def test_modify_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/modify_job"
        assert load_route(route) is True
    def test_log_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/log"
        assert load_route(route) is True
    def test_get_nexum_file(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/nexum"
        assert load_route(route,method=requests.get) is True
    def test_get_nexum_service_url(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/nexumservice"
        assert load_route(route,method=requests.get) is True
    def test_get_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_job"
        assert load_route(route,method=requests.get) is True
    def test_force_checkin_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/force_checkin"
        assert load_route(route) is True
    def test_restore_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/restore"
        assert load_route(route) is True
    def test_get_status(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_status"
        assert load_route(route,method=requests.get) is True
    def test_get_job_status(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_job_status"
        assert load_route(route,method=requests.get) is True
    def test_force_update(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/force_update"
        assert load_route(route) is True
    def test_get_version(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_version"
        assert load_route(route,method=requests.get) is True
    def test_get_jobs(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_jobs"
        assert load_route(route,method=requests.get) is True
    def test_beat(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/beat"
        assert load_route(route) is True
    def test_get_urls_from_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/urls"
        assert load_route(route,method=requests.get) is True
    def test_make_smb(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/make_smb"
        assert load_route(route) is True
    def test_get_smb(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_smb"
        assert load_route(route,method=requests.get) is True
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
        assert load_route(route,method=requests.get) is True
    def test_check_installer(self):
        """
        Tests the testing function
        """
        route = f"{TENANT_SERVER}/check-installer"
        assert load_route(route,method=requests.get) is True
    def test_uninstall(self):
        """
        Tests the testing function
        """
        route = f"{TENANT_SERVER}/uninstall"
        assert load_route(route,method=requests.get) is True
if __name__ == '__main__':
    unittest.main()
