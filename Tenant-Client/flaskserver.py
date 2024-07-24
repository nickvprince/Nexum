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
from flask import Flask, request,make_response
from logger import Logger
from job import Job
from jobsettings import JobSettings
from conf import Configuration
from MySqlite import MySqlite
from api import API
RUN_JOB_OBJECT = None




# SETTINGS
API_KEY_SETTING = "apikey"
MSP_API_SETTING = "msp_api"
JOB_STATUS_SETTING = "job_status"
STATUS_SETTING = "Status"
CLIENT_ID_SETTING = "CLIENT_ID"

# NETWORK
HOST = "0.0.0.0"
PORT = 5001

# GENERAL
RX_API_KEY = "apikey"
FILE_NAME = "flaskserver.py"
RX_CLIENT_ID = "client_id"
RX_PATH = "path"




@staticmethod
def convert_job_status():
    """
    Converts the status to a enum
    """
    job_status = MySqlite.read_setting("job_status")
    if job_status == "InProgress":
        return 1
    elif job_status == "NotStarted":
        return 0
    elif job_status == "Failed":
        return 3
    elif job_status == "Completed":
        return 2
    else:
        return -1

@staticmethod
def convert_device_status():
    """
    Converts the status to a enum
    """
    status = MySqlite.read_setting(STATUS_SETTING)
    if status == "Online":
        return 1
    elif status == "Offline":
        return 0
    elif status == "ServiceOffline":
        return 2
    else:
        return -1

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
        """     
        Authenticates server requests
        """

        apikey = MySqlite.read_setting(API_KEY_SETTING)
        msp_api = MySqlite.read_setting("msp_api")
        if str(apikey) == str(recieved_client_secret) or str(recieved_client_secret) == str(msp_api):
            return 200
        else:
            logger.log("ERROR", "get_files", "Access denied",
            "405", FILE_NAME)
            return 405
        return 500




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
        path = data.get(RX_PATH, '')
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')
        # get the ID from the json body
        identification = data.get(RX_CLIENT_ID, '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        # pylint: enable=unused-variable, enable=invalid-name
        # check if the path exists
        elif not os.path.exists(path):
            logger.log("ERROR", "get_files", "Path not found",
            "404", FILE_NAME)
            code=404
            msg="Incorrect path"
        # check if the path is a directory
        elif not os.path.isdir(path):
            logger.log("ERROR", "get_files", "Path is not a directory",
            "404", FILE_NAME)
            code=404
            msg="Path is not a directory"
        # check if the path is accessible
        elif not os.access(path, os.R_OK):
            logger.log("ERROR", "get_files", "Path is not accessible",
            "404", FILE_NAME)
            code=404
            msg="Path is not accessible"
        # check if the path is empty
        elif not os.listdir(path):
            logger.log("ERROR", "get_files", "Path is empty",
            "404", FILE_NAME)
            code=404
            msg="Path is empty"
        # get the files and directories in the path
        # if error is returned, do not get file directories
        if code==0:
            try:
                files = os.listdir(path)
            except PermissionError:
                logger.log("ERROR", "get_files", "Permission error",
                "1005", FILE_NAME)
                code = 401
                msg = "Access Denied"
            except:
                logger.log("ERROR", "get_files", "General Error getting files",
                "1002",FILE_NAME)
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
        logger=Logger()
        # get the json body
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        if FlaskServer.auth(recieved_client_secret, logger,
                        MySqlite.read_setting(CLIENT_ID_SETTING)) == 405:
            return make_response("401 Access Denied", 401)
        elif FlaskServer.auth(recieved_client_secret, logger,
                        MySqlite.read_setting(CLIENT_ID_SETTING)) == 200:
            # get job info and send back
            return make_response("200 OK", 200)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/force_checkin', methods=['GET'], )
    @staticmethod
    def force_checkin():
        """
        Forces a heartbeat
        """
        logger=Logger()
        # get the json body
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')
        authcode= FlaskServer.auth(recieved_client_secret, logger,
                                MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            # force a heartbeat
            return make_response ("200 OK", 200)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/get_Status', methods=['POST'], )
    @staticmethod
    def get_status():
        """
        Gets the current status of running jobs or error state, version information etc
        """
        logger=Logger()
        # get the json body
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        authcode= FlaskServer.auth(recieved_client_secret,
                                logger, MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            content = {
                "status": convert_job_status(),
            }
            # return status
            return make_response (content,200)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/get_job_status', methods=['POST'], )
    @staticmethod
    def get_job_status():
        """
        Gets the current status of running job
        """
        logger=Logger()
        # get the json body
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        authcode= FlaskServer.auth(recieved_client_secret, logger,
                                MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            content = {
                "status": convert_job_status(),
            }
            # return status
            return make_response (content,200)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/get_version', methods=['GET'], )
    @staticmethod
    def get_version():
        """
        Gets version information from the client
        """
        logger=Logger()
        # get the json body
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        authcode= FlaskServer.auth(recieved_client_secret, logger,
                                MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            version = API.get_version()
            # return status
            return make_response ("200 OK", 200,{"version":version})
        else:
            return make_response("500 Internal Server Error", 500)

# -------------------------------------- GET ROUTES ------------------------------------------------

# -------------------------------------- PUT ROUTES ------------------------------------------------
    @website.route('/start_job', methods=['PUT'], )
    @staticmethod
    def start_job():
        """
        Triggers the RunJob with the job assigned to this computer
        """
        logger=Logger()

        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger,
                        MySqlite.read_setting(CLIENT_ID_SETTING)) == 405:
            code = 401
            msg = "Access Denied"
        RUN_JOB_OBJECT.trigger_job()
        if code==0:
            return make_response("200 OK", 200)
        else:
            return make_response(msg, code)

    @website.route('/stop_job', methods=['PUT'], )
    @staticmethod
    def stop_job():
        """
        Triggers the stopjob with the job assigned to this computer
        """
        logger=Logger()
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')
        # get the ID from the json body
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger,
                        MySqlite.read_setting(CLIENT_ID_SETTING)) == 405:
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
        logger=Logger()
        # get the json body

        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        authcode= FlaskServer.auth(recieved_client_secret, logger,
                                MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            #update client
            return make_response ("200 OK", 200)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/enable_job', methods=['PUT'], )
    @staticmethod
    def enable_job():
        """
        Triggers the RunJob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body

        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        authcode= FlaskServer.auth(recieved_client_secret, logger,
                                MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            RUN_JOB_OBJECT.enable_job()
            return make_response ("200 OK", 200)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/kill_job', methods=['PUT'], )
    @staticmethod
    def kill_job():
        """
        Triggers the killjob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body

        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        authcode= FlaskServer.auth(recieved_client_secret, logger,
                                MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            RUN_JOB_OBJECT.kill_job()
            return make_response ("200 OK", 200)
        else:
            return make_response("500 Internal Server Error", 500)


# -------------------------------------- PUT ROUTES ------------------------------------------------
# -------------------------------------- POST ROUTES -----------------------------------------------
    @website.route('/restore', methods=['POST'], )
    @staticmethod
    def restore():
        """
        Restores files or directories
        """
        logger=Logger()
        # get the json body

        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(RX_API_KEY, '')

        authcode= FlaskServer.auth(recieved_client_secret, logger,
                                MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            return make_response ("200 OK", 200)
        else:
            return make_response("500 Internal Server Error", 500)

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
        secret = request.headers.get(RX_API_KEY)


        if FlaskServer.auth(secret, logger, MySqlite.read_setting(CLIENT_ID_SETTING)) == 200:
            recieved_job = data.get("0", '')
            # recieve settings as json
            recieved_settings = recieved_job.get('settings', '')


            job_to_save = Job()
            job_to_save.set_id(MySqlite.read_setting(CLIENT_ID_SETTING))
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
            try:
                job_to_save.save()
                RUN_JOB_OBJECT.set_job(job_to_save)
                return "200 OK"
            except Exception:
                return make_response("Job exists",200)
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
        self.website.run(port=PORT,host=HOST)
