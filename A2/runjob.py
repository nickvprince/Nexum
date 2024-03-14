from logger import Logger
import subprocess
import time
import job
import threading

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
                self.job_running_var = False
                # set job status to killed

            elif self.job_pending is True and self.stop_job_var is False : # Run the job if a job is pending. If the job is not stopped state
                # run the job
                self.job_pending = False # set job pending to false since it was just run
                command='wbadmin start backup -backupTarget:'+LOCAL_JOB.get_settings().get_backup_path()+' -include:C: -allCritical -vssFull -quiet -user:'+LOCAL_JOB.get_settings().get_user()+' -password:'+LOCAL_JOB.get_settings().get_password()
                p=subprocess.Popen(['powershell.exe', command])
                time.sleep(10)
                p.kill()

                # set job status to running
                self.job_running_var = True # set job running to true

            time.sleep(5)
            Logger.debug_print("Check backup status schedule here and run accordingly")
            # check if time has passed since it should have run
            if LOCAL_JOB.settings.start_time is None or LOCAL_JOB.settings.stop_time is None:
                LOCAL_JOB.settings.start_time = ""
                LOCAL_JOB.settings.stop_time = ""
            if (LOCAL_JOB.settings.start_time < time.asctime()) and (LOCAL_JOB.settings.stop_time > time.asctime()):
                Logger.debug_print("Job Triggered by time")
                command='wbadmin start backup -backupTarget:'+LOCAL_JOB.get_settings().get_backup_path()+' -include:C: -allCritical -vssFull -quiet -user:'+LOCAL_JOB.get_settings().get_user()+' -password:'+LOCAL_JOB.get_settings().get_password()
                p = subprocess.Popen(['powershell.exe', command])

                time.sleep(10)
                p.kill()
                # Run the Job


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
