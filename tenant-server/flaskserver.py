"""
# Program: Tenant-server
# File: flaskserver.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the FlaskServer class. This class is used 
# to host the tenant-client REST API

# Class Types: 
#               1. FlaskServer - API

Error Code:
1000 - Writing to the database failed. Address already exists in the database.
"""
# pylint: disable= import-error, global-statement,unused-argument, line-too-long
import os
import time
import requests
from flask import Flask, request,make_response
from security import Security, CLIENT_SECRET
from logger import Logger
from runjob import RunJob
from job import Job
from jobsettings import JobSettings
from conf import Configuration
from sql import InitSql, MySqlite
from HeartBeat import MY_CLIENTS

from requests import get
import socket
import uuid
import base64
CLIENT_REGISTRATION_PATH = "api/DataLink/Register"
RUN_JOB_OBJECT = None
CLIENTS = ()
KEYS = "LJA;HFLASBFOIASH[jfnW.FJPIH","JBQDPYQ7310712631DHLSAU8AWY]"
MASTER_UNINSTALL_KEY = "LJA;HFLASBFOIASH[jfnW.FJPIH"
URLS_ROUTE="api/DataLink/Urls"
PORT=5002
HOST = '0.0.0.0'

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
    status = MySqlite.read_setting("Status")
    if status == "Online":
        return 1
    elif status == "Offline":
        return 0
    elif status == "ServiceOffline":
        return 2
    else:
        return -1



@staticmethod
def get_time():
    """
    Returns the current time
    """
    return time.strftime("%Y-%m-%dt%H:%M:%S:%m", time.localtime())






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
    def auth(apikey, logger,identification):
        """    
        authenticates requests to the server
        """

        internal_apikey = MySqlite.read_setting("apikey")
        msp_apikey = MySqlite.read_setting("msp_api")
        if apikey == internal_apikey or apikey == msp_apikey:
            return 200
        else:
            logger.log("ERROR", "auth", "Access denied",
            "405", "flaskserver.py")
            return 405

    @staticmethod
    def get_local_files(path:str):
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
        # get the clientSecret from the json body
        code = 0
        msg = ""
        # pylint: enable=unused-variable, enable=invalid-name
        # check if the path exists
        if not os.path.exists(path):
            logger.log("ERROR", "get_local_files", "Path not found",
            "404", "flaskserver.py")
            code=404
            msg="Incorrect path"
        # check if the path is a directory
        elif not os.path.isdir(path):
            logger.log("ERROR", "get_local_files", "Path is not a directory",
            "404", "flaskserver.py")
            code=404
            msg="Path is not a directory"
        # check if the path is accessible
        elif not os.access(path, os.R_OK):
            logger.log("ERROR", "get_local_files", "Path is not accessible",
            "404", "flaskserver.py")
            code=404
            msg="Path is not accessible"
        # check if the path is empty
        elif not os.listdir(path):
            logger.log("ERROR", "get_local_files", "Path is empty",
            "404", "flaskserver.py")
            code=404
            msg="Path is empty"
        # get the files and directories in the path
        # if error is returned, do not get file directories
        elif str(path).lower() == str("c:"):
            logger.log("ERROR", "get_local_files", "Path is not accessible",
            "401", "flaskserver.py")
            code=401
            msg="Path is not accessible Access Denied"
        if code==0:
            try:
                files = os.listdir(path)
                msg = files
            except PermissionError:
                logger.log("ERROR", "get_files", "Permission error",
                "1005", "flaskserver.py")
                code = 401
                msg = "Access Denied"
            except:
                logger.log("ERROR", "get_files", "General Error getting files",
                "1002", "flaskserver.py")
                code= 500
                msg = "Internal server error"
        else:
            return make_response(msg, code)
        # <-- Turn into a method

        return files

    # GET ROUTES
    @website.route('/get_files', methods=['GET'], )
    @staticmethod
    def get_files():
        """
        Get list of files from a provided path
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get('apikey')
        # get the ID from the json body
        client_id = data.get('client_id', '')
        code = 0
        msg = ""
        global CLIENTS 
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(recieved_client_secret, logger, client_id) == 405:
            code = 401
            msg = "Access Denied"
        else:
            if client_id == 0:
                return FlaskServer.get_local_files(data.get('path', ''))
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(client_id): # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[2]}:{i[3]}/get_files"
                    try:
                        response= requests.get(url,headers={"apikey":MySqlite.read_setting("apikey")}, json={"path":data.get('path', '')},timeout=10)
                        body = response.json()
                        headers = response.headers
                        return make_response(body, 200)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "get_files.relay.get_files", f"Timeout connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "get_files.relay.get_files", f"Error connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500
            return make_response("Client not found", 403)
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)


    # POST ROUTES

    @website.route('/start_job', methods=['POST'], )
    @staticmethod
    def start_job():
        """
        Triggers the RunJob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()

        # get the ID from the json body
        identification = data.get('client_id', '')
        apikey = request.headers.get('apikey', '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            if identification == 0:
                RUN_JOB_OBJECT.trigger_job()
                return "200 OK"
            
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(identification): # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[2]}:{i[3]}/start_job"
                    try:
                        return requests.put(url,data={},headers={"apikey":MySqlite.read_setting("apikey")},timeout=10)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "start_job.relay.start_job", f"Timeout connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "start_job.relay.start_job", f"Error connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500

            return make_response("Client not found", 403)
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @website.route('/stop_job', methods=['POST'], )
    @staticmethod
    def stop_job():
        """
        Triggers the stop_job with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        apikey = request.headers.get('apikey', '')
        # get the ID from the json body
        identification = data.get('client_id', '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            if identification == 0:
                RUN_JOB_OBJECT.stop_job()
                return "200 OK"
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(identification):# find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[2]}:{i[3]}/stop_job"
                    try:
                        response= requests.put(url,data={},headers={"apikey":MySqlite.read_setting("apikey")},timeout=10)
                        return make_response("",response.status_code)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "stop_job.relay.stop_job", f"Timeout connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "stop_job.relay.stop_job", f"Error connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500
            return make_response("Client not found", 403)
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @website.route('/kill_job', methods=['POST'], )
    @staticmethod
    def kill_job():
        """
        Triggers the killjob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        apikey = request.headers.get('apikey', '')
        # get the ID from the json body
        identification = data.get('client_id', '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            if identification == 0:
                RUN_JOB_OBJECT.kill_job()
                return "200 OK"
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(identification): # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[2]}:{i[3]}/kill_job"
                    try:
                        response= requests.put(url,headers={"apikey":MySqlite.read_setting("apikey")},timeout=10)
                        return make_response("",response.status_code)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "kill_job.relay.kill_job", f"Timeout connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "kill_job.relay.kill_job", f"Error connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500
            return make_response("Client not found", 403)

        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)


    @website.route('/enable_job', methods=['POST'], )
    @staticmethod
    def enable_job():
        """
        Triggers the RunJob with the job assigned to this computer
        """
        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        apikey = request.headers.get('apikey', '')
        # get the ID from the json body
        identification = data.get('client_id', '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            if identification == 0:
                RUN_JOB_OBJECT.enable_job()
                return "200 OK"
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(identification): # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[2]}:{i[3]}/enable_job"
                    try:
                        response= requests.put(url, headers={"apikey":MySqlite.read_setting("apikey")},timeout=10)
                        return make_response("",response.status_code)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "enable_job.relay.enable_job", f"Timeout connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "enable_job.relay.enable_job", f"Error connecting to {i[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500
            return make_response("Client not found", 403)
        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)

    @website.route('/modify_job', methods=['POST'], )
    @staticmethod
    def modify_job():
        """
        Sets the current job to the new job. Or creates on if it does not exist
        """
        global RUN_JOB_OBJECT
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        logger=Logger()
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')

        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == str(0):
                recieved_job = data.get("0", '')
                # recieve settings as json
                recieved_settings = recieved_job.get('settings', '')

                job_to_save = Job()
                job_to_save.set_id(0)
                job_to_save.set_title(recieved_job.get('title', ''))
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
                config = Configuration(0, 0, apikey)
                config.address = recieved_settings.get('path', '')
                settings.set_username(recieved_settings.get('user', ''))
                settings.set_password(recieved_settings.get('password', ''))

                job_to_save.set_settings(settings)
                job_to_save.set_config(config)
                job_to_save.save()
                RUN_JOB_OBJECT = job_to_save
                return "200 OK"
            else:
                for i in CLIENTS:

                    if str(i[0]) == str(identification):
                        url = f"http://{i[2]}:{i[3]}/modify_job"
                        try:
                            content = request.get_json()
                            response=requests.request("POST", url, json=content, headers={"Content-Type":"application/json","apikey":MySqlite.read_setting("apikey")},timeout=10)
                            return make_response("", response.status_code)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "modify_job.relay.modify_job", f"Timeout connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Timeout connecting to {i[0]}"
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "modify_job.relay.modify_job", f"Error connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Error connecting to {i[0]}"
                        except:
                            msg = "Internal server error"
                            code = 500
                return make_response("Client not found", 403)
        elif FlaskServer.auth(apikey, logger, id) == 405:
            return "401 Access Denied"
        else:
            return "500 Internal Server Error"
        
    @staticmethod
    @website.route('/nexum', methods=['GET'], )
    def nexum():
        """
        Returns the nexum.exe from the MSP --INTERNAL--
        """
        logger=Logger()
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        if FlaskServer.auth(apikey, logger, identification) == 200:
            try:
                request = requests.request("GET", f"{"http://"}{MySqlite.read_setting("server_address")}:{MySqlite.read_setting("server_port")}/{"urls"}",
                        timeout=10, headers={"Content-Type": "application/json","apikey":MySqlite.read_setting("apikey")},
                        verify=False)

                request = request.json()

                server_url=request["nexumUrl"]

                request = requests.request("GET", f"{server_url}",
                        timeout=10, headers={"Content-Type": "application/json","apikey":MySqlite.read_setting("apikey")},
                        verify=False)
                return request


            except Exception as e:
                MySqlite.write_log("ERROR","nexum", "Failed to get URLS " + str(e),"1009","flaskserver.py")


    @staticmethod
    @website.route('/nexumservice', methods=['GET'], )
    def nexumservice():
        """
        Returns the nexum.exe from the MSP --INTERNAL--
        """
        logger=Logger()
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        if FlaskServer.auth(apikey, logger, identification) == 200:
            try:
                request = requests.request("GET", f"{"http://"}{MySqlite.read_setting("server_address")}:{MySqlite.read_setting("server_port")}/{"urls"}",
                        timeout=10, headers={"Content-Type": "application/json","apikey":MySqlite.read_setting("apikey")},
                        verify=False)

                request = request.json()

                service_url=request["nexumServiceUrl"]

                request = requests.request("GET", f"{service_url}",
                        timeout=10, headers={"Content-Type": "application/json","apikey":MySqlite.read_setting("apikey")},
                        verify=False)
                return request


            except Exception as e:
                MySqlite.write_log("ERROR","nexumservice", "Failed to get URLS " + str(e),"1009","flaskserver.py")

    @website.route('/get_job', methods=['GET'], )
    @staticmethod
    def get_job():
        """
        Gives Current Job Information
        """
        data = request.get_json()
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == str(0):
                j:Job = Job()
                j.load(0)
                data = {
                    "0":{
                    "title": j.get_title(),
                    "settings": {
                        "schedule": j.get_settings()[1],
                        "startTime": j.get_settings()[2],
                        "stopTime": j.get_settings()[3],
                        "retryCount": j.get_settings()[4],
                        "sampling": j.get_settings()[5],
                        "notifyEmail": j.get_settings()[8],
                        "heartbeat_interval": j.get_settings()[9],
                        "retention": j.get_settings()[6],
                        "path": j.get_config()[2],
                        "user": j.get_settings()[11],
                        "password": j.get_settings()[12]
                    }
                    }
                }
                return make_response(data,200)
            for client in CLIENTS:
                if client[0] == identification:
                    url = f"http://{client[2]}:{client[3]}/get_job"
                    try:
                        return requests.get(url, headers={"apikey":apikey,"Content-Type": "application/json"},json={},timeout=10,verify=False)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "get_job.relay.get_job", f"Timeout connecting to {client[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Timeout connecting to {client[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "get_job.relay.get_job", f"Error connecting to {client[0]}",
                        "500", "flaskserver.py")
                        code=402
                        msg=f"Error connecting to {client[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500
        return make_response("Client not found", 403)
    
    @website.route('/force_checkin', methods=['POST'], )
    @staticmethod
    def force_checkin():
        """
        Forces a heartbeat
        """
        data = request.get_json()
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            pass
        return "200 OK"
    

    @website.route('/restore', methods=['POST'], )
    @staticmethod
    def restore():
        """
        Restores files or directories
        """
        data = request.get_json()
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        path = data.get('path', '')
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            pass
        return "200 OK"


    @website.route('/get_status', methods=['GET'], )
    @staticmethod
    def get_status():
        """
        Gets the current status of running jobs or error state, version information etc
        """
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        logger=Logger()
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == str(0):
                content = {
                    "status": convert_device_status(),
                }
                return make_response(content, 200)
            else:
                for i in CLIENTS:
                    if str(i[0]) == str(identification):
                        url = f"http://{i[2]}:{i[3]}/get_Status"
                        try:
                            data = requests.get(url, headers={"apikey":apikey,"Content-Type": "application/json"},json={},timeout=10,verify=False)
                            # data.headers["status"]
                            status = data.headers.get("status")
                            return status
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "get_status.relay.get_status", f"Timeout connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Timeout connecting to {i[0]}"
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "get_status.relay.get_status", f"Error connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Error connecting to {i[0]}"
                        except:
                            msg = "Internal server error"
                            code = 500
                return make_response("Client not found", 403)
        else:
            return make_response("401 Access Denied", 401)

    @website.route('/get_job_status', methods=['POST'], )
    @staticmethod
    def get_job_status():
        """
        Gets the current status of running job
        """

        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get('apikey', '')
        # get the ID from the json body
        recieved_client_id = data.get('client_id', '')


        authcode= FlaskServer.auth(recieved_client_secret, logger, MySqlite.read_setting("CLIENT_ID"))
        if authcode == 405:
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            if recieved_client_id == str(0):
                content = {
                    "status": convert_job_status(),
                }
                return make_response (content,200)
            else:
                global CLIENTS
                CLIENTS = MySqlite.load_clients()
                for i in CLIENTS:
                    if i[0] == recieved_client_id:
                        url = f"http://{i[2]}:{i[3]}/get_job_status"
                        try:
                            return requests.post(url, headers={"apikey":MySqlite.read_setting("apikey"),"Content-Type":"application/json"},json={},timeout=10)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "get_job_status.relay.get_job_status", f"Timeout connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Timeout connecting to {i[0]}"
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "get_job_status.relay.get_job_status", f"Error connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Error connecting to {i[0]}"
                        except:
                            msg = "Internal server error"
                            code = 500
                return make_response("Client not found", 403)
            # return status
            return make_response (content,200)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/force_update', methods=['POST'], )
    @staticmethod
    def force_update():
        """
        Forces the client to pull an update from the server
        """
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        logger=Logger()
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == 0:
                pass # update server
            else:   
                for i in CLIENTS:
                    if i[1] == identification:
                        url = f"http://{i[2]}:{i[3]}/force_update"
                        try:
                            return requests.post(url, json={"apikey":MySqlite.read_setting("apikey"), "ID": identification},timeout=10)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "force_update.relay.force_update", f"Timeout connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Timeout connecting to {i[0]}"
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "force_update.relay.force_update", f"Error connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Error connecting to {i[0]}"
                        except:
                            msg = "Internal server error"
                            code = 500
                return make_response("Client not found", 403)
        return "200 OK"


    @website.route('/get_version', methods=['GET'], )
    @staticmethod
    def get_version():
        """
        Gets version information from the client
        """
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == str(0):
                content= {
                    "version": MySqlite.read_setting("version"),
                    "tag": MySqlite.read_setting("versiontag")
                }
                return make_response(content,200)
            else:
                for i in CLIENTS:
                    if i[1] == identification:
                        url = f"http://{i[2]}:{i[3]}/get_version"
                        try:
                            return requests.post(url, json={"apikey":MySqlite.read_setting("apikey"), "ID": identification},timeout=10)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "get_version.relay.get_version", f"Timeout connecting to {i[0]}",
                            "500","flaskserver.py")
                            code=402
                            msg=f"Timeout connecting to {i[0]}"
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "get_version.relay.get_version", f"Error connecting to {i[0]}",
                            "500", "flaskserver.py")
                            code=402
                            msg=f"Error connecting to {i[0]}"
                        except:
                            msg = "Internal server error"
                            code = 500
                return make_response("Client not found", 403)
        return make_response("401 Access Denied", 401)


    @website.route('/get_jobs', methods=['GET'], )
    @staticmethod
    def get_jobs():
        """
        Gets jobs information from the client
        """
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = data.get('client_id', '')
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            jobs = []
            j:Job = Job()
            j.load(0)
            data = {
                    "0":{
                    "title": j.get_title(),
                    "client_id": "0",
                    "settings": {
                        "schedule": j.get_settings()[1],
                        "startTime": j.get_settings()[2],
                        "stopTime": j.get_settings()[3],
                        "retryCount": j.get_settings()[4],
                        "sampling": j.get_settings()[5],
                        "notifyEmail": j.get_settings()[8],
                        "heartbeat_interval": j.get_settings()[9],
                        "retention": j.get_settings()[6],
                        "path": j.get_config()[2],
                        "user": j.get_settings()[11],
                        "password": j.get_settings()[12]
                    }
                }
            }
            jobs.append(data)
            for client in CLIENTS:
                url = f"http://{client[2]}:{client[3]}/get_job"
                try:
                    response= requests.get(url, headers={"apikey":apikey,"Content-Type": "application/json"},json={},timeout=10,verify=False)
                    jobs.append(response.json())
                except requests.exceptions.ConnectTimeout :
                    logger.log("ERROR", "getjobs.relay.getjob", f"Timeout connecting to {client[0]}",
                    "500", "flaskserver.py")
                    code=402
                    msg=f"Timeout connecting to {client[0]}"
                except requests.exceptions.ConnectionError:
                    logger.log("ERROR", "get_jobs.relay.get_job", f"Error connecting to {client[0]}",
                    "500", "flaskserver.py")
                    code=402
                    msg=f"Error connecting to {client[0]}"
                except:
                    msg = "Internal server error"
                    code = 500
            return make_response(jobs,200)
        else:
            return make_response("401 Access Denied", 401)

    @website.route('/beat', methods=['POST'], )
    @staticmethod
    def beat():
        """
        A spot for clients to send heartbeats to --INTERNAL--
        """
        secret = request.headers.get('secret')
        print(secret)
        identification = request.headers.get('id')
        client_list = MySqlite.load_clients()
        for client in client_list:
            if client[0] == identification:
                if(MySqlite.read_setting("apikey") == secret):
                    print("-----------------")
                    print(secret)
                    print(MySqlite.read_setting("apikey"))
                    MySqlite.update_heartbeat_time(identification)
                    return "200 OK"
                else:
                    return make_response("401 Access Denied",401)
        return make_response("403 Client not found",403)

    @website.route('/urls', methods=['GET'], )
    @staticmethod
    def urls():
        """
        Returns a list of all the urls --INTERNAL--
        """
        secret = request.headers.get('apikey')
        if(MySqlite.read_setting("apikey") == secret):
            req = requests.request("GET", f"{"https://"}{MySqlite.read_setting("server_address")}:{MySqlite.read_setting("msp-port")}/{URLS_ROUTE}",
                        timeout=30, headers={"Content-Type": "application/json","apikey":secret}, verify=False)
            Logger.log("INFO","urls","flask",f"Request to MSP: {req.status_code}",200, "flaskserver.py")
            Logger.log("INFO","urls","flask",f"Request to MSP: {req.content}",200, "flaskserver.py")

            return make_response(req.content, str(req.status_code))


    
    @website.route('/make_smb', methods=['POST'], )
    @staticmethod
    def make_smb():
        """
        Makes a smb share
        """
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get('apikey')
        name = data.get('name', '')
        path = data.get('path', '')
        password = data.get('password', '')
        username = data.get('username', '')
        identification = data.get('client_id', '')
        path = os.path.normpath(path)
        logger=Logger()
        auth = FlaskServer.auth(apikey, logger,  MySqlite.read_setting("client_id"))
        if auth == 200:
            if identification is not None:
                identification=MySqlite.write_backup_server(name, path, username, password)
                content = {
                    "id":identification
                }
                return make_response(content, 200)
            else:
                MySqlite.edit_backup_server(int(identification), name, path, username, password)
                return make_response("200 OK", 200)
        elif auth == 405:
            return make_response("401 Access Denied", 401)
        else:
            return make_response("500 Internal Server Error", 500)
        
    @website.route('/get_smb', methods=['GET'], )
    @staticmethod
    def get_smb():
        """
        Gets smb share information
        """

        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = request.get_json().get('id', '')
        logger=Logger()
        auth=FlaskServer.auth(apikey, logger,  MySqlite.read_setting("client_id"))
        if auth == 200:
            server= MySqlite.get_backup_server(int(identification))
            # format smb info as json 
            data = {
                "Name":server[4],
                "Path":server[1],
                "Username":server[2],
                "Password":server[3]
            }
            return make_response(data, 200)
        elif auth == 405:
            return make_response("401 Access Denied", 401)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/delete_smb', methods=['DELETE'], )
    @staticmethod
    def delete_smb():
        """
        Deletes a smb share
        """
        # get client secret from header
        apikey = request.headers.get('apikey')
        identification = request.get_json().get('id', '')
        logger=Logger()
        auth = FlaskServer.auth(apikey, logger, MySqlite.read_setting("id"))
        if auth == 200:
            MySqlite.delete_backup_server(int(identification))
            return make_response("200 OK", 200)
        elif auth == 405:
            return make_response("401 Access Denied", 401)
        else:
            return make_response("500 Internal Server Error", 500)
    # PUT ROUTES
    @website.route('/check-installer', methods=['GET'], )
    @staticmethod
    def check_installer():
        """
        Gets version information from the client and requests an install with the MSP --INTERNAL--
        """
        secret = request.headers.get('apikey')
        key = request.get_json().get('installationKey', '')
        logger = Logger()
            


        if(MySqlite.read_setting("apikey") == secret):
            print("MATCH")
            # has valid api key
            
            InitSql.clients()
            # pull all info from the body
            body = request.get_json()
            identification = MySqlite.get_next_client_id()
            name = body.get('name', '')
            ip = body.get('ipaddress', '')
            port = body.get('port', '')
            status = 'Installing'
            mac = body["macaddresses"][0]["address"]
            uid = body.get('uuid', '')
            type = body.get('type', '')
            # ensure no duplicate client ip's
            clients = MySqlite.load_clients()
            for client in clients:
                if client[2] == ip:
                    return make_response("403 - PC Already connected", 403)
            # reformat and send to msp

            payload = {
            "name":socket.gethostname(),
            "uuid":uid,
            "client_Id":identification,
            "ipaddress":socket.gethostbyname(socket.gethostname()),
            "port":port,
            "type":type,
            "macaddresses":[
                {
                "id":0,
                "address":':'.join(['{:02x}'.format((uuid.getnode() >> ele) & 0xff)
                            for ele in range(0,8*6,8)][::-1])
                }
            ],
            "installationKey":key
        }
            logger.log("INFO", "check-installer", f"Request to MSP: {payload}", 200, "flaskserver.py")
            logger.log("INFO", "check-installer", f"path :{"https://"} {MySqlite.read_setting("server_address")}:{MySqlite.read_setting("msp-port")}/{CLIENT_REGISTRATION_PATH}", 200, "flaskserver.py")
            req = requests.request("POST", f"{"https://"}{MySqlite.read_setting("server_address")}:{MySqlite.read_setting("msp-port")}/{CLIENT_REGISTRATION_PATH}",
                        timeout=30, headers={"Content-Type": "application/json","apikey":secret}, json=payload, verify=False)
            logger.log("INFO", "check-installer", f"Request to MSP: {req.status_code}", 200, "flaskserver.py")
            # if msp returns 200 ok, then return 200 ok
            if req.status_code == 200:
                result = MySqlite.write_client(identification, name, ip, port, status, mac)
                if result == 200:
                    # verify for the MSP

                    return make_response("200 ok", 200, {"clientid": identification,"msp_api":MySqlite.read_setting("msp_api")})
                else:
                    return make_response("500 Internal Server Error - CODE: 1000", 403)
            else:
                return make_response(f"{req.status_code} - {req.text}", req.status_code)
        return make_response("401 Access Denied", 401)


    @website.route('/uninstall', methods=['GET'], )
    @staticmethod
    def uninstall():
        """ 
        Client posts to this route to uninstall the client
        the client is then removed from the database and the heartbeat database
        the client is sent a 200 ok response and the msp is notified --INTERNAL-- AND --EXTERNAL--
        """
        # change this to check with msp for uninstall ###########################################
        secret = request.headers.get('clientSecret')
        key = request.headers.get('key')
        logger = Logger()
        if FlaskServer.auth(secret, logger, 0) == 200:
            if key == MySqlite.read_setting("Master-Uninstall"):
                body = request.get_json()
                identification = body.get('clientid', '')
                if MySqlite.get_last_checkin(identification) == None:
                    return make_response("403 Rejected - Client does not exist", 403)
                MySqlite.delete_client(identification)
                return make_response("200 ok", 200)
            else:
                logger.log("ERROR", "uninstall", "Key does not match",1201, "flaskserver.py")
                return make_response("403 Rejected", 403)
        else:
            logger.log("ERROR", "uninstall", "Access Denied",405,"flaskserver.py")
            return make_response("401 Access Denied", 401)
    # HELPERS
    def run(self):
        """
        Runs the server
        """
        global RUN_JOB_OBJECT
        RUN_JOB_OBJECT = RunJob()
        self.website.run(port=5002, host='0.0.0.0')
    def __init__(self):
        # load all clients from DB
        global MY_CLIENTS
        MY_CLIENTS = MySqlite.load_clients()
        Logger.debug_print("flask server started")
        self.run()
