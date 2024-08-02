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
import base64
import datetime
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import requests


#pylint: disable=bare-except,line-too-long,broad-except
current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
settingsDirectory = os.path.join(__file__.rsplit("sql.py",1)[0],"../settings") # directory for settings
SETTINGS_PATH= os.path.join(
    settingsDirectory+'/settings.db') # path to the settings database
jobFile=os.path.join('/settings.db')
configFile=os.path.join('/settings.db')
job_settingsFile=os.path.join('/settings.db')
logdirectory = os.path.join(current_dir,'../logs') # directory for logs
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
def create_db_file(directory,path):
    """
    create the database file if it does not exist and the folder for it
    """
    try:
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
    except Exception as e:
        MySqlite.write_log("ERROR", "MySqlite", "Error creating db file - "+str(e), 500, datetime.datetime.now())

  # encrypt a string using AES
@staticmethod
def encrypt_string(password, string):
    """
    Encrypt a string with AES-256 bit encryption
    """
    try:
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
    except Exception as e:
        MySqlite.write_log("ERROR", "MySqlite", "Error encrypting string - "+str(e), 500, datetime.datetime.now())

# decrypt a string using AES
@staticmethod
def decrypt_string(password, string):
    """
    Decrypt a string with AES-256 bit decryption
    """
    try:
        # Pad the password to be 16 bytes long
        password_hashed = str(password).ljust(16).encode('utf-8')

        # Create a new AES cipher with the password as the key
        cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
        decryptor = cipher.decryptor()

        # Decode the string from base64
        decoded_string = base64.b64decode(string)

        # Decrypt the string using AES
        decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()
    except:
        return "Decryption failed"
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
    def delete_client(input_id:int):
        """
        Delete the client from the database
        """
        try:
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute("DELETE FROM clients WHERE id = ?", (input_id,))
            conn.commit()

            cursor.execute("DELETE FROM heartbeat WHERE id = ?", (input_id,))
            conn.commit()

            cursor.execute("DELETE FROM job WHERE ID = ?", (input_id,))
            conn.commit()

            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error deleting client - "+str(e), 500, datetime.datetime.now())

    @staticmethod
    def get_last_checkin(input_id):
        """
        Get the last checkin time from the settings database
        """
        try:
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute('''SELECT lastCheckin FROM heartBeat WHERE id = ?''', (input_id,))
            result = cursor.fetchone()
            conn.close()
            if result :
                return result[0]
            else:
                return None
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error getting last checkin - "+str(e), 500, datetime.datetime.now())
            return None

    @staticmethod
    def get_heartbeat_missed_tolerance(input_id):
        """
        Get the missed notify count from the settings database
        """
        try:
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute('''SELECT missedNotifyCount FROM heartBeat WHERE id = ?''', (input_id,))
            result = cursor.fetchone()
            conn.close()
            if result:
                return result[0]
            else:
                return -1
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error getting heartbeat missed tolerance - "+str(e), 500, datetime.datetime.now())
            return -1

    @staticmethod
    def write_heartbeat(input_id, interval, last_checkin, missed_notify_count):
        """
        Write a heartbeat to the settings database
        """
        try:
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute('''INSERT INTO heartBeat (id, interval, lastCheckin, missedNotifyCount)
                            VALUES (?, ?, ?, ?)''',
                            (input_id, interval, last_checkin, missed_notify_count))
            conn.commit()
            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error writing heartbeat - "+str(e), 500, datetime.datetime.now())
    @staticmethod
    def update_heartbeat_time(input_id):
        """
        Update the heartbeat time in the settings database
        """
        try:
            current_time = datetime.datetime.now()
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute('''SELECT id FROM heartBeat WHERE id = ?''', (input_id,))
            result = cursor.fetchone()
            if result:
                identification = result[0]
                cursor.execute('''UPDATE heartBeat SET lastCheckin = ? WHERE id = ?''', (current_time, identification))
                # Do something with the retrieved values
            else:
                # Handle the case when no record is found
                cursor.execute('''INSERT INTO heartbeat (id, interval, lastCheckin, missedNotifyCount)
                                VALUES (?, ?, ?, ?)''',
                (input_id, 5, current_time, 3)) # default missednotify of 3 and default interval of 5
            conn.commit()
            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error updating heartbeat time - "+str(e), 500, datetime.datetime.now())


    @staticmethod
    def delete_backup_server(identification:int):
        """
        Delete a backup server from the database
        """
        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute("DELETE FROM backup_servers WHERE id = ?", (identification,))
            conn.commit()
            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error deleting backup server - "+str(e), 500, datetime.datetime.now())

    @staticmethod
    def edit_backup_server(identification:int, path, username, password, name):
        """
        Edit a backup server in the database
        """
        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''UPDATE backup_servers SET path = ?, username = ?, password = ?, name = ? WHERE id = ?''',
            (path, username, password, name, identification))
            conn.commit()
            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error editing backup server - "+str(e), 500, datetime.datetime.now())

    @staticmethod
    def write_backup_server( name, path, username, password):
        """
        Write a backup server to the database
        """
        # get the next id

        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''SELECT MAX(id) FROM backup_servers''')
            result = cursor.fetchone()[0]
            if result is not None:
                identification = int(result) + 1
            else:
                identification = 1
            conn.close()

            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''INSERT INTO backup_servers (id, path, username, password, name)
                        VALUES (?, ?, ?, ?, ?)''',
                        (identification, path, username, password, name))
            conn.commit()
            conn.close()
            return identification
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error writing backup server - "+str(e), 500, datetime.datetime.now())
            return -1
    @staticmethod
    def get_backup_server(identification:int):
        """
        Get a backup server from the database
        """
        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''SELECT * FROM backup_servers WHERE id = ?''', (identification,))
            result = cursor.fetchone()
            conn.close()
            return result
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error getting backup server - "+str(e), 500, datetime.datetime.now())
            return None

    @staticmethod
    def load_clients():
        """
        Load clients from the database
        """
        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''SELECT * FROM clients''')
            clients = cursor.fetchall()
            conn.close()

            return clients
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error loading clients - "+str(e), 500, datetime.datetime.now())
            return None

    @staticmethod
    def write_log(severity, subject, message, code, date):
        """ 
        Write a log to the database
        """
        try:
            conn = sqlite3.connect(logdirectory+logpath)
            identification = 0
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
        except Exception as e:
            print(e)
    @staticmethod
    def get_client_uuid(client_id):
        """
        Get the uuid of a client
        """
        try:
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute('''SELECT uuid FROM clients WHERE id = ?''', (client_id,))
            result = cursor.fetchone()
            conn.close()
            return result[0]
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error getting client uuid - "+str(e), 500, datetime.datetime.now())
            return None
    @staticmethod
    def write_setting(setting, value):
        """
        Write a setting to the database
        """
        result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
        capture_output=True, text=True,check=True,shell=True)
        output = result.stdout.strip()
        output = output.split('\n\n', 1)[-1]
        output = output[:32]


        value = encrypt_string(output,value).rstrip()

        try:
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
            existing_value = cursor.fetchone()
            if existing_value:
                cursor.execute('''UPDATE settings SET value = ? WHERE setting = ?''', (value, setting))
            else:
                cursor.execute('''INSERT INTO settings (setting, value) VALUES (?, ?)''',
                            (setting, value))
            conn.commit()
            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error writing setting - "+str(e), 500, datetime.datetime.now())

        if setting == "Status":
            header ={
                "Content-Type":"application/json",
                "apikey":MySqlite.read_setting("apikey")
            }
            content = {
                "client_id": MySqlite.read_setting("CLIENT_ID"),
                "uuid": MySqlite.read_setting("uuid"),
                "status": convert_device_status()

            }

            try:
                server_address = MySqlite.read_setting("msp_server_address")
                msp_port = MySqlite.read_setting("msp-port")
                protocol = r"https://"

                response = requests.put(f"{protocol}{server_address}:{msp_port}/api/DataLink/Update-Device-Status", headers=header, json=content,timeout=5,verify=False)
                if response.status_code == 200:
                    return 200
            except Exception as e:
                print(e)

            else:
                return 500


    @staticmethod
    def read_setting(setting):
        """
        Read a setting from the database
        """
        try:
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
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
    def get_next_client_id():
        """
        Get the next client id
        """
        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''SELECT MAX(id) FROM clients''')
            result = cursor.fetchone()[0]
            if result is not None:
                identification = int(result) + 1
            else:
                identification = 1
            conn.close()
            return identification
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error getting next client id - "+str(e), 500, datetime.datetime.now())
            return None

    @staticmethod
    def get_client(identification:int):
        """
        Get a client from the database
        """
        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''SELECT * FROM clients WHERE id = ?''', (identification,))
            result = cursor.fetchone()
            conn.close()
            return result
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error getting client - "+str(e), 500, datetime.datetime.now())
            return None
    @staticmethod
    def update_client(client):
        """
        Update a client in the database
        """
        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''UPDATE clients SET Name = ?, Address = ?, Port = ?, Status = ?, MAC = ? WHERE id = ?''',
            (client[1], client[2], client[3], client[4], client[5], client[0]))
            conn.commit()
            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error updating client - "+str(e), 500, datetime.datetime.now())

    @staticmethod
    def write_client(identification, name, address, port, status, mac, uuid):
        """
        Write a client to the database
        """
        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''SELECT Address FROM clients WHERE Address = ?''', (address,))
            existing_address = cursor.fetchone()
            if existing_address:
                return 500
            cursor.execute('''INSERT INTO clients (id, Name, Address, Port, Status, MAC,uuid)
                        VALUES (?, ?, ?, ?, ?, ?,?)''',
                        (identification, name, address, port, status, mac,uuid))
            conn.commit()
            conn.close()
            MySqlite.write_heartbeat(identification, 5, datetime.datetime.now(), 3)
            return 200
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Error writing client - "+str(e), 500, datetime.datetime.now())
            return 500
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
    def heartbeat():
        """
        Ensure heartbeat file exists and the table is created
        """
        try:
            create_db_file(settingsDirectory,job_settingsFile)
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''CREATE TABLE IF NOT EXISTS heartbeat
                            (id TEXT, interval TEXT, lastCheckin TEXT, missedNotifyCount TEXT)''')
            conn.commit()
            conn.close()
            MySqlite.write_log("INFO", "MySqlite", "HeartBeat table created", 200, datetime.datetime.now())
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Heartbeat table not created - "+str(e), 500, datetime.datetime.now())
    @staticmethod
    def job_settings():
        """
        ensure job settings file exists and the table is created
        """
        try:
            create_db_file(settingsDirectory,job_settingsFile)
            # create settings table
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('''CREATE TABLE IF NOT EXISTS job_settings
                (ID TEXT, schedule TEXT, startTime TEXT, stopTime TEXT, 
                    retryCount TEXT, sampling TEXT, retention TEXT, lastJob TEXT, 
                        notifyEmail TEXT, heartbeatInterval TEXT,path TEXT,username TEXT,password TEXT)''')
            # Close connection
            conn.commit()
            conn.close()
            MySqlite.write_log("INFO", "MySqlite", "Job Settings table created", 200, datetime.datetime.now())
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Job settings table not created - "+str(e), 500, datetime.datetime.now())
    @staticmethod
    def clients():
        """ 
        ensure clients db table is created
        """
        try:
            create_db_file(settingsDirectory,job_settingsFile)
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            # drop table
            #cursor.execute('''DROP TABLE IF EXISTS clients''')
            cursor.execute('''CREATE TABLE IF NOT EXISTS clients
                            (id TEXT, Name TEXT, Address TEXT, Port TEXT, Status TEXT, MAC TEXT, uuid TEXTS)''')
            # Close connection
            conn.commit()
            conn.close()
            MySqlite.write_log("INFO", "MySqlite", "clients table created", 200, datetime.datetime.now())
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "clients table not created - "+str(e), 500, datetime.datetime.now())
    @staticmethod
    def config_files():
        """ 
        Ensure config file exists and the table is created
        """
        try:
            create_db_file(settingsDirectory,configFile)
            # create log table
            conn = sqlite3.connect(settingsDirectory+configFile)
            cursor = conn.cursor()
            cursor.execute('''CREATE TABLE IF NOT EXISTS config
                                        (ID TEXT, tenantSecret TEXT, Address TEXT)''')
            # close connection
            conn.commit()
            conn.close()
            MySqlite.write_log("INFO", "MySqlite", "config table created", 200, datetime.datetime.now())
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "config table not created - "+str(e), 500, datetime.datetime.now())

    @staticmethod
    def job_files():
        """
        Ensure job file exists and the table is created
        """
        try:
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
            MySqlite.write_log("INFO", "MySqlite", "Job table created", 200, datetime.datetime.now())
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Job table not created - "+str(e), 500, datetime.datetime.now())

    @staticmethod
    def log_files():
        """
        Ensure log file exists and the table is created
        """
        try:
            create_db_file(logdirectory,logpath)
            # create log table
            conn = sqlite3.connect(logdirectory+logpath)
            cursor = conn.cursor()
            cursor.execute('''CREATE TABLE IF NOT EXISTS logs
            (id TEXT, severity TEXT, subject TEXT, message TEXT, code TEXT, date TEXT)''')
            # close connection
            conn.commit()
            conn.close()
            MySqlite.write_log("INFO", "MySqlite", "logs table created", 200, datetime.datetime.now())
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "logs table not created - "+str(e), 500, datetime.datetime.now())

    @staticmethod
    def settings():
        """
        Ensure settings file exists and the table is created
        """
        try:
            create_db_file(settingsDirectory,"\\Settings.db")
            # create settings table
            conn = sqlite3.connect(SETTINGS_PATH)
            cursor = conn.cursor()
            cursor.execute('''CREATE TABLE IF NOT EXISTS settings
                                (setting TEXT, value TEXT)''')
            # Close connection
            conn.commit()
            conn.close()
            MySqlite.write_log("INFO", "MySqlite", "Job Settings table created", 200, datetime.datetime.now())
        except Exception as e:
            MySqlite.write_log("ERROR", "MySqlite", "Job settings table not created - "+str(e), 500, datetime.datetime.now())

    @staticmethod
    def backup_servers():
        """
        Ensure backup servers file exists and the table is created
        """
        try:
            # create db file settingsDirectory\\settings
            create_db_file(settingsDirectory,job_settingsFile)
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            # creat table for backup servers : id,smb path,user,pass,name
            cursor.execute('''CREATE TABLE IF NOT EXISTS backup_servers
                            (id TEXT, path TEXT, username TEXT, password TEXT, name TEXT)''')
            MySqlite.write_log("INFO", "MySqlite", "backup server table created", 200, datetime.datetime.now())
        except:
            MySqlite.write_log("ERROR", "MySqlite", "Backup servers table not created", 500, datetime.datetime.now())
    def __init__(self):
        # initialize all tables when the object is created
        InitSql.log_files()
        InitSql.settings()
        InitSql.job_files()
        InitSql.config_files()
        InitSql.job_settings()
        InitSql.clients()
        InitSql.heartbeat()
        InitSql.backup_servers()
        MySqlite.write_log("INFO", "MySqlite", "All tables created finishing sql INIT", 200, datetime.datetime.now())
