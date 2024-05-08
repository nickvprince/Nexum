"""
# Program: Tenant-Client
# File: flaskserver.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the FlaskServer class. This class is used 
# to host the tenant-client REST API

# Class Types: 
#               1. FlaskServer - API

"""
# pylint: disable= import-error, global-statement,unused-argument

import os
import time
from flask import Flask, request,make_response
from security import Security, CLIENT_SECRET
from logger import Logger
from job import Job
from jobsettings import JobSettings
from conf import Configuration
RUN_JOB_OBJECT = None




# pylint: disable= bare-except
class FlaskServer():
    """
    Class to manage the server
    """

    website = Flask(__name__)

    @staticmethod
    def set_run_job_object(run_job_object):
        """
        Set the run job object
        """
        global RUN_JOB_OBJECT
        RUN_JOB_OBJECT = run_job_object

    @staticmethod
    def auth(recieved_client_secret, logger,identification):
        """     This is substituted with local clientSecret
        try:
            # open sql connection to 'NEXUM-SQL' and select * from Security where ID = id
            conn = pyodbc.connect('DRIVER={SQL Server};SERVER=NEXUM-SQL;
            DATABASE=your_database_name;Trusted_Connection=yes;')
            cursor = conn.cursor()
            query = "SELECT * FROM Security WHERE ID = ?"
            cursor.execute(query, (ID,))
            result = cursor.fetchone()

            # set salt to the result[0], pepper to result[1], and salt2 to result[2]
            salt = result[0]
            pepper = result[1]
            salt2 = result[2]

            # close the sql connection
            conn.close()
        except:
            return "405 Incorrect ID"

        """
        # A2 uses client secret unhashed to generate the password used to encrypt the data
        # The data is given salt, pepper, and salt2 then hashed.
        # the hash is then encypted then sent

        # to decrypt would be
        # decrypt the data
        # create a variable with the salt, pepper, and salt2 added to the client secret then hashed
        # compare the hash to the decrypted data
        # if they match the KEY is valid
        recieved_client_secret = Security.decrypt_client_secret(recieved_client_secret)

        temp = Security.sha256_string(CLIENT_SECRET)

        temp = Security.add_salt_pepper(temp, "salt", "pepricart", "salt2")



        if str(recieved_client_secret) != temp:
            logger.log("ERROR", "get_files", "Access denied",
            "405", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            return 405
        return 200




# -------------------------------------- GET ROUTES ------------------------------------------------
    @website.route('/get_files', methods=['GET'], )
    @staticmethod
    def get_files():
        """
        Server requests a path such as C: and returns a list of files and directories in that path
        requirement: json Body that includes 'path', clientSecret hashed with sha256, and a salt, 
        pepper, and salt2. It must also be encrypted with the pre-determined password and the ID 
        for the salt, pepper, and salt2 is required
        returns: a list of files and directories in the path
        if the clientSecret is incorrect returns "401 Access Denied"
        if the ID is incorrect returns "405 Incorrect ID"
        if the path is not a directory returns "404 Path is not a directory"
        if the path is not accessible returns "404 Path is not accessible"
        if the path is empty returns "404 Path is empty"
        if the path is not found returns "404 Path not found"
        else return 500 internal server error
        @param request: the request from the client
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the path from the json body
        path = data.get('path', '')
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        # pylint: enable=unused-variable, enable=invalid-name
        # check if the path exists
        elif not os.path.exists(path):
            logger.log("ERROR", "get_files", "Path not found",
            "404", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            code=404
            msg="Incorrect path"
        # check if the path is a directory
        elif not os.path.isdir(path):
            logger.log("ERROR", "get_files", "Path is not a directory",
            "404", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            code=404
            msg="Path is not a directory"
        # check if the path is accessible
        elif not os.access(path, os.R_OK):
            logger.log("ERROR", "get_files", "Path is not accessible",
            "404", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            code=404
            msg="Path is not accessible"
        # check if the path is empty
        elif not os.listdir(path):
            logger.log("ERROR", "get_files", "Path is empty",
            "404", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            code=404
            msg="Path is empty"
        # get the files and directories in the path
        # if error is returned, do not get file directories
        if code==0:
            try:
                files = os.listdir(path)
            except PermissionError:
                logger.log("ERROR", "get_files", "Permission error",
                "1005", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                code = 401
                msg = "Access Denied"
            except:
                logger.log("ERROR", "get_files", "General Error getting files",
                "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                code= 500
                msg = "Internal server error"
        else:
            return make_response(msg, code)
        # <-- Turn into a method

        return files
    # POST ROUTES

    @website.route('/get_job', methods=['GET'], )
    @staticmethod
    def get_job():
        """
        Gives Current Job Information
        """
        return "200 OK"

    @staticmethod
    def get_jobs():
        """
        Gives job information for all clients
        """
        return "200 OK"
    @website.route('/force_checkin', methods=['GET'], )
    @staticmethod
    def force_checkin():
        """
        Forces a heartbeat
        """
        return "200 OK"


    @website.route('/get_Status', methods=['GET'], )
    @staticmethod
    def get_job_status():
        """
        Gets the current status of running jobs or error state, version information etc
        """
        return "200 OK"

    @website.route('/get_version', methods=['GET'], )
    @staticmethod
    def get_version():
        """
        Gets version information from the client
        """
        return "200 ok"
# -------------------------------------- GET ROUTES ------------------------------------------------

# -------------------------------------- PUT ROUTES ------------------------------------------------
    @website.route('/start_job', methods=['PUT'], )
    @staticmethod
    def start_job():
        """
        Triggers the RunJob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.trigger_job()
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @website.route('/stop_job', methods=['PUT'], )
    @staticmethod
    def stop_job():
        """
        Triggers the stopjob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.stop_job()
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @website.route('/force_update', methods=['PUT'], )
    @staticmethod
    def force_update():
        """
        Forces the client to pull an update from the server
        """
        return "200 OK"

    @website.route('/enable_job', methods=['PUT'], )
    @staticmethod
    def enable_job():
        """
        Triggers the RunJob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.enable_job()
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @website.route('/kill_job', methods=['PUT'], )
    @staticmethod
    def kill_job():
        """
        Triggers the killjob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()

        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.kill_job()
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)
# -------------------------------------- PUT ROUTES ------------------------------------------------
# -------------------------------------- POST ROUTES -----------------------------------------------
    @website.route('/restore', methods=['POST'], )
    @staticmethod
    def restore():
        """
        Restores files or directories
        """
        return "200 OK"

    @website.route('/modify_job', methods=['POST'], )
    @staticmethod
    def modify_job():
        """
        Sets the current job to the new job. Or creates on if it does not exist
        """
        #pylint: disable=global-variable-not-assigned
        # disabled because it is used in RUN_JOB_OBJECT.set_job(job_to_save)
        global RUN_JOB_OBJECT
        #pylint: enable=global-variable-not-assigned
        logger=Logger()
        data = request.get_json()
        # get client secret from header
        secret = request.headers.get('clientSecret')
        identification = request.headers.get('ID')

        if FlaskServer.auth(secret, logger, identification) == 200:
            recieved_job = data.get(identification, '')
            # recieve settings as json
            recieved_settings = recieved_job.get('settings', '')

            job_to_save = Job()
            job_to_save.set_id(identification)
            job_to_save.set_title(recieved_job.get('title', ''))

            # create a settings object
            settings = JobSettings()
            settings.set_id(0)
            settings.set_schedule(recieved_settings.get('schedule', ''))
            settings.set_start_time(recieved_settings.get('startTime', ''))
            settings.set_stop_time(recieved_settings.get('stopTime', ''))
            settings.set_retry_count(recieved_settings.get('retryCount', ''))
            settings.set_sampling(recieved_settings.get('sampling', ''))
            settings.set_notify_email(recieved_settings.get('notifyEmail', ''))
            settings.set_heartbeat_interval(recieved_settings.get('heartbeat_interval', ''))
            settings.set_retry_count(recieved_settings.get('retryCount', ''))
            settings.set_retention(recieved_settings.get('retention', ''))
            settings.backup_path=recieved_settings.get('path', '')
            settings.set_user(recieved_settings.get('user', ''))
            settings.set_password(recieved_settings.get('password', ''))

            #create config object
            config = Configuration(0, 0, secret)
            config.address = recieved_settings.get('path', '')

            # set config and settings to job
            job_to_save.set_settings(settings)
            job_to_save.set_config(config)

            # save the job to the database
            job_to_save.save()
            RUN_JOB_OBJECT.set_job(job_to_save)
            return "200 OK"
        elif FlaskServer.auth(secret, logger, id) == 405:
            return "401 Access Denied"
        else:
            return "500 Internal Server Error"
#-------------------------------------- GET ROUTES ------------------------------------------------
    # HELPERS
    def run(self):
        """
        Runs the server
        """
        self.website.run()
    def __init__(self):
        Logger.debug_print("flask server started")
        self.run()
