"""
# Program: Tenant-server
# File: initsql.py
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

import os
import sqlite3
import subprocess
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import base64

current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
settingsDirectory = os.path.join(current_dir, '..\\settings') # directory for settings
SETTINGS_PATH= os.path.join(current_dir, 
    settingsDirectory+'\\settings.db') # path to the settings database
jobFile=os.path.join('/settings.db')
configFile=os.path.join('/settings.db')
job_settingsFile=os.path.join('/settings.db')
logdirectory = os.path.join(current_dir,'../logs') # directory for logs
logpath = os.path.join('/log.db') # path to the log database
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
    string = string.ljust((len(string) // 16 + 1) * 16).encode('utf-8')

    # Encrypt the string using AES
    encrypted_string = encryptor.update(string) + encryptor.finalize()

    # Encode the encrypted string in base64
    encoded_string = base64.b64encode(encrypted_string)

    return encoded_string.decode('utf-8')

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

    # Decrypt the string using AES
    decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()
    try:
        return decrypted_string.decode('utf-8')
    except UnicodeDecodeError:
        return "Decryption failed"
    except:
        return "Decryption failed"
     
class MySqlite():
    """
    Class to interact with the sqlite database
    Type: File IO
    """
    @staticmethod
    def load_clients():
        """
        Load clients from the database
        """
        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('''SELECT * FROM clients''')
        clients = cursor.fetchall()
        conn.close()
        return clients
    @staticmethod 
    def write_log(severity, subject, message, code, date):
        """ 
        Write a log to the database
        """
        conn = sqlite3.connect(logdirectory+logpath)
        id = 0
        cursor = conn.cursor()


        cursor.execute('''SELECT MAX(id) FROM logs''')
        result = cursor.fetchone()[0]
        if result is not None:
            id = int(result) + 1
        else:
            id = 1


        cursor.execute('''INSERT INTO logs (id,severity, subject, message, code, date)
                    VALUES (?,?, ?, ?, ?, ?)''', (id, severity, subject, message, code, date))
        conn.commit()
        conn.close()
    @staticmethod
    def write_setting(setting, value):
        """
        Write a setting to the database
        """
        result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'], capture_output=True, text=True)
        output = result.stdout.strip()
        output = output.split('\n\n', 1)[-1]
        output = output[:24]

        value = encrypt_string(output,value)

        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
        existing_value = cursor.fetchone()
        if existing_value:
            cursor.execute('''UPDATE settings SET value = ? WHERE setting = ?''', (value, setting))
        else:
            cursor.execute('''INSERT INTO settings (setting, value) VALUES (?, ?)''', (setting, value))
        conn.commit()
        conn.close()

    @staticmethod
    def read_setting(setting):
        """
        Read a setting from the database
        """

        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
        value = cursor.fetchone()[0]
        conn.close()
        result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'], capture_output=True, text=True) # enc with uuid
        output = result.stdout.strip()
        output = output.split('\n\n', 1)[-1]
        output = output[:24]
        value = decrypt_string(output,value)
        return value.rstrip()
    @staticmethod 
    def get_next_client_id():
        """
        Get the next client id
        """
        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('''SELECT MAX(id) FROM clients''')
        result = cursor.fetchone()[0]
        if result is not None:
            id = int(result) + 1
        else:
            id = 1
        conn.close()
        return id
    @staticmethod
    def write_client(id, name, address, port, status, mac):
        """
        Write a client to the database
        """
        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('''SELECT Address FROM clients WHERE Address = ?''', (address,))
        existing_address = cursor.fetchone()
        if existing_address:
            return 500
        cursor.execute('''INSERT INTO clients (id, Name, Address, Port, Status, MAC)
                    VALUES (?, ?, ?, ?, ?, ?)''', (id, name, address, port, status, mac))
        conn.commit()
        conn.close()
        return 200
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
        create_db_file(settingsDirectory,job_settingsFile)
        # create settings table
        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS job_settings
            (ID TEXT, schedule TEXT, startTime TEXT, stopTime TEXT, 
                retryCount TEXT, sampling TEXT, retention TEXT, lastJob TEXT, 
                    notifyEmail TEXT, heartbeatInterval TEXT)''')
        # Close connection
        conn.commit()
        conn.close()
    @staticmethod
    def clients():
        """ 
        ensure clients db table is created
        """
        create_db_file(settingsDirectory,job_settingsFile)
        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS clients
                        (id TEXT, Name TEXT, Address TEXT, Port TEXT, Status TEXT, MAC TEXT)''')
        # Close connection
        conn.commit()
        conn.close()
    @staticmethod
    def config_files():
        """ 
        Ensure config file exists and the table is created
        """
        create_db_file(settingsDirectory,configFile)
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
        create_db_file(settingsDirectory,jobFile)
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
        create_db_file(logdirectory,logpath)
           # create log table
        conn = sqlite3.connect(logdirectory+logpath)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS logs
        (id TEXT, severity TEXT, subject TEXT, message TEXT, code TEXT, date TEXT)''')
        # close connection
        conn.commit()
        conn.close()

    @staticmethod
    def settings():
        """
        Ensure settings file exists and the table is created
        """
        create_db_file(settingsDirectory,"\\Settings.db")
        # create settings table
        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''CREATE TABLE IF NOT EXISTS settings
                            (setting TEXT, value TEXT)''')
        # Close connection
        conn.commit()
        conn.close()


    def __init__(self):
        # initialize all tables when the object is created
        InitSql.log_files()
        InitSql.settings()
        InitSql.job_files()
        InitSql.config_files()
        InitSql.job_settings()
