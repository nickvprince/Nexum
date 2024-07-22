"""
Tests the helperfunctions.py file
"""

import unittest
import sys
import os
sys.path.insert(0,os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import helperfunctions

class Testclient(unittest.TestCase):
    """
    Tests the helperfunctions.py file
    """

    def test_testing(self):
        """
        Tests the testing function
        """
        # assert 1=1
        assert 1==1
        

if __name__ == '__main__':
    unittest.main()
