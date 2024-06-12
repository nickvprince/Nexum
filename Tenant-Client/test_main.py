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
from job import Job
import jobsettings
from conf import Configuration
import conf
from MySqlite import MySqlite
from security import Security
from helperfunctions import get_client_info
from helperfunctions import *
from integrationtests import TestAPINewFile
from jobsettingtests import JobSettingsTests
from apitests import TestAPI
from configurationtests import ConfigurationTests
from securitytests import SecurityTests
from jobtests import JobTests
from api import API
# pylint: disable=missing-function-docstring
# pylint: disable=missing-class-docstring

class TestMain(unittest.TestCase):

    def test_create_db_file(self):
        dir = "logs/"
        path = "test.db"
        MySqlite.create_db_file(dir, path)
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




if __name__ == '__main__':
    InitSql()
    unittest.main()

