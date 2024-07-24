import unittest
import os
import sys
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import conf
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