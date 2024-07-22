"""
# Program: Tenant-server
# File: job.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the Job class. This class is used 
# to hold the entire job information and its settings

# Class Types: 
#               1. Job - Entity

"""
# pylint: disable= import-error, unused-argument,line-too-long
import conf
import jobsettings
from sql import settingsDirectory, jobFile, configFile, job_settingsFile, sqlite3,MySqlite
import datetime


class Job():
    """
    Class that holds the entire job
    Type: entity
    Relationship: Job -> Configuration 1..1
    Relationship: Job -> Job Settings 1..1
    """
    id =  None
    title = None
    created = None
    config:conf = conf.Configuration(0,"","")
    settings:jobsettings = jobsettings.JobSettings()

    # pylint: disable=missing-function-docstring
    # Getters and setters

    # Getters
    def get_id(self):
        return self.id
    def get_title(self):
        return self.title
    def get_created(self):
        return self.created
    def get_config(self):
        return self.config
    def get_settings(self):
        return self.settings

    # Setters
    def set_id(self, incoming_id):
        self.id = incoming_id
    def set_title(self, title_in):
        self.title= title_in
    def set_created(self, created_in):
        self.created = created_in
    def set_config(self, config_in):
        self.config = config_in
    def set_settings(self, settings_in):
        self.settings = settings_in
    #pylint: enable=missing-function-docstring

    def save(self):
        """
        Saves the job to the database
        """
        
        MySqlite.write_log("INFO","JOB","Saving Job",0,"")
        conn1 = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor1 = conn1.cursor()
        cursor1.execute("DELETE FROM job_settings WHERE ID = '0'")
        cursor1.execute('INSERT INTO job_settings (ID, schedule, startTime, stopTime, retryCount, sampling, retention, lastJob, notifyEmail, heartbeatInterval,path,username,password)'
                        'VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?,?,?,?)', (self.settings.get_id(), self.settings.get_schedule(), self.settings.get_start_time(), self.settings.get_stop_time(), self.settings.get_retry_count(), self.settings.get_sampling(), self.settings.get_retention(), self.settings.get_last_job(), self.settings.get_notify_email(), self.settings.get_heartbeat_interval(),self.settings.get_backup_path(),self.settings.get_user(),self.settings.get_password()))
        conn1.commit()
        conn1.close()
        conn2 = sqlite3.connect(settingsDirectory+configFile)
        cursor2 = conn2.cursor()
        cursor2.execute("DELETE FROM config WHERE ID = '0'")
        cursor2.execute('INSERT INTO config (ID, tenantSecret, Address)'
                        'VALUES (?, ?, ?)', (self.config.get_id(), self.config.get_tenant_secret(), self.config.get_address()))
        conn2.commit()
        conn2.close()
        conn = sqlite3.connect(settingsDirectory+jobFile)
        cursor = conn.cursor()
        cursor.execute("DELETE FROM job WHERE ID = '0'")
        cursor.execute('INSERT INTO job (ID, Title, created, configID, settingsID)'
                        'VALUES (?, ?, ?, ?, ?)', (self.get_id(), self.get_title(), self.get_created(), self.config.get_id(), self.settings.get_id()))
        conn.commit()
        conn.close()
    def load(self,id_in):
        """
        Loads the job from the database
        """
        try:
            conn = sqlite3.connect(settingsDirectory+jobFile)
            cursor = conn.cursor()
            cursor.execute('SELECT * FROM job WHERE ID = ?', (id_in,))
            info = cursor.fetchone()
            self.set_id(info[0])
            self.set_title(info[1])
            self.set_created(info[2])
            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR","JOB","Error Loading Job - "+str(e),500,datetime.datetime.now())

        try:
            conn = sqlite3.connect(settingsDirectory+configFile)
            cursor = conn.cursor()
            cursor.execute('SELECT * FROM config WHERE ID = ?', ("0"))
            my_config = cursor.fetchone()
            conn.close()
        except Exception as e:
            MySqlite.write_log("ERROR","JOB","Error Loading Config - "+str(e),500,datetime.datetime.now())

        try:
            conn = sqlite3.connect(settingsDirectory+job_settingsFile)
            cursor = conn.cursor()
            cursor.execute('SELECT * FROM job_settings WHERE ID = ?', ("0"))
            my_settings = cursor.fetchone()
            conn.close()
            self.set_config(my_config)
            self.set_settings(my_settings)
        except Exception as e:
            MySqlite.write_log("ERROR","JOB","Error Loading Job Settings - "+str(e),500,datetime.datetime.now())

    def delete(self):
        """ 
        Deletes the job from the database
        """
        conn = sqlite3.connect(settingsDirectory+jobFile)
        cursor = conn.cursor()
        cursor.execute('DELETE FROM job WHERE ID = ?', (self.get_id(),))
        conn.commit()
        conn.close()

        conn = sqlite3.connect(settingsDirectory+configFile)
        cursor = conn.cursor()
        cursor.execute('DELETE FROM config WHERE ID = ?', (self.get_id(),))
        conn.commit()
        conn.close()

        conn = sqlite3.connect(settingsDirectory+job_settingsFile)
        cursor = conn.cursor()
        cursor.execute('DELETE FROM job_settings WHERE ID = ?', (self.get_id(),))
        conn.commit()
        conn.close()
