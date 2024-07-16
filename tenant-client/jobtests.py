
import unittest
from job import Job
import conf
import jobsettings

class JobTests(unittest.TestCase):
    def test_get_id(self):
        j= Job()
        j.set_id(1)
        self.assertEqual(j.get_id(), 1)

    def test_get_title(self):
        j = Job()
        j.set_title("Backup")
        self.assertEqual(j.get_title(), "Backup")

    def test_get_created(self):
        j = Job()
        j.set_created("2021-09-15")
        self.assertEqual(j.get_created(), "2021-09-15")

    def test_get_config(self):
        j = Job()
        config = conf.Configuration(1, "Backup", "2021-09-15")
        j.set_config(config)
        self.assertEqual(j.get_config(), config)

    def test_get_settings(self):
        j = Job()
        settings = jobsettings.JobSettings()
        j.set_settings(settings)
        self.assertEqual(j.get_settings(), settings)

    def test_set_id(self):
        j = Job()
        j.set_id(1)
        self.assertEqual(j.get_id(), 1)

    def test_set_title(self):
        j = Job()
        j.set_title("Backup")
        self.assertEqual(j.get_title(), "Backup")

    def test_set_created(self):
        j = Job()
        j.set_created("2021-09-15")
        self.assertEqual(j.get_created(), "2021-09-15")

    def test_set_config(self):
        j = Job()
        config = conf.Configuration(1, "Backup", "2021-09-15")
        j.set_config(config)
        self.assertEqual(j.get_config(), config)

    def test_set_settings(self):
        j = Job()
        settings = jobsettings.JobSettings()
        j.set_settings(settings)
        self.assertEqual(j.get_settings(), settings)
