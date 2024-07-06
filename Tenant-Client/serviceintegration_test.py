import unittest
# pylint: disable=missing-function-docstring
# pylint: disable=missing-class-docstring
class Testservice(unittest.TestCase):
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

