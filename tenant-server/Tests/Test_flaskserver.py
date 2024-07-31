"""
Tests the flaskserver.py file

beat/enable_job/force_update/get_files,urls,killjob

nexum,nexumservice,uninstall,verify
"""

import unittest
import sys
import os
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from flaskserver import FlaskServer
import unittest
import sys
import os
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from api import API
from sql import MySqlite
import time
import requests

APIKEY = "testing"

ACCEPTED_TIME = 10

TENANT_PORT = 5002
TENANT_SERVER = f"http://localhost:{TENANT_PORT}"

@staticmethod
def test_route(route,method=requests.post,body = {'client_id':'0'})->requests.Response:
    """
    Tests a route, measures the time and returns if it passed or not
    """
    # make post request to MSP to get the URL's
    try:
        headers = {
            'apikey': APIKEY,
            'content-type': 'application/json'
        }
        response = method(route, headers=headers,timeout=ACCEPTED_TIME,json=body)
        return response
    except Exception as e:
        print(e)
        return response

class Testclient(unittest.TestCase):
    """
    Tests the Heartbeat.py file
    """
    def test_get_server_files(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_files"
        # assert time is less than accepted time
        body = {
            "client_id": "0",
            "path":"c:\\users\\"
        }
        assert test_route(route,method=requests.get,body=body).status_code == 200

    def test_start_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/start_job"
        assert test_route(route).status_code == 200
    def test_stop_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/stop_job"
        assert test_route(route).status_code == 200
    def test_kill_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/kill_job"
        assert test_route(route).status_code == 200
    def test_enable_server_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/enable_job"
        assert test_route(route).status_code == 200
    def test_modify_server_job_smb_exists(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/make_smb"
        body = {
            "name":"backupserver1",
            "path":"\\\\192.168.50.12\\Data",
            "password":"VoVnR9CjAVP4d5K9uLMcZg==",
            "username":"Username"
        }
        response = test_route(route,body=body)
        identification = response.json()['id']
        body = {
            "client_id": "1",
            "title": "test job",
            "settings": {
            "backupServerId": identification,
            "schedule": "1100101",
            "startTime": "15:00",
            "stopTime": "18:25",
            "retryCount": 3,
            "sampling": 0,
            "id":"0",
            "heartbeat_interval": 30,
            "path": "c:\\users\\teche\\documents\\nexumTests\\",
            "user": "NasBackupper",
            "password": "B@ck1tUp$",
            "retention":14
            }
        }
        route=f"{TENANT_SERVER}/modify_job"
        assert test_route(route).status_code == 200

    def test_modify_server_job_smb_not_exists(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/delete_smb/1"
        body = {

        }
        test_route(route,method=requests.delete,body=body)
        body = {
            "client_id": "1",
            "title": "test job",
            "settings": {
            "backupServerId": 1,
            "schedule": "1100101",
            "startTime": "15:00",
            "stopTime": "18:25",
            "retryCount": 3,
            "sampling": 0,
            "id":"0",
            "heartbeat_interval": 30,
            "path": "c:\\users\\teche\\documents\\nexumTests\\",
            "user": "NasBackupper",
            "password": "B@ck1tUp$",
            "retention":14
            }
        }
        route=f"{TENANT_SERVER}/modify_job"
        assert test_route(route).status_code == 403

    def test_log_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/log"
        assert test_route(route).status_code == 200
    def test_get_nexum_file(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/nexum"
        assert test_route(route,method=requests.get).status_code == 500
    def test_get_nexum_service_url(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/nexumservice"
        assert test_route(route,method=requests.get).status_code == 500
    def test_get_job(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_job/0"
        assert test_route(route,method=requests.get).status_code == 200
    def test_force_checkin_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/force_checkin"
        assert test_route(route).status_code == 200
    def test_restore_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/restore"
        assert test_route(route).status_code == 200
    def test_get_status(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_status/0"
        assert test_route(route,method=requests.get).status_code == 200
    def test_get_job_status(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_job_status/0"
        assert test_route(route,method=requests.get).status_code == 200
    def test_force_update(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/force_update"
        assert test_route(route).status_code == 403
    def test_get_version(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_version/0"
        assert test_route(route,method=requests.get).status_code == 200
    def test_get_jobs(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_jobs"
        assert test_route(route,method=requests.get).status_code == 200
    def test_beat(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/beat"
        assert test_route(route).status_code == 403
    def test_get_urls_from_server(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/urls"
        assert test_route(route,method=requests.get).status_code == 403
    def test_make_smb(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/make_smb"
        assert test_route(route).status_code == 200
    def test_get_smb(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/get_smb/1"
        assert test_route(route,method=requests.get).status_code == 200
    def test_delete_smb(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/delete_smb/1"
        assert test_route(route,method=requests.delete).status_code == 200
    def test_verify(self):
        """
        Tests the testing function
        """
        route=f"{TENANT_SERVER}/verify"
        assert test_route(route,method=requests.get).status_code == 500
    def test_check_installer(self):
        """
        Tests the testing function
        """
        route = f"{TENANT_SERVER}/check-installer"
        body = {
            "client_id": "0",
            "installationKey": "abcd"
        }
        assert test_route(route,method=requests.get,body=body).status_code == 500
    def test_uninstall(self):
        """
        Tests the testing function
        """
        route = f"{TENANT_SERVER}/uninstall"
        body = {
            "client_id": "0",
            "uuid:": "abcd"
        }
        assert test_route(route,method=requests.get,body=body).status_code == 500
if __name__ == '__main__':
    unittest.main()
