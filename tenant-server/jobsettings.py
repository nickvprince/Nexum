"""
# Program: Tenant-server
# File: jobsettings.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the JobSettings class. This class is used
# to hold job setting information


# Class Types: 
#               1. JobSettings - Entity

"""
# pylint: disable= import-error, unused-argument
class JobSettings():
    """
    Holds job setting information
    type: entity
    Relationship: Job -> Job Settings 1..1
    """
    id = None
    schedule = None
    start_time = None
    stop_time = None
    retry_count = None
    sampling= None
    retention = None
    last_job = None
    notify_email = None
    heartbeat_interval = None
    backup_path = None
    user = None
    password = None

    # pylint: disable=missing-function-docstring
    # Getters and setters

    # Getters
    def get_user(self):
        return self.user
    def get_password(self):
        return self.password
    def set_backup_path(self, backup_path_in):
        self.backup_path = backup_path_in
    def get_backup_path(self):
        return self.backup_path
    def get_id(self):
        return self.id
    def get_schedule(self):
        return self.schedule
    def get_start_time(self):
        return self.start_time
    def get_stop_time(self):
        return self.stop_time
    def get_retry_count(self):
        return self.retry_count
    def get_sampling(self):
        return self.sampling
    def get_retention(self):
        return self.retention
    def get_last_job(self):
        return self.last_job
    def get_notify_email(self):
        return self.notify_email
    def get_heartbeat_interval(self):
        return self.heartbeat_interval

    # Setters
    def set_username(self, user_in):
        self.user = user_in
    def set_password(self, password_in):
        self.password = password_in
    def set_id(self, incoming_id):
        self.id = incoming_id
    def set_schedule(self, schedule_in):
        self.schedule = schedule_in
    def set_start_time(self, start_time_in):
        self.start_time = start_time_in
    def set_stop_time(self, stop_time_in):
        self.stop_time = stop_time_in
    def set_retry_count(self, retry_count_in):
        self.retry_count = retry_count_in
    def set_sampling(self, sampling_in):
        self.sampling = sampling_in
    def set_retention(self, retention_in):
        self.retention = retention_in
    def set_last_job(self, last_job_in):
        self.last_job = last_job_in
    def set_notify_email(self, notify_email_in):
        self.notify_email = notify_email_in
    def set_heartbeat_interval(self, heartbeat_interval_in):
        self.heartbeat_interval = heartbeat_interval_in
    #pylint: enable=missing-function-docstring
