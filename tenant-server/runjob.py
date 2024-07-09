"""
# Program: Tenant-server
# File: runjob.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the runjob class. This class is used 
# to run the job assigned to this computer and manage the job

# Class Types: 
#               1. RunJob - Controller

"""

# pylint: disable= import-error, unused-argument
import subprocess
import time
import threading
import job
from logger import Logger
from jobsettings import JobSettings
from flask import request
import requests
import json
import base64

from sql import MySqlite

from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
# pylint: disable=line-too-long



LOCAL_JOB:job = job.Job() # job assigned to this computer


        
@staticmethod
def unpad(ct):
    return ct[:-ct[-1]]
@staticmethod
def shuffle():
    """
    Shuffles the api keys
    """
    api=MySqlite.read_setting("apikey")
    msp=MySqlite.read_setting("msp_api")
    # for char in range mspapi-1 password = msp_api[i]+api[i+1]
    password = ""
    for i in range(len(msp)):
       password+=msp[i]+api[i]
    return password
@staticmethod
def decrypt_password(password:str):
        encryption_key=shuffle()

        # only take first 32 chars
        encryption_key=encryption_key[:32]

        cipher = Cipher(algorithms.AES(encryption_key.encode("utf-8")), modes.ECB(), backend=default_backend())
        decryptor = cipher.decryptor()
        

        # Decode the string from base64
        decoded_string = base64.b64decode(password)
        
        # Decrypt the string using AES
        decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()
        decrypted_string = str(decrypted_string.decode("utf-8"))
        #rstrip \0b
        decrypted_string = decrypted_string.rstrip("\x0b")
        return str(decrypted_string)
class RunJob():
    """
    Class to run the job assigned to this computer and manage the job
    Type: Controller
    Relationship: LOCAL_JOB 1..1
    """
    job_pending = False # Has a job been missed or been queued
    leave = False # triggers internal exit
    thread = None # thread that is running the job
    stop_job_var = False # stop the job
    kill_job_var = False # stop the job
    job_running_var = False

    def run(self):
        """
        Runs the backup job. This is the main function that runs the backup job
        the backup job is run by filling the LOCAL_JOB with the job assigned to this computer
        The LOCAL_JOB may be assigned using the API class to get it from the tenant server API
        @param: self
        """
        while self.leave is False: # As long as the job is not terminated
            if self.kill_job_var is True:
                # stop the job

                url = 'http://127.0.0.1:5004/stop_job_service'
                headers = {
                    "apikey": MySqlite.read_setting("apikey"),
                    "Content-Type": "application/json"
                }
                try:
                    response = requests.post(url, headers=headers,timeout=15)
                    if response.json()["result"] == "{[b'wbadmin 1.0 - Backup command-line tool\\r\\n', b'(C) Copyright Microsoft Corporation. All rights reserved.\\r\\n', b'\\r\\n', b'ERROR - The user name or password is unexpected because the backup location \\r\\n', b'is not a remote shared folder.\\r\\n', b'\\r\\n']}":
                        MySqlite.write_setting("job_status","NotStarted")
                        MySqlite.write_setting("Status","Online")
                    else:
                        MySqlite.write_setting("job_status","NotStarted")
                        MySqlite.write_setting("Status","Online")
                except ConnectionError:
                    MySqlite.write_setting("job_status","NotStarted")
                    MySqlite.write_setting("Status","ServiceOffline")
                except TimeoutError:
                    MySqlite.write_setting("job_status","NotStarted")
                    MySqlite.write_setting("Status","ServiceOffline")
                except Exception as e:
                    MySqlite.write_setting("job_status","NotStarted")
                    MySqlite.write_setting("Status","ServiceOffline")
                    Logger.debug_print("Error: "+str(e))
                time.sleep(2)
                # check response for what happened
                self.job_running_var = False
                # set job status to killed
                

            elif self.job_pending is True and self.stop_job_var is False : # Run the job if a job is pending. If the job is not stopped state
                # run the job
                self.job_pending = False # set job pending to false since it was just run
                command='-backupTarget:'+LOCAL_JOB.get_settings()[10]+' -include:C: -allCritical -vssFull -quiet -user:'+LOCAL_JOB.get_settings()[11]+' -password:'+decrypt_password(LOCAL_JOB.get_settings()[12])
                #command='-backupTarget:'+"d:"+' -include:C: -allCritical -vssFull -quiet'

                url = 'http://127.0.0.1:5004/start_job_service'
                body = {
                    "start_job_commands": str(command)
                }
                headers = {
                    "apikey": MySqlite.read_setting("apikey"),
                    "Content-Type": "application/json"
                }
                try:
                    response = requests.post(url, data=json.dumps(body), headers=headers,timeout=15)

                    if response.json()["result"] == "{[b'wbadmin 1.0 - Backup command-line tool\\r\\n', b'(C) Copyright Microsoft Corporation. All rights reserved.\\r\\n', b'\\r\\n', b'ERROR - The user name or password is unexpected because the backup location \\r\\n', b'is not a remote shared folder.\\r\\n', b'\\r\\n']}":
                        MySqlite.write_setting("job_status","NotStarted")
                        MySqlite.write_setting("Status","Online")
                        self.job_running_var = False # set job running to true
                    else:
                        MySqlite.write_setting("job_status","InProgress")
                        MySqlite.write_setting("Status","Online")
                        self.job_running_var = True # set job running to true
                except TimeoutError:
                    MySqlite.write_setting("job_status","NotStarted")
                    MySqlite.write_setting("Status","ServiceOffline")
                    self.job_running_var = False # set job running to true
                except ConnectionError:
                    MySqlite.write_setting("job_status","NotStarted")
                    MySqlite.write_setting("Status","ServiceOffline")
                    self.job_running_var = False # set job running to true
                except Exception as e:
                    MySqlite.write_setting("job_status","NotStarted")
                    MySqlite.write_setting("Status","ServiceOffline")
                    Logger.debug_print("Error: "+str(e))
                    self.job_running_var = False # set job running to true
                # check response for what happened
                # set job status to running
               


                

            time.sleep(5)
            Logger.debug_print("Check backup status schedule here and run accordingly")
            # check if time has passed since it should have run
            if LOCAL_JOB.get_settings()is not None:
                if LOCAL_JOB.get_settings()[2] is None or LOCAL_JOB.get_settings()[3] is None:
                    LOCAL_JOB.get_settings().start_time = ""
                    LOCAL_JOB.get_settings().stop_time = ""
                if (LOCAL_JOB.get_settings()[2] < time.asctime()) and (LOCAL_JOB.get_settings()[3] > time.asctime()):
                    # check if backup allowed to run today
                    Logger.debug_print("Job Triggered by time")
                    command='-backupTarget:'+LOCAL_JOB.get_settings().get_backup_path()+' -include:C: -allCritical -vssFull -quiet -user:'+LOCAL_JOB.get_settings()[11]+' -password:'+decrypt_password(LOCAL_JOB.get_settings()[12])
                    url = 'http://127.0.0.1:5004/start_job_service'
                    body = {
                        "start_job_commands": str(command)
                    }
                    headers = {
                        "apikey": MySqlite.read_setting("apikey"),
                        "Content-Type": "application/json"
                    }
                    try:
                        response = requests.post(url, data=json.dumps(body), headers=headers,timeout=15)
                        MySqlite.write_setting("job_status","InProgress")
                    except TimeoutError:
                        MySqlite.write_setting("job_status","NotStarted")
                        MySqlite.write_setting("Status","ServiceOffline")
                    except Exception as e:
                        Logger.debug_print("Error: "+str(e))

                    # set job status to running
                    self.job_running_var = True
            else:
                pass


    def __init__(self):
        self.thread = threading.Thread(target=self.run)
        self.thread.daemon = True
        self.thread.start()
    def trigger_job(self):
        """
        Triggers the job to run
        """
        self.job_pending = True
    def enable_job(self):
        """
        Enables the job to run
        """
        self.stop_job_var = False
    def stop_job(self):
        """
        Sets the job_stop state to True thus stopping the job.
        This does not stop active jobs. It will only prevent new jobs from being started.
        """
        self.stop_job_var = True
    def kill_job(self):
        """
        Stops the currently running job
        """
        # stop the job
        self.kill_job_var = True

    @staticmethod
    def get_job():
        """
        Gets the job info
        """
        return LOCAL_JOB
