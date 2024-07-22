"""
# Program: Tenant-Client
# File: MySqlite.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the InitSql class. This class is used
# to initialize the SQL files and tables 
# at the beginning of the program and ensure they exist

# Class Types: 
#               1. InitSql - File IO

"""
#pylint: disable= bare-except

import os
import sqlite3
import subprocess
import base64
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import requests

ENCODING = 'utf-8'
# pah directories
current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
settingsDirectory = os.path.join(current_dir, '..\\settings') # directory for settings
logdirectory = os.path.join(current_dir,'../logs') # directory for logs

# path files
SETTINGS_PATH = os.path.join(current_dir,
                            settingsDirectory+'\\settings.db') # path to the settings database
jobFile=os.path.join('/settings.db')
configFile=os.path.join('/settings.db')
job_settingsFile=os.path.join('/settings.db')
logpath = os.path.join('/log.db') # path to the log database

@staticmethod
def convert_device_status():
    """
    Converts the status to a enum
    """
    status = MySqlite.read_setting("Status")
    if status == "Online":
        return 1
    elif status == "Offline":
        return 0
    elif status == "ServiceOffline":
        return 2
    else:
        return -1

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
    try:
        string = str(string).ljust((len(string) // 16 + 1) * 16).encode(ENCODING)


    # Encrypt the string using AES
        encrypted_string = encryptor.update(string) + encryptor.finalize()

    # Encode the encrypted string in base64
        encoded_string = base64.b64encode(encrypted_string)
        return encoded_string.decode(ENCODING)
    except UnicodeDecodeError:
        MySqlite.write_log("ERROR", "SQL", "Failed to encrypt string - Unicode decode error",
                            "1007", "Failed to encrypt string")
        return "Encryption Failed"
    except:
        MySqlite.write_log("ERROR", "SQL", "Failed to encrypt string",
                            "1007", "Failed to encrypt string")
        return "Encryption Failed"

# decrypt a string using AES
@staticmethod
def decrypt_string(password, string):
    """
    Decrypt a string with AES-256 bit decryption
    """
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
        MySqlite.write_log("ERROR", "SQL", "Failed to decrypt string - Unicode decode error",
                            "1004", "Failed to decrypt string")
        return "Decryption failed"
    except:
        MySqlite.write_log("ERROR", "SQL", "Failed to decrypt string",
                           "1004", "Failed to decrypt string")
        return "Decryption failed"

class MySqlite():
    """
    Class to interact with the sqlite database
    Type: File IO
    """

    @staticmethod
    def write_log(severity, subject, message, code, date):
        """ 
        Write a log to the database
        """
        conn = sqlite3.connect(logdirectory+logpath)
        identification= 0
        cursor = conn.cursor()


        cursor.execute('''SELECT MAX(id) FROM logs''')
        result = cursor.fetchone()[0]
        if result is not None:
            identification = int(result) + 1
        else:
            identification = 1


        cursor.execute('''INSERT INTO logs (id,severity, subject, message, code, date)
                    VALUES (?,?, ?, ?, ?, ?)''',
                    (identification, severity, subject, message, code, date))
        conn.commit()
        conn.close()

    @staticmethod
    def write_setting(setting, value):
        """
        Write a setting to the database
        """
        result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
                        capture_output=True, text=True,check=True,shell=True) # Use UUID to encrypt the data
        output = result.stdout.strip()
        output = output.split('\n\n', 1)[-1]
        output = output[:32]

        value = encrypt_string(output,value)

        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
        existing_value = cursor.fetchone()
        if existing_value: # if setting exists overwrite it
            cursor.execute('''UPDATE settings SET value = ? WHERE setting = ?''', (value, setting))
        else: # Write a setting
            cursor.execute('''INSERT INTO settings (setting, value) VALUES (?, ?)''',
                            (setting, value))
        conn.commit()
        conn.close()

        if setting == "Status":
            headers = {
                "apikey": MySqlite.read_setting("apikey"),
                "Content-Type": "application/json"
            }
            content = {
                "severity": "INFO",
                "function":"status",
                "code":convert_device_status(),
                "uuid": MySqlite.read_setting("uuid"),
                "client_id": MySqlite.read_setting("CLIENT_ID"),
            }
            try:
                response = requests.post(f"http://{MySqlite.read_setting('server_address')}:{MySqlite.read_setting('server_port')}/log", headers=headers, json=content,timeout=5)
            except Exception as e:
                MySqlite.write_log("ERROR", "API", "Error sending status", "0", "9/7/2024")

    @staticmethod
    def read_setting(setting):
        """
        Read a setting from the database
        """

        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
        try:
            value = cursor.fetchone()[0]
            conn.close()
            result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
                                    capture_output=True, text=True,check=True,shell=True) # enc with uuid
            output = result.stdout.strip()
            output = output.split('\n\n', 1)[-1]
            output = output[:32]
            value = decrypt_string(output,value)
            return value.rstrip()
        except:
            return None
    @staticmethod

    def create_db_file(directory,path):
        """
        create the database file if it does not exist and the folder for it
        """
        # ensure ../ logs directory exists
        if not os.path.exists(os.path.join(directory)):
            os.makedirs(os.path.join(directory))
        # if file does not exist create it
        if not os.path.exists(directory+path):
            if path=="":
                conn = sqlite3.connect(directory)
            elif directory=="" :
                conn = sqlite3.connect(path)
            else:
                conn = sqlite3.connect(directory+path)
            conn.close()
