import unittest
from security import Security

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
        result = Security.encrypt_string("WelcoeHome", "Password")
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