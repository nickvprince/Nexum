"""
# Program: Tenant-Client
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
import jobsettings
from MySqlite import MySqlite


# pylint: disable=line-too-long,broad-except,global-variable-not-assigned



LOCAL_JOB = job.Job() # job assigned to this computer
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
                self.kill_job_var = False
                self.job_pending = False
                command = "wbadmin stop job -quiet"
                Logger.debug_print("Kill the Job here by running powershell script")
                p = subprocess.Popen(['powershell.exe', command])
                time.sleep(10)
                p.kill()
                MySqlite.write_setting("status","Idle")
                self.job_running_var = False
                # set job status to killed

            elif self.job_pending is True and self.stop_job_var is False : # Run the job if a job is pending. If the job is not stopped state
                # run the job
                self.job_pending = False # set job pending to false since it was just run
                command='wbadmin start backup -backupTarget:'+LOCAL_JOB.get_settings().get_backup_path()+' -include:C: -allCritical -vssFull -quiet -user:'+LOCAL_JOB.get_settings().get_user()+' -password:'+LOCAL_JOB.get_settings().get_password()

                p=subprocess.Popen(['powershell.exe', command])
                time.sleep(10)
                p.kill()
                MySqlite.write_setting("status","Running")
                # set job status to running
                self.job_running_var = True # set job running to true

            time.sleep(5)
            Logger.debug_print("Check backup status schedule here and run accordingly")
            # check if time has passed since it should have run
            try:
                if LOCAL_JOB.settings[2] is None or LOCAL_JOB.settings[3] is None:
                    LOCAL_JOB.settings[2] = ""
                    LOCAL_JOB.settings[3] = ""
                current_time = time.strftime("%H:%M")
                if (LOCAL_JOB.settings[2] <= current_time) and (LOCAL_JOB.settings[3] > current_time):
                    Logger.debug_print("Job Triggered by time")
                    # Check that it is allowed to run today
                    try:
                        command='wbadmin start backup -backupTarget:'+LOCAL_JOB.settings[12]+' -include:C: -allCritical -vssFull -quiet -user:'+LOCAL_JOB.settings[10]+' -password:'+LOCAL_JOB.settings[11]
                        p = subprocess.Popen(['powershell.exe', command])
                        time.sleep(10)
                        Logger.log("INFO", "RunJob", p.stdout.read(), "0000",time.asctime())
                        p.kill()
                    except Exception as e:
                        Logger.log("ERROR", "RunJob", e, "1007", time.asctime())
                    # Run the Job
            except:
                Logger.log("INFO", "RunJob", "Job may not be configured or failed to run", "1008", time.asctime())
                pass


    def __init__(self):
        global LOCAL_JOB
        LOCAL_JOB.load(0)
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
