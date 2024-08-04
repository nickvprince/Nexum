import unittest
import os
import sys
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from api import API
from job import Job
import jobsettings
from conf import Configuration
from MySqlite import MySqlite
# pylint: disable=missing-function-docstring
# pylint: disable=missing-class-docstring
class TestAPI(unittest.TestCase):


    def test_get_tenant_portal_url(self):
        MySqlite.write_setting("TENANT_PORTAL_URL", "https://nexum.com/tenant_portal")
        expected_url = "https://nexum.com/tenant_portal"
        self.assertEqual(API.get_tenant_portal_url(), expected_url)

    def test_get_status(self):
        test_cases = [
            'running',
            'stopped',
            'error',
            'Idle',
        ]

        test_number:int = 0
        for i in enumerate(test_cases):
            MySqlite.write_setting("Status", test_cases[test_number])
            with self.subTest(test=i):
                result = API.get_status()
                self.assertEqual(result, test_cases[test_number], f"Test case {i} failed {result} != {test_cases[test_number]}")
                test_number +=1

    def test_get_percent(self):
        expected_percent = "0%"
        self.assertEqual(API.get_percent(), expected_percent)

    def test_get_version(self):
        MySqlite.write_setting("version", "1")
        expected_version = "1"
        self.assertEqual(API.get_version(), expected_version)

    def test_get_job_nojob(self):
        jb:Job = Job()
        jb.set_id(1)
        js:jobsettings.JobSettings = jobsettings.JobSettings()
        js.set_id(1)
        js.set_schedule("1110010")
        js.set_start_time("12:00")
        js.set_stop_time("13:00")
        js.set_retry_count(3)
        js.set_sampling(5)
        js.set_retention(7)
        js.set_last_job("2021-09-15")
        js.set_notify_email("test@test.com")
        js.set_heartbeat_interval(5)

        jc:Configuration = Configuration(1, MySqlite.read_setting("TENANT_SECRET"), "2024-06-12")
        jb.set_title("backup")
        jb.set_config(jc)
        jb.set_settings(js)
        jb.save()
        jb.delete()

        self.assertEqual(API.get_job().title, None)

    def test_get_job(self):
        jb:Job = Job()
        jb.set_id(1)
        js:jobsettings.JobSettings = jobsettings.JobSettings()
        js.set_id(1)
        js.set_schedule("1110010")
        js.set_start_time("12:00")
        js.set_stop_time("13:00")
        js.set_retry_count(3)
        js.set_sampling(5)
        js.set_retention(7)
        js.set_last_job("2021-09-15")
        js.set_notify_email("test@test.com")
        js.set_heartbeat_interval(5)

        jc:Configuration = Configuration(1, MySqlite.read_setting("TENANT_SECRET"), "2024-06-12")
        jb.set_title("backup")
        jb.set_config(jc)
        jb.set_settings(js)
        jb.save()

        self.assertEqual(API.get_job().title, jb.title)
    

    def test_get_client_id(self):
        expected_client_id = int(MySqlite.read_setting("CLIENT_ID"))
        self.assertEqual(API.get_client_id(), expected_client_id)

    def test_get_tenant_id(self):
        expected_tenant_id = int(MySqlite.read_setting("TENANT_ID"))
        self.assertEqual(API.get_tenant_id(), expected_tenant_id)

    def test_get_download_key(self):
        expected_download_key = "1234"
        self.assertEqual(API.get_download_key(), expected_download_key)

    def test_send_success_install(self):
        expected_status = True
        self.assertEqual(API.send_success_install(MySqlite.read_setting("CLIENT_ID"),0,0), expected_status)

if __name__ == '__main__':
    unittest.main()