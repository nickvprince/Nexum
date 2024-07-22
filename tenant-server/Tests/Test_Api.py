"""
Tests the API.py file
"""

import unittest
import sys
import os
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from api import API

class TestApi(unittest.TestCase):
    """
    Tests the API.py file
    """

    def test_get_update_available(self):
        """
        Tests the testing function
        """
        API.get_update_available()

if __name__ == '__main__':
    unittest.main()
