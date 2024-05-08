"""
# Program: Tenant-Client
# File: test_main.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the tests for the main.py file


"""
# pylint: disable= import-error, unused-argument
import time
import unittest
import os
import main
import job
import conf
import jobsettings
from sql import InitSql, create_db_file
from security import Security
from helperfunctions import get_client_info
from helperfunctions import *
from api import API
# pylint: disable=missing-function-docstring
# pylint: disable=missing-class-docstring


class TestAPI(unittest.TestCase):


    def test_get_tenant_portal_url(self):
        expected_url = "https://nexum.com/tenant_portal"
        self.assertEqual(API.get_tenant_portal_url(), expected_url)

    def test_get_status(self):
        expected_status = "running"
        self.assertEqual(API.get_status(), expected_status)

    def test_get_percent(self):
        expected_percent = 70
        self.assertEqual(API.get_percent(), expected_percent)

    def test_get_version(self):
        expected_version = "1.2.7"
        self.assertEqual(API.get_version(), expected_version)

    def test_get_job(self):
        expected_job = "backup"
        self.assertEqual(API.get_job(), expected_job)

    def test_get_client_id(self):
        expected_client_id = 1
        self.assertEqual(API.get_client_id(), expected_client_id)

    def test_get_tenant_id(self):
        expected_tenant_id = 1
        self.assertEqual(API.get_tenant_id(), expected_tenant_id)

    def test_get_download_key(self):
        expected_download_key = "1234"
        self.assertEqual(API.get_download_key(), expected_download_key)
    def test_send_success_install(self):
        expected_status = True
        self.assertEqual(API.send_success_install(0,0,0), expected_status)

# test run job test init sql, test Logger, test Icon Manager


class SecurityTests(unittest.TestCase):

    def test_split_string(self):
        """
        Test the split_string function from the Security class 
        to ensure an even split with a right side majority
        @test: "test" -> ("te", "st") -- Test even number of characters
        @test: "testing" -> ("tes", "ting") -- test odd number of 
        characters with right side majority
        @test: "hello world" -> ("hello", " world") -- test odd number of characters with space
        """
        test_cases = [
            ("test", ("te", "st")),
            ("testing", ("tes", "ting")),
            ("hello world", ("hello", " world")),
        ]

        for i, (input_str, expected_output) in enumerate(test_cases):
            with self.subTest(test=i):
                result = Security.split_string(input_str)
                self.assertEqual(result, expected_output)

    def test_sha(self):
        """
        Test the sha256_hash function from the Security class
        to ensure the correct hash is generated
        @test: "test" -> "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"
        @test: "testing" -> "cf80cd8aed482d5d1527d7dc72fceff84e6326592848447d2dc0b0e87dfc9a90
        """
        test_cases = [
            ("test", "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"),
            ("testing", "cf80cd8aed482d5d1527d7dc72fceff84e6326592848447d2dc0b0e87dfc9a90"),
        ]
        for i, (input_str, expected_output) in enumerate(test_cases):
            with self.subTest(test=i):
                result = Security.sha256_string(input_str)
                self.assertEqual(result, expected_output)

    def test_encrypt(self):
        """
        Test the encrypt function from the Security class
        to ensure the correct encrypted string is generated
        @test: "WelcomeHome","Password" -> "b'gAAAAABgF9Xf2PvH3lK6X6wBQ7Xm0g7nL2Qf8xw5Lz5Zp4rY5eXc
        """
        result = Security.encrypt_string("WelcomeHome", "Password")
        self.assertEqual(result,"v2lpkJw3rLrXCDmeci/ZxQ==")
    def test_encrypt_different_password(self):
        """
        Test the encrypt function from the Security class
        to ensure the correct encrypted string is generated
        @test: "WelcomeHome","Passwrd" !="b'gAAAAABgF9Xf2PvH3lK6X6wBQ7Xm0g7nL2Qf8xw5Lz5Zp4rY5eXc
        """
        result = Security.encrypt_string("WelcomeHome", "Pasword")
        self.assertNotEqual(result,"v2lpkJw3rLrXCDmeci/ZxQ==")
    def test_decrypt_wrong_password(self):
        """
        Test the encrypt function from the Security class
        to ensure the correct encrypted string is generated
        @test: "WelcomeHome","Passwrd" !="b'gAAAAABgF9Xf2PvH3lK6X6wBQ7Xm0g7nL2Qf8xw5Lz5Zp4rY5eXc
        """
        result = Security.decrypt_string("WelcomHome", "v2lpkJw3rLrXCDmeci/ZxQ==")
        self.assertNotEqual(result,"Password")
    def test_decrypt_right_password(self):
        """
        Test the encrypt function from the Security class
        to ensure the correct encrypted string is generated
        @test: "WelcomeHome","Passwrd" !="b'gAAAAABgF9Xf2PvH3lK6X6wBQ7Xm0g7nL2Qf8xw5Lz5Zp4rY5eXc
        """
        print(Security.encrypt_string("WelcomeHome", "Password"))
        print(Security.decrypt_string("WelcomeHome", "v2lpkJw3rLrXCDmeci/ZxQ=="))
class TestMain(unittest.TestCase):

    def test_create_db_file(self):
        dir = "logs/"
        path = "test.db"
        create_db_file(dir, path)
        self.assertTrue(os.path.exists(dir + path))
        # Delete the file
        os.remove(dir + path)
    def test_get_client_info(self):
        # Set up the expected values
        expected_client_id = -1
        expected_tenant_id = -1
        expected_tenant_portal_url = "https://nexum.com/tenant_portal"

        # Call the function to get the client info
        get_client_info()

        # Check if the global variables are assigned correctly
        self.assertEqual(CLIENT_ID, expected_client_id)
        self.assertEqual(TENANT_ID, expected_tenant_id)
        self.assertEqual(TENANT_PORTAL_URL, expected_tenant_portal_url)
        time.sleep(0.2)


    def test_logs(self):
        current_user = os.getlogin()
        file_path = f"C:\\Users\\{current_user}\\Downloads\\nexumlog.csv"
        main.logs()
        self.assertTrue(os.path.exists(file_path))


class JobTests(unittest.TestCase):
    def test_get_id(self):
        j= job.Job()
        j.set_id(1)
        self.assertEqual(j.get_id(), 1)

    def test_get_title(self):
        j = job.Job()
        j.set_title("Backup")
        self.assertEqual(j.get_title(), "Backup")

    def test_get_created(self):
        j = job.Job()
        j.set_created("2021-09-15")
        self.assertEqual(j.get_created(), "2021-09-15")

    def test_get_config(self):
        j = job.Job()
        config = conf.Configuration(1, "Backup", "2021-09-15")
        j.set_config(config)
        self.assertEqual(j.get_config(), config)

    def test_get_settings(self):
        j = job.Job()
        settings = jobsettings.JobSettings()
        j.set_settings(settings)
        self.assertEqual(j.get_settings(), settings)

    def test_set_id(self):
        j = job.Job()
        j.set_id(1)
        self.assertEqual(j.get_id(), 1)

    def test_set_title(self):
        j = job.Job()
        j.set_title("Backup")
        self.assertEqual(j.get_title(), "Backup")

    def test_set_created(self):
        j = job.Job()
        j.set_created("2021-09-15")
        self.assertEqual(j.get_created(), "2021-09-15")

    def test_set_config(self):
        j = job.Job()
        config = conf.Configuration(1, "Backup", "2021-09-15")
        j.set_config(config)
        self.assertEqual(j.get_config(), config)

    def test_set_settings(self):
        j = job.Job()
        settings = jobsettings.JobSettings()
        j.set_settings(settings)
        self.assertEqual(j.get_settings(), settings)

class ConfigurationTests(unittest.TestCase):

    def test_get_tenant_secret(self):
        c = conf.Configuration(1, "Backup", "123 test street")
        self.assertEqual(c.get_tenant_secret(), "Backup")

    def test_get_id(self):
        c = conf.Configuration(1, "Backup", "123 test street")
        self.assertEqual(c.get_id(), 1)

    def test_get_address(self):
        c = conf.Configuration(1, "Backup", "123 test street")
        self.assertEqual(c.get_address(), "123 test street")

    def test_set_tenant_secret(self):
        c = conf.Configuration(1, "Backup", "123 test street")
        c.set_tenant_secret("Restore")
        self.assertEqual(c.get_tenant_secret(), "Restore")

    def test_set_id(self):
        c = conf.Configuration(1, "Backup", "123 test street")
        c.set_id(2)
        self.assertEqual(c.get_id(), 2)

    def test_set_address(self):
        c = conf.Configuration(1, "Backup", "123 test street")
        c.set_address("456 test street")
        self.assertEqual(c.get_address(), "456 test street")


class JobSettings (unittest.TestCase):
    def test_get_id(self):
        js = jobsettings.JobSettings()
        js.set_id(1)
        self.assertEqual(js.get_id(), 1)

    def test_get_schedule(self):
        js = jobsettings.JobSettings()
        js.set_schedule("1110010")
        self.assertEqual(js.get_schedule(), "1110010")

    def test_get_start_time(self):
        js = jobsettings.JobSettings()
        js.set_start_time("12:00")
        self.assertEqual(js.get_start_time(), "12:00")

    def test_get_stop_time(self):
        js = jobsettings.JobSettings()
        js.set_stop_time("13:00")
        self.assertEqual(js.get_stop_time(), "13:00")

    def test_get_retry_count(self):
        js = jobsettings.JobSettings()
        js.set_retry_count(3)
        self.assertEqual(js.get_retry_count(), 3)

    def test_get_sampling(self):
        js = jobsettings.JobSettings()
        js.set_sampling(5)
        self.assertEqual(js.get_sampling(), 5)

    def test_get_retention(self):
        js = jobsettings.JobSettings()
        js.set_retention(7)
        self.assertEqual(js.get_retention(), 7)

    def test_get_last_job(self):
        js = jobsettings.JobSettings()
        js.set_last_job("2021-09-15")
        self.assertEqual(js.get_last_job(), "2021-09-15")

    def test_get_notify_email(self):
        js = jobsettings.JobSettings()
        js.set_notify_email("test@gmail.com")
        self.assertEqual(js.get_notify_email(), "test@gmail.com")

    def test_get_heartbeat_interval(self):
        js = jobsettings.JobSettings()
        js.set_heartbeat_interval(60)
        self.assertEqual(js.get_heartbeat_interval(), 60)

    def test_set_id(self):
        js = jobsettings.JobSettings()
        js.set_id(1)
        self.assertEqual(js.get_id(), 1)

    def test_set_schedule(self):
        js = jobsettings.JobSettings()
        js.set_schedule("1110010")
        self.assertEqual(js.get_schedule(), "1110010")

    def test_set_start_time(self):
        js = jobsettings.JobSettings()
        js.set_start_time("12:00")
        self.assertEqual(js.get_start_time(), "12:00")

    def test_set_stop_time(self):
        js = jobsettings.JobSettings()
        js.set_stop_time("13:00")
        self.assertEqual(js.get_stop_time(), "13:00")

    def test_set_retry_count(self):
        js = jobsettings.JobSettings()
        js.set_retry_count(3)
        self.assertEqual(js.get_retry_count(), 3)

    def test_set_sampling(self):
        js = jobsettings.JobSettings()
        js.set_sampling(5)
        self.assertEqual(js.get_sampling(), 5)

    def test_set_retention(self):
        js = jobsettings.JobSettings()
        js.set_retention(7)
        self.assertEqual(js.get_retention(), 7)

    def test_set_last_job(self):
        js = jobsettings.JobSettings()
        js.set_last_job("2021-09-15")
        self.assertEqual(js.get_last_job(), "2021-09-15")

    def test_set_notify_email(self):
        js = jobsettings.JobSettings()
        js.set_notify_email("test@gmail.com")
        self.assertEqual(js.get_notify_email(), "test@gmail.com")

    def test_set_heartbeat_interval(self):
        js = jobsettings.JobSettings()
        js.set_heartbeat_interval(60)
        self.assertEqual(js.get_heartbeat_interval(), 60)








if __name__ == '__main__':

   unittest.main()
