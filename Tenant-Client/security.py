"""
# Program: Tenant-Client
# File: security.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the Security class. This class is used 
# to manage security, encrypt and decrypt strings, and manage the client secret

# Class Types: 
#               1. API - Connector

"""
# pylint: disable= import-error, unused-argument, global-statement
import base64
import hashlib
import time
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
from logger import Logger
from MySqlite import MySqlite
CLIENT_SECRET = "ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD" # secret for the client to communicate with A2
ENCODING = "utf-8"
# pylint: disable= bare-except

class Security():
    """
    Class to manage security
    Type: Security
    Relationship: NONE
    """
    @staticmethod
    def set_client_secret(secret):
        """
        Set the client secret
        """
        global CLIENT_SECRET
        CLIENT_SECRET = secret
    @staticmethod
    def get_client_secret():
        """
        Get the client secret
        """
        return CLIENT_SECRET
    @staticmethod
    def load_client_secret():
        """
        Load the client secret from the db

        """
        global CLIENT_SECRET
        temp = MySqlite.read_setting("client_secret")
        CLIENT_SECRET = temp


    @staticmethod
    def split_string(string):
        """
        split a string in half with right side majority
        """
        length = len(string)
        half_length = length // 2
        right_side = str(string)[half_length:]
        left_side = str(string)[:half_length]
        return left_side, right_side
    #sha256 a string
    @staticmethod
    def sha256_string(string):
        """
        SHA256 a string
        """
        # Convert the string to bytes
        string_bytes = str(string).encode(ENCODING)

            # Compute the SHA-256 hash
        sha256_hash = hashlib.sha256(string_bytes).hexdigest()

        return sha256_hash

    # encrypt a string using AES
    @staticmethod
    def encrypt_string(password, string):
        """
        Encrypt a string with AES-256 bit encryption
        """
        # Pad the password to be 16 bytes long
        password_hashed = str(password).ljust(16).encode(ENCODING)

        # Create a new AES cipher with the password as the key
        cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
        encryptor = cipher.encryptor()

        # Pad the string to be a multiple of 16 bytes long
        string = string.ljust((len(string) // 16 + 1) * 16).encode(ENCODING)

        # Encrypt the string using AES
        encrypted_string = encryptor.update(string) + encryptor.finalize()

        # Encode the encrypted string in base64
        encoded_string = base64.b64encode(encrypted_string)

        return encoded_string.decode(ENCODING)

    # decrypt a string using AES
    @staticmethod
    def decrypt_string(password, string):
        """
        Decrypt a string with AES-256 bit decryption
        """
        l = Logger()
        # Pad the password to be 16 bytes long
        password_hashed = str(password).ljust(16).encode(ENCODING)

        # Create a new AES cipher with the password as the key
        cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
        decryptor = cipher.decryptor()

        # Decode the string from base64
        decoded_string = base64.b64decode(string)

        # Decrypt the string using AES
        decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()
        try:
            return decrypted_string.decode(ENCODING)
        except UnicodeDecodeError:
            l.log("ERROR", "decrypt_string", "Decryption failed", "1004", time.asctime())
            return "Decryption failed"
        except:
            l.log("ERROR", "decrypt_string", "General Error decrypting string",
            "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            return "Decryption failed"
