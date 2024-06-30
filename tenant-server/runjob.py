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
from sql import MySqlite

# pylint: disable=line-too-long



LOCAL_JOB:job = job.Job() # job assigned to this computer
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
                    "Content-Type": "application/json"
                }
                response = requests.post(url, headers=headers,timeout=15)
                time.sleep(10)
                # check response for what happened
                self.job_running_var = False
                # set job status to killed
                new_client = MySqlite.get_client(MySqlite.read_setting("CLIENT_ID"))
                new_client = list(new_client)
                new_client.remove(new_client[4])
                new_client.insert(4,"idle")
                MySqlite.update_client(new_client)

            elif self.job_pending is True and self.stop_job_var is False : # Run the job if a job is pending. If the job is not stopped state
                # run the job
                self.job_pending = False # set job pending to false since it was just run
               # command='-backupTarget:'+LOCAL_JOB.get_settings().get_backup_path()+' -include:C: -allCritical -vssFull -quiet -user:'+LOCAL_JOB.get_settings().get_user()+' -password:'+LOCAL_JOB.get_settings().get_password()
                command='-backupTarget:'+"d:"+' -include:C: -allCritical -vssFull -quiet'

                url = 'http://127.0.0.1:5004/start_job_service'
                body = {
                    "start_job_commands": str(command)
                }
                headers = {
                    "Content-Type": "application/json"
                }
                try:
                    response = requests.post(url, data=json.dumps(body), headers=headers,timeout=15)
                except Exception as e:
                    Logger.debug_print("Error: "+str(e))
                # check response for what happened
                # set job status to running
                
                new_client = MySqlite.get_client(MySqlite.read_setting("CLIENT_ID"))
                new_client = list(new_client)
                new_client.remove(new_client[4])
                new_client.insert(4,"running")
                MySqlite.update_client(new_client)


                self.job_running_var = True # set job running to true

            time.sleep(5)
            Logger.debug_print("Check backup status schedule here and run accordingly")
            # check if time has passed since it should have run
            if LOCAL_JOB.get_settings()is not None:
                if LOCAL_JOB.get_settings()[2] is None or LOCAL_JOB.get_settings()[3] is None:
                    LOCAL_JOB.get_settings().start_time = ""
                    LOCAL_JOB.get_settings().stop_time = ""
                if (LOCAL_JOB.get_settings()[2] < time.asctime()) and (LOCAL_JOB.get_settings()[3] > time.asctime()):
                    Logger.debug_print("Job Triggered by time")
                    command='-backupTarget:'+"d:"+' -include:C: -allCritical -vssFull -quiet'
                    # command='-backupTarget:'+LOCAL_JOB.get_settings().get_backup_path()+' -include:C: -allCritical -vssFull -quiet -user:'+LOCAL_JOB.get_settings().get_user()+' -password:'+LOCAL_JOB.get_settings().get_password()
                    p = subprocess.Popen(['powershell.exe', command],shell=True)

                    time.sleep(10)
                    p.kill()
                    # Run the Job
                    # set job status to running
                    if MySqlite.read_setting("CLIENT_ID") == "0":
                        pass
                    else:
                        new_client = MySqlite.get_client(MySqlite.read_setting("CLIENT_ID"))
                        new_client = list(new_client)
                        new_client.remove(new_client[4])
                        new_client.insert(4,"running")
                        MySqlite.update_client(new_client)
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
