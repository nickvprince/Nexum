"""
# Program: Tenant-Client
# File: InitSqlite.py
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

import sqlite3
import base64
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
from MySqlite import *



# encrypt a string using AES
@staticmethod
def encrypt_string(password, string):
    """
    Encrypt a string with AES-256 bit encryption
    """
    # Pad the password to be 16 bytes long
    password_hashed = str(password).ljust(16).encode('utf-8')

    # Create a new AES cipher with the password as the key
    cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
    encryptor = cipher.encryptor()

    # Pad the string to be a multiple of 16 bytes long
    try:
        string = str(string).ljust((len(string) // 16 + 1) * 16).encode('utf-8')


    # Encrypt the string using AES
        encrypted_string = encryptor.update(string) + encryptor.finalize()

    # Encode the encrypted string in base64
        encoded_string = base64.b64encode(encrypted_string)
        return encoded_string.decode('utf-8')
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
    password_hashed = str(password).ljust(16).encode('utf-8')

    # Create a new AES cipher with the password as the key
    cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
    decryptor = cipher.decryptor()

    # Decode the string from base64
    decoded_string = base64.b64decode(string)
    try:
    # Decrypt the string using AES
        decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()

        return decrypted_string.decode('utf-8')
    except UnicodeDecodeError:
        MySqlite.write_log("ERROR", "SQL", "Failed to decrypt string - Unicode decode error",
                            "1004", "Failed to decrypt string")
        return "Decryption failed"
    except:
        MySqlite.write_log("ERROR", "SQL", "Failed to decrypt string",
                           "1004", "Failed to decrypt string")
        return "Decryption failed"

class InitSql():
    """
    Initialized SQL information files. This includes
    Type: File IO
    Relationship: NONE
    
    1. job_settings
    2. config
    3. job
    4. logs
    5. settings

    Use this at the beginning of the program to ensure all tables
    are created when at the beginning rather then runtime

    """
    @staticmethod
    def job_settings():
        """
        ensure job settings file exists and the table is created
        """
        MySqlite.create_db_file(settingsDirectory,job_settingsFile)
        # create settings table
        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS job_settings
                (ID TEXT, schedule TEXT, startTime TEXT, stopTime TEXT, 
                    retryCount TEXT, sampling TEXT, retention TEXT, lastJob TEXT, 
                        notifyEmail TEXT, heartbeatInterval TEXT, user TEXT, 
                        password TEXT, backupPath TEXT)''')
            # Close connection
        conn.commit()
    @staticmethod
    def config_files():
        """ 
        Ensure config file exists and the table is created
        """
        MySqlite.create_db_file(settingsDirectory,configFile)
           # create log table
        conn = sqlite3.connect(settingsDirectory+configFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS config
                                    (ID TEXT, tenantSecret TEXT, Address TEXT)''')
        # close connection
        conn.commit()
        conn.close()

    @staticmethod
    def job_files():
        """
        Ensure job file exists and the table is created
        """
        MySqlite.create_db_file(settingsDirectory,jobFile)
           # create log table
        conn = sqlite3.connect(settingsDirectory+jobFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS job
             (ID TEXT, Title TEXT, created TEXT, 
                configID TEXT, settingsID TEXT)''')
        # close connection
        conn.commit()
        conn.close()

    @staticmethod
    def log_files():
        """
        Ensure log file exists and the table is created
        """
        MySqlite.create_db_file(logdirectory,logpath)
           # create log table
        conn = sqlite3.connect(logdirectory+logpath)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS logs
        (id TEXT,severity TEXT, subject TEXT, message TEXT, code TEXT, date TEXT)''')
        # close connection
        conn.commit()
        conn.close()

    @staticmethod
    def load_settings():
        """
        Loads settings into memory
        """
        MySqlite.create_db_file(settingsDirectory,SETTINGS_PATH)
        # create settings table
        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''Select * from settings''')
        settings = cursor.fetchall()
        result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
        capture_output=True, text=True,check=True,shell=True) # enc with uuid
        output = result.stdout.strip()
        output = output.split('\n\n', 1)[-1]
        output = output[:32]
        global memory_settings
        for setting in settings:
            sett = decrypt_string(output,setting[1])
            memory_settings[setting[0]] =sett.rstrip()
        # Close connection
        conn.commit()
        conn.close()
    @staticmethod
    def settings():
        """
        Ensure settings file exists and the table is created
        """
        MySqlite.create_db_file(settingsDirectory,"\\Settings.db")
        # create settings table
        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS settings
                            (setting TEXT, value TEXT)''')
        # Close connection
        conn.commit()
        conn.close()


    def __init__(self):
        """
        Initializing the initsql class will ensure all tables are created
        """
        # initialize all tables when the object is created
        InitSql.log_files()
        InitSql.settings()
        InitSql.load_settings()
        InitSql.job_files()
        InitSql.config_files()
        InitSql.job_settings()
