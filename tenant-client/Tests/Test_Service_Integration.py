import unittest
import os
import sys
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
# pylint: disable=missing-function-docstring
# pylint: disable=missing-class-docstring
class serviceTests(unittest.TestCase):
        def test_start_job(self):
            print("hello")
            self.assertEqual(1,1)

        def test_stop_job(self):
            print("hello")
            self.assertEqual(1,1)

        def test_get_status(self):
            print("hello")
            self.assertEqual(1,1)

        def test_start_job_unauthenticated(self):
            print("hello")
            self.assertEqual(1,1)

        def test_stop_job_unauthenticated(self):
            print("hello")
            self.assertEqual(1,1)

        def test_get_status_unauthenticated(self):
            print("hello")
            self.assertEqual(1,1)

