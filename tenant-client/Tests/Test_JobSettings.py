import os
import sys
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import jobsettings
import unittest
# pylint: disable=missing-function-docstring
# pylint: disable=missing-class-docstring
class JobSettingsTests (unittest.TestCase):
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