"""
# Program: Tenant-Client
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
# pylint: disable= import-error, unused-argument
import conf
import jobsettings
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
    config = conf.Configuration(0,"","")
    settings = jobsettings.JobSettings()

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
