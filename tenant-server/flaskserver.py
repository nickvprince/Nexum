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
# pylint: disable= import-error, global-statement,unused-argument, line-too-long, broad-except

import socket
import json
import uuid
import os
import requests
from flask import Flask, request,make_response
from logger import Logger
from runjob import RunJob
from job import Job
from jobsettings import JobSettings
from conf import Configuration
from sql import InitSql, MySqlite
from api import API


# msp routes
CLIENT_REGISTRATION_PATH = "api/DataLink/Register"
URLS_ROUTE="api/DataLink/Urls"
UNINSTALL_ROUTE = "/api/DataLink/Uninstall"
UPDATE_DEVICE_STATUS_ROUTE = "/api/DataLink/Update-Device-Status"

# network settings
MSP_PROTOCOL = "https://"
TENANT_PROTOCOL = "http://"
PORT=5002
HOST = '0.0.0.0'

# general settings
TIMEOUT = 5
VERIFY = False
FILENAME = "Flaskserver.py"

# request tags such as json tag ie {CLIENT_ID:0} tag would be CLIENT_ID
RX_CLIENT_ID = "client_id"
RX_UUID = "uuid"

# setting names
MSP_SERVER_SETTING = "msp_server_address"
APIKEY = "apikey"
MSP_API_SETTING = "msp_api"
MSP_PORT_SETTING = "msp-port"
CLIENT_ID_SETTING = "CLIENT_ID"
VERSION_SETTING = "version"
VERSION_TAG_SETTING = "versiontag"


RUN_JOB_OBJECT = None
CLIENTS = ()

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

        internal_apikey = MySqlite.read_setting(APIKEY)
        msp_apikey = MySqlite.read_setting(MSP_API_SETTING)
        if apikey == internal_apikey or apikey == msp_apikey:
            return 200
        else:
            logger.log("ERROR", "auth", "Access denied",
            "405", FILENAME)
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
            "404", FILENAME)
            code=404
            msg="Incorrect path"
        # check if the path is a directory
        elif not os.path.isdir(path):
            logger.log("ERROR", "get_local_files", "Path is not a directory",
            "404", FILENAME)
            code=404
            msg="Path is not a directory"
        # check if the path is accessible
        elif not os.access(path, os.R_OK):
            logger.log("ERROR", "get_local_files", "Path is not accessible",
            "404", FILENAME)
            code=404
            msg="Path is not accessible"
        # check if the path is empty
        elif not os.listdir(path):
            logger.log("ERROR", "get_local_files", "Path is empty",
            "404", FILENAME)
            code=404
            msg="Path is empty"
        # get the files and directories in the path
        # if error is returned, do not get file directories
        elif str(path).lower() == str("c:"):
            logger.log("ERROR", "get_local_files", "Path is not accessible",
            "401", FILENAME)
            code=401
            msg="Path is not accessible Access Denied"
        if code==0:
            try:
                files = os.listdir(path)
                msg = files
            except PermissionError:
                logger.log("ERROR", "get_files", "Permission error",
                "1005", FILENAME)
                code = 401
                msg = "Access Denied"
            except:
                logger.log("ERROR", "get_files", "General Error getting files",
                "1002", FILENAME)
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
        recieved_client_secret = request.headers.get(APIKEY)
        # get the ID from the json body
        client_id = data.get(RX_CLIENT_ID, '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(recieved_client_secret, logger, client_id) == 405:
            logger.log("ERROR", "get_files", "Access Denied",401,FILENAME)
            code = 401
            msg = "Access Denied"
        else:
            if str(client_id) == str(0):
                logger.log("INFO", "get_files", "Getting local files",0,FILENAME)
                return FlaskServer.get_local_files(data.get('path', ''))
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(client_id): # find the client address to match the ID passed where 0 is localhost
                    url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/get_files"
                    try:
                        logger.log("INFO", "get_files", f"Getting files from {url}",0,FILENAME)
                        response= requests.get(url,headers={APIKEY:MySqlite.read_setting(APIKEY)}, json={"path":data.get('path', '')},timeout=TIMEOUT)
                        body = response.json()
                        return make_response(body, 200)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "get_files.relay.get_files", f"Timeout connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "get_files.relay.get_files", f"Error connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        logger.log("ERROR", "get_files.relay.get_files", "Internal server error",500,FILENAME)
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
        identification = data.get(RX_CLIENT_ID, '')
        apikey = request.headers.get(APIKEY, '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 405:
            logger.log("ERROR", "start_job", "Access Denied",401,FILENAME)
            code = 401
            msg = "Access Denied"
        else:
            if str(identification) == str(0):
                logger.log("INFO", "start_job", "Starting job",0,FILENAME)
                RUN_JOB_OBJECT.trigger_job()
                return make_response("200 OK", 200)
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(identification): # find the client address to match the ID passed where 0 is localhost
                    url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/start_job"
                    try:
                        logger.log("INFO", "start_job", f"Starting job on {i[0]}",0,FILENAME)
                        response = requests.put(url,data={},headers={APIKEY:MySqlite.read_setting(APIKEY)},timeout=TIMEOUT)
                        return make_response(response.content,response.status_code)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "start_job.relay.start_job", f"Timeout connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "start_job.relay.start_job", f"Error connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except Exception as e:
                        logger.log("ERROR", "start_job.relay.start_job", "Internal server error"+ str(e),500,FILENAME)
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
        apikey = request.headers.get(APIKEY, '')
        # get the ID from the json body
        identification = data.get(RX_CLIENT_ID, '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 405:
            logger.log("ERROR", "stop_job", "Access Denied",401,FILENAME)
            code = 401
            msg = "Access Denied"
        else:
            if str(identification) == str(0):
                logger.log("INFO", "stop_job", "Stopping job",0,FILENAME)
                RUN_JOB_OBJECT.stop_job()
                return make_response("200 OK", 200)
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(identification):# find the client address to match the ID passed where 0 is localhost
                    url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/stop_job"
                    try:
                        logger.log("INFO", "stop_job", f"Stopping job on {url}",0,FILENAME)
                        response= requests.put(url,data={},headers={APIKEY:MySqlite.read_setting(APIKEY)},timeout=TIMEOUT)
                        return make_response("",response.status_code)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "stop_job.relay.stop_job", f"Timeout connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "stop_job.relay.stop_job", f"Error connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except Exception as e:
                        logger.log("ERROR", "stop_job.relay.stop_job", "Internal server error"+ str(e),500,FILENAME)
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
        apikey = request.headers.get(APIKEY, '')
        # get the ID from the json body
        identification = data.get(RX_CLIENT_ID, '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 405:
            logger.log("ERROR", "kill_job", "Access Denied",401,FILENAME)
            code = 401
            msg = "Access Denied"
        else:
            if str(identification) == str(0):
                logger.log("INFO", "kill_job", "Killing job",0,FILENAME)
                RUN_JOB_OBJECT.kill_job()
                return make_response("200 OK", 200)
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(identification): # find the client address to match the ID passed where 0 is localhost
                    url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/kill_job"
                    try:
                        logger.log("INFO", "kill_job", f"Killing job on {url}",0,FILENAME)
                        response= requests.put(url,headers={APIKEY:MySqlite.read_setting(APIKEY)},timeout=TIMEOUT)
                        return make_response("",response.status_code)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "kill_job.relay.kill_job", f"Timeout connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "kill_job.relay.kill_job", f"Error connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except Exception as e:
                        logger.log("ERROR", "kill_job.relay.kill_job", "Internal server error"+ str(e),500,FILENAME)
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
        apikey = request.headers.get(APIKEY, '')
        # get the ID from the json body
        identification = data.get(RX_CLIENT_ID, '')
        code = 0
        msg = ""
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 405:
            logger.log("ERROR", "enable_job", "Access Denied",401,FILENAME)
            code = 401
            msg = "Access Denied"
        else:
            if str(identification) == str(0):
                logger.log("INFO", "enable_job", "Enabling job",0,FILENAME)
                RUN_JOB_OBJECT.enable_job()
                return make_response("200 OK", 200)
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if str(i[0]) == str(identification): # find the client address to match the ID passed where 0 is localhost
                    url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/enable_job"
                    try:
                        logger.log("INFO", "enable_job", f"Enabling job on {url}",0,FILENAME)
                        response= requests.put(url, headers={APIKEY:MySqlite.read_setting(APIKEY)},timeout=TIMEOUT)
                        return make_response("",response.status_code)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "enable_job.relay.enable_job", f"Timeout connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "enable_job.relay.enable_job", f"Error connecting to {i[0]}",
                        "500", FILENAME)
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except Exception as e:
                        logger.log("ERROR", "enable_job.relay.enable_job", "Internal server error"+ str(e),500,FILENAME)
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
        data = json.dumps(data)
        data = json.loads(data)
        # get client secret from header
        apikey = request.headers.get(APIKEY)
        identification = data.get(RX_CLIENT_ID, '')

        if FlaskServer.auth(apikey, logger, identification) == 200:
            if str(identification) == str(0):
                logger.log("INFO", "modify_job", "Modifying job",0,FILENAME)

                # recieve settings as json

                recieved_settings = data.get('0', '').get('settings', '')
                recieved_settings = json.dumps(recieved_settings)
                recieved_settings = json.loads(recieved_settings)
                job_to_save = Job()
                job_to_save.set_id(0)
                job_to_save.set_title(data.get('0', '').get('title', ''))
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
                

                config = Configuration(0, 0, apikey)
                #update to use the smb share id provided instead of user password path from the request
                server_id = recieved_settings.get('id', '')
                server = MySqlite.get_backup_server(server_id)
                if server is None:
                    settings.backup_path=recieved_settings.get('path', '')
                    config.address = recieved_settings.get('path', '')
                    settings.set_username(recieved_settings.get('user', ''))
                    settings.set_password(recieved_settings.get('password', ''))
                else:
                    settings.backup_path=server[1]
                    config.address = server[1]
                    settings.set_username(server[2])
                    settings.set_password(server[3])

                job_to_save.set_settings(settings)
                job_to_save.set_config(config)
                job_to_save.save()
                logger.log("INFO", "modify_job", "Job Saved",0,FILENAME)
                return make_response("200 OK", 200)
            else:
                for i in CLIENTS:

                    if str(i[0]) == str(identification):
                        url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/modify_job"
                        try:
                            content = request.get_json()
                            server = MySqlite.get_backup_server(content.get('backupServerId', ''))
                            content['backup_path'] = server[1]
                            content['username'] = server[2]
                            content['password'] = server[3]
                            logger.log("INFO", "modify_job", f"Modifying job on {url}",0,FILENAME)
                            response=requests.request("POST", url, json=content, headers={"Content-Type":"application/json",APIKEY:MySqlite.read_setting(APIKEY)},timeout=TIMEOUT)
                            return make_response("", response.status_code)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "modify_job.relay.modify_job", f"Timeout connecting to {i[0]}",
                            "500", FILENAME)
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "modify_job.relay.modify_job", f"Error connecting to {i[0]}",
                            "500", FILENAME)
                        except Exception as e:
                            logger.log("ERROR", "modify_job.relay.modify_job", "Internal server error"+ str(e),500,FILENAME)
                return make_response("Client not found", 403)
        elif FlaskServer.auth(apikey, logger, id) == 405:
            return "401 Access Denied"
        else:
            return "500 Internal Server Error"
    @website.route('/log', methods=['POST'], )
    @staticmethod
    def log():
        """
        Logs a message
        """
        logger=Logger()
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get(APIKEY)

        identification = data.get(RX_CLIENT_ID, '')

        if FlaskServer.auth(apikey, logger, identification) == 200:
            if data.get('function','') == 'status':
                header ={
                "Content-Type":"application/json",
                APIKEY:MySqlite.read_setting(APIKEY)
                }
                content = {
                    RX_CLIENT_ID: int(identification),
                    RX_UUID: str(data.get('uuid','')),
                    "status": int(data.get('code',''))
                }

                try:
                    server_address = MySqlite.read_setting(MSP_SERVER_SETTING)
                    msp_port = MySqlite.read_setting(MSP_PORT_SETTING)
                    try:
                        client = MySqlite.get_client(identification)
                        if str(client[4]) == str(2) and str(client[4] == str(1)) :
                            pass
                        else:
                            client[4] = data.get('code','')
                            MySqlite.update_client(client)
                            response = requests.put(f"{MSP_PROTOCOL}{server_address}:{msp_port}{UPDATE_DEVICE_STATUS_ROUTE}", headers=header, json=content,timeout=TIMEOUT,verify=VERIFY)
                            return make_response(response.content,response.status_code)
                    except: # old code so if new fails the old way still works
                        response = requests.put(f"{MSP_PROTOCOL}{server_address}:{msp_port}{UPDATE_DEVICE_STATUS_ROUTE}", headers=header, json=content,timeout=TIMEOUT,verify=VERIFY)
                        return make_response(response.content,response.status_code)
                except Exception:
                    logger.log("ERROR","log", "Failed to post status","1009",FILENAME)
            else:
                logger.log(data.get('severity', ''), data.get('function', ''), data.get('message', ''), data.get('code', ''), data.get('file', ''), data.get('alert', 'False'))
                return "200 OK"
        return make_response("401 Access Denied", 401)
    @website.route('/nexum', methods=['GET'], )
    @staticmethod
    def nexum():
        """
        Returns the nexum.exe from the MSP --INTERNAL--
        """
        logger=Logger()
        # get client secret from header
        apikey = request.headers.get(APIKEY)

        if FlaskServer.auth(apikey, logger, 0) == 200:
            try:
                logger.log("INFO", "nexum", "Getting URLS",0,FILENAME)
                requestsecond = requests.request("GET", f"{MSP_PROTOCOL}{MySqlite.read_setting(MSP_SERVER_SETTING)}:{MySqlite.read_setting(MSP_PORT_SETTING)}/{URLS_ROUTE}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json",APIKEY:MySqlite.read_setting(APIKEY)},
                        verify=VERIFY)

                requestsecond = requestsecond.json()

                server_url=requestsecond["nexumUrlLocal"]
                requestsecond = requests.request("GET", f"{MSP_PROTOCOL}{server_url}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json",APIKEY:MySqlite.read_setting(APIKEY)},
                        verify=VERIFY)
                logger.log("INFO", "nexum", "Got Nexum File.. Relaying",0,FILENAME)
                return make_response(requestsecond.content,requestsecond.status_code)

            except ConnectionError:
                logger.log("ERROR","nexum", "Failed to get URLS","1009",FILENAME)
                return make_response("Timeout,403")
            except Exception as e:
                logger.log("ERROR","nexum", "Failed to get URLS " + str(e),"1009",FILENAME)
                return make_response("500 Internal Server Error", 500)
            
        else:
            logger.log("ERROR", "nexum", "Access Denied",401,FILENAME)
            return make_response("401 Access Denied", 401)

    @website.route('/nexumservice', methods=['GET'], )
    @staticmethod
    def nexumservice():
        """
        Returns the nexum.exe from the MSP --INTERNAL--
        """

        logger=Logger()
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get(APIKEY)
        identification = data.get(RX_CLIENT_ID, '')
        if FlaskServer.auth(apikey, logger, identification) == 200:
            try:
                logger.log("INFO", "nexumservice", "Getting URLS",0,FILENAME)
                request2 = requests.request("GET", f"{"https://"}{MySqlite.read_setting(MSP_SERVER_SETTING)}:{MySqlite.read_setting(MSP_PORT_SETTING)}/{URLS_ROUTE}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json",APIKEY:MySqlite.read_setting(APIKEY)},
                        verify=VERIFY)

                request2 = request.json()

                service_url=request["nexumServiceUrlLocal"]

                request3 = requests.request("GET", f"{MSP_PROTOCOL}{service_url}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json",APIKEY:MySqlite.read_setting(APIKEY)},
                        verify=VERIFY)
                logger.log("INFO", "nexumservice", "Got Nexum Service File.. Relaying",0,FILENAME)
                return make_response(request3.content,request3.status_code)

            except ConnectionError:
                logger.log("ERROR","nexumservice", "Failed to get URLS","1009",FILENAME)
                return make_response("Timeout,403")
            except Exception as e:
                logger.log("ERROR","nexumservice", "Failed to get URLS " + str(e),"1009",FILENAME)
                make_response("500 Internal Server Error", 500)
        else:
            return make_response("401 Access Denied", 401)
    @website.route('/get_job/<int:identification>', methods=['GET'], )
    @staticmethod
    def get_job(identification):
        """
        Gives Current Job Information
        """
        data = request.get_json()
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        # get client secret from header
        apikey = request.headers.get(APIKEY)
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == 0:
                logger.log("INFO", "get_job", "Getting job",0,FILENAME)
                j:Job = Job()
                j.load(0)
                data = {}
                try:
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
                except Exception as e:
                    pass
                
                return make_response(data,200)
            for client in CLIENTS:
                if client[0] == identification:
                    url = f"{TENANT_PROTOCOL}{client[2]}:{client[3]}/get_job"
                    try:
                        logger.log("INFO", "get_job", f"Getting job from {url}",0,FILENAME)
                        return requests.get(url, headers={APIKEY:apikey,"Content-Type": "application/json"},json={},timeout=TIMEOUT,verify=VERIFY)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "get_job.relay.get_job", f"Timeout connecting to {client[0]}",
                        "500", FILENAME)
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "get_job.relay.get_job", f"Error connecting to {client[0]}",
                        "500", FILENAME)
                    except Exception as e:
                        logger.log("ERROR", "get_job.relay.get_job", "Internal server error"+ str(e),500,FILENAME)
        else:
            return make_response("401 Access Denied", 401)
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
        apikey = request.headers.get(APIKEY)
        identification = data.get(RX_CLIENT_ID, '')
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == str(0):
                return make_response("200 OK", 200)
            else:
                for client in CLIENTS:
                    if client[0] == identification:
                        url = f"{TENANT_PROTOCOL}{client[2]}:{client[3]}/force_checkin"
                        try:
                            logger.log("INFO", "force_checkin", f"Forcing checkin from {url}",0,FILENAME)
                            response=requests.post(url, headers={APIKEY:apikey,"Content-Type": "application/json"},json={},timeout=TIMEOUT,verify=VERIFY)
                            return make_response(response.content,response.status_code)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "force_checkin.relay.force_checkin", f"Timeout connecting to {client[0]}",
                            "500", FILENAME)
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "force_checkin.relay.force_checkin", f"Error connecting to {client[0]}",
                            "500", FILENAME)
                        except Exception as e:
                            logger.log("ERROR", "force_checkin.relay.force_checkin", "Internal server error"+ str(e),500,FILENAME)
                return make_response("Client not found", 403)
        make_response("Internal server error", 500)

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
        apikey = request.headers.get(APIKEY)
        identification = data.get(RX_CLIENT_ID, '')
        path = data.get('path', '')
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            pass
        return "200 OK"


    @website.route('/get_status/<int:identification>', methods=['GET'], )
    @staticmethod
    def get_status(identification):
        """
        Gets the current status of running jobs or error state, version information etc
        """
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get(APIKEY)
        logger=Logger()
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == 0:
                logger.log("INFO", "get_status", "Getting status",0,FILENAME)
                content = {
                    "status": convert_device_status(),
                }

                return make_response(content, 200)
            else:
                for i in CLIENTS:
                    if str(i[0]) == str(identification):
                        url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/get_Status"
                        try:
                            logger.log("INFO", "get_status", f"Getting status from {url}",0,FILENAME)
                            data = requests.get(url, headers={APIKEY:apikey,"Content-Type": "application/json"},json={},timeout=TIMEOUT,verify=VERIFY)
                            # data.headers["status"]
                            status = data.headers.get("status")
                            return status
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "get_status.relay.get_status", f"Timeout connecting to {i[0]}",
                            "500", FILENAME)
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "get_status.relay.get_status", f"Error connecting to {i[0]}",
                            "500", FILENAME)
                        except Exception as e:
                            logger.log("ERROR", "get_status.relay.get_status", "Internal server error"+ str(e),500,FILENAME)
                return make_response("Client not found", 403)
        else:
            logger.log("ERROR", "get_status", "Access Denied",401,FILENAME)
            return make_response("401 Access Denied", 401)

    @website.route('/get_job_status/<int:identification>', methods=['GET'], )
    @staticmethod
    def get_job_status(identification):
        """
        Gets the current status of running job
        """

        logger=Logger()
        # get the clientSecret from the json body
        recieved_client_secret = request.headers.get(APIKEY, '')
        # get the ID from the json body


        authcode= FlaskServer.auth(recieved_client_secret, logger, MySqlite.read_setting(CLIENT_ID_SETTING))
        if authcode == 405:
            logger.log("ERROR", "get_job_status", "Access Denied",401,FILENAME)
            return make_response("401 Access Denied", 401)
        elif authcode == 200:
            if identification == 0:
                logger.log("INFO", "get_job_status", "Getting job status",0,FILENAME)
                content = {
                    "status": convert_job_status(),
                }

                return make_response (content,200)
            else:
                global CLIENTS
                CLIENTS = MySqlite.load_clients()
                for i in CLIENTS:
                    if i[0] == identification:
                        url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/get_job_status"
                        try:
                            logger.log("INFO", "get_job_status", f"Getting job status from {url}",0,FILENAME)
                            return requests.post(url, headers={APIKEY:MySqlite.read_setting(APIKEY),"Content-Type":"application/json"},json={},timeout=TIMEOUT)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "get_job_status.relay.get_job_status", f"Timeout connecting to {i[0]}",
                            "500", FILENAME)
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "get_job_status.relay.get_job_status", f"Error connecting to {i[0]}",
                            "500", FILENAME)
                        except Exception as e:
                            logger.log("ERROR", "get_job_status.relay.get_job_status", "Internal server error"+ str(e),500,FILENAME)
                return make_response("Client not found", 403)
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
        apikey = request.headers.get(APIKEY)
        identification = data.get(RX_CLIENT_ID, '')
        logger=Logger()
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == 0:
                logger.log("INFO", "force_update", "Forcing update",0,FILENAME)
            else:
                for i in CLIENTS:
                    if str(i[1]) == str(identification):
                        url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/force_update"
                        try:
                            logger.log("INFO", "force_update", f"Forcing update on {url}",0,FILENAME)
                            return requests.post(url, json={APIKEY:MySqlite.read_setting(APIKEY), "ID": identification},timeout=TIMEOUT)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "force_update.relay.force_update", f"Timeout connecting to {i[0]}",
                            "500", FILENAME)
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "force_update.relay.force_update", f"Error connecting to {i[0]}",
                            "500", FILENAME)
                        except Exception as e:
                            logger.log("ERROR", "force_update.relay.force_update", "Internal server error"+ str(e),500,FILENAME)
                return make_response("Client not found", 403)
        else:
            return make_response("401 Access Denied", 401)


    @website.route('/get_version/<int:identification>', methods=['GET'], )
    @staticmethod
    def get_version(identification):
        """
        Gets version information from the client
        """
        # get client secret from header
        apikey = request.headers.get(APIKEY)
        global CLIENTS
        CLIENTS = MySqlite.load_clients()
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            if identification == 0:
                logger.log("INFO", "get_version", "Getting version",0,FILENAME)
                content= {
                    VERSION_SETTING: MySqlite.read_setting(VERSION_SETTING),
                    "tag": MySqlite.read_setting(VERSION_TAG_SETTING)
                }
                return make_response(content,200)
            else:
                for i in CLIENTS:
                    if i[1] == identification:
                        url = f"{TENANT_PROTOCOL}{i[2]}:{i[3]}/get_version"
                        try:
                            logger.log("INFO", "get_version", f"Getting version from {url}",0,FILENAME)
                            return requests.post(url, json={APIKEY:MySqlite.read_setting(APIKEY), "ID": identification},timeout=TIMEOUT)
                        except requests.exceptions.ConnectTimeout :
                            logger.log("ERROR", "get_version.relay.get_version", f"Timeout connecting to {i[0]}",
                            "500",FILENAME)
                        except requests.exceptions.ConnectionError:
                            logger.log("ERROR", "get_version.relay.get_version", f"Error connecting to {i[0]}",
                            "500", FILENAME)
                        except Exception as e:
                            logger.log("ERROR", "get_version.relay.get_version", "Internal server error"+ str(e),500,FILENAME)
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
        apikey = request.headers.get(APIKEY)
        identification = data.get(RX_CLIENT_ID, '')
        logger=Logger()
        if FlaskServer.auth(apikey, logger, identification) == 200:
            jobs = []
            logger.log("INFO", "get_jobs", "Getting server job",0,FILENAME)
            j:Job = Job()
            j.load(0)
            try:
                data = {
                        "0":{
                        "title": j.get_title(),
                        RX_CLIENT_ID: "0",
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
            except Exception as e:
                pass
            jobs.append(data)
            for client in CLIENTS:
                url = f"http://{client[2]}:{client[3]}/get_job"
                try:
                    logger.log("INFO", "get_jobs", f"Getting job from {url}",0,FILENAME)
                    response= requests.get(url, headers={APIKEY:apikey,"Content-Type": "application/json"},json={},timeout=TIMEOUT,verify=VERIFY)
                    jobs.append(response.json())
                except requests.exceptions.ConnectTimeout :
                    logger.log("ERROR", "getjobs.relay.getjob", f"Timeout connecting to {client[0]}",
                    "500", FILENAME)
                except requests.exceptions.ConnectionError:
                    logger.log("ERROR", "get_jobs.relay.get_job", f"Error connecting to {client[0]}",
                    "500", FILENAME)
                except Exception as e:
                    logger.log("ERROR", "get_jobs.relay.get_job", "Internal server error"+ str(e),500,FILENAME)
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
        identification = request.headers.get('id')
        logger = Logger()
        client_list = MySqlite.load_clients()
        for client in client_list:
            if str(client[0]) == str(identification):
                if MySqlite.read_setting(APIKEY) == secret:
                    logger.log("INFO","beat",f"Client heartbeat added{identification}",200,FILENAME)
                    MySqlite.update_heartbeat_time(identification)
                    header ={
                    "Content-Type":"application/json",
                    APIKEY:MySqlite.read_setting(APIKEY)
                    }
                    content = {
                    RX_CLIENT_ID: int(identification),
                    RX_UUID: MySqlite.get_client_uuid(identification),
                    "status": 1# Online device status

                    }

                    try:
                        server_address = MySqlite.read_setting(MSP_SERVER_SETTING)
                        msp_port = MySqlite.read_setting(MSP_PORT_SETTING)
                        _ = requests.put(f"{MSP_PROTOCOL}{server_address}:{msp_port}{UPDATE_DEVICE_STATUS_ROUTE}", headers=header, json=content,timeout=TIMEOUT,verify=VERIFY)
                    except Exception:
                        return False
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
        secret = request.headers.get(APIKEY)
        if MySqlite.read_setting(APIKEY) == secret:
            try:
                req = requests.request("GET", f"{MSP_PROTOCOL}{MySqlite.read_setting(MSP_SERVER_SETTING)}:{MySqlite.read_setting(MSP_PORT_SETTING)}/{URLS_ROUTE}",
                            timeout=TIMEOUT, headers={"Content-Type": "application/json",APIKEY:secret}, verify=VERIFY)
            except Exception as e:
                Logger.log("ERROR","urls","flask",f"Request to MSP: {e}",500, FILENAME)
                return make_response("Connection Error",403)

            return make_response(req.content, str(req.status_code))


    @website.route('/make_smb', methods=['POST'], )
    @staticmethod
    def make_smb():
        """
        Makes a smb share
        """
        data = request.get_json()
        # get client secret from header
        apikey = request.headers.get(APIKEY)
        name = data.get('Name', '')
        path = data.get('Path', '')
        password = data.get('Password', '')
        username = data.get('Username', '')
        identification = data.get('CLIENT_ID', '')
        #path = os.path.normpath(path) leae out so wbadmin has \\\\device\\path over \\device\path
        logger=Logger()
        auth = FlaskServer.auth(apikey, logger,  MySqlite.read_setting(CLIENT_ID_SETTING))
        if auth == 200:
            if identification is not None:
                logger.log("INFO", "make_smb", f"Making smb share {name} {path} {username} {password}",200,FILENAME)
                identification=MySqlite.write_backup_server(name, path, username, password)
                content = {
                    "id":identification
                }
                return make_response(content, 200)
            else:
                logger.log("INFO","make_smb","editing smb",0,FILENAME)
                MySqlite.edit_backup_server(int(identification), name, path, username, password)
                return make_response("200 OK", 200)
        elif auth == 405:
            logger.log("ERROR", "make_smb", "Access Denied",401,FILENAME)
            return make_response("401 Access Denied", 401)
        else:
            return make_response("500 Internal Server Error", 500)
    @website.route('/get_smb/<int:identification>', methods=['GET'], )
    @staticmethod
    def get_smb(identification):
        """
        Gets smb share information
        """

        # get client secret from header
        apikey = request.headers.get(APIKEY)
        logger=Logger()
        auth=FlaskServer.auth(apikey, logger,  MySqlite.read_setting(CLIENT_ID_SETTING))
        if auth == 200:
            logger.log("INFO", "get_smb", f"Getting smb share {identification}",200,FILENAME)
            server= MySqlite.get_backup_server(int(identification))

            # format smb info as json
            data={}
            try:
                data = {
                    "Name":server[4],
                    "Path":server[1],
                    "Username":server[2],
                    "Password":server[3]
                }
            except Exception as e:
                pass
            return make_response(data, 200)
        elif auth == 405:
            return make_response("401 Access Denied", 401)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/delete_smb/<int:identification>', methods=['DELETE'], )
    @staticmethod
    def delete_smb(identification):
        """
        Deletes a smb share
        """
        # get client secret from header
        apikey = request.headers.get(APIKEY)
        logger=Logger()
        auth = FlaskServer.auth(apikey, logger, MySqlite.read_setting("id"))
        if auth == 200:
            logger.log("INFO", "delete_smb", f"Deleting smb share {identification}",200,FILENAME)
            MySqlite.delete_backup_server(int(identification))
            return make_response("200 OK", 200)
        elif auth == 405:
            logger.log("ERROR", "delete_smb", "Access Denied",401,FILENAME)
            return make_response("401 Access Denied", 401)
        else:
            return make_response("500 Internal Server Error", 500)

    @website.route('/verify', methods=['GET'], )
    @staticmethod
    def verify():
        """
        Client can verify a successful install
        """
        # get client secret from header
        apikey = request.headers.get(APIKEY)
        logger=Logger()
        auth = FlaskServer.auth(apikey, logger, MySqlite.read_setting("id"))
        if auth == 200:
            content = request.get_json()
            client_id = content.get('client_id', '')
            uuid = content.get('uuid', '')
            installationKey = content.get('installationKey', '')
            try:
                payload = {
                "client_Id":client_id,
                "uuid":uuid,
                "installationKey":installationKey
                }

                _ = requests.request("POST", f"{"https://"}{MySqlite.read_setting("msp_server_address")}:{MySqlite.read_setting("msp-port")}/api/datalink/verify", 
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
                        json=payload, verify=False)
            except ConnectionError:
                make_response("403 Timeout",403)
            except Exception as e:
                return make_response("500 Internal Server Error", 500)
        else:
            return make_response("401 Access Denied", 401)
        
    @website.route('/update_job_status', methods=['POST'], )
    @staticmethod
    def update_job_status():
        apikey = request.headers.get(APIKEY)
        logger=Logger()
        auth = FlaskServer.auth(apikey, logger, MySqlite.read_setting("id"))
        if auth == 200:
            data = request.get_json()
            client_id = data.get('client_id', '')
            status = data.get('status', '')
            percent = data.get('percent', '')
            try:
                payload = {
                "client_id":client_id,
                "status":status,
                "progress":percent,
                "uuid":MySqlite.read_setting("uuid")
                }

                req = requests.request("PUT", f"{"https://"}{MySqlite.read_setting("msp_server_address")}:{MySqlite.read_setting("msp-port")}/api/datalink/Update_Job_Status", 
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
                        json=payload, verify=False)
                return make_response(req.content, req.status_code)
            except ConnectionError:
                make_response("403 Timeout",403)
            except Exception as e:
                return make_response("500 Internal Server Error", 500)
        else:
            return make_response("401 Access Denied", 401)
        pass

            
    # PUT ROUTES
    @website.route('/check-installer', methods=['GET'], )
    @staticmethod
    def check_installer():
        """
        Gets version information from the client and requests an install with the MSP --INTERNAL--
        """
        secret = request.headers.get(APIKEY)
        key = request.get_json().get('installationKey', '')
        logger = Logger()

        if MySqlite.read_setting(APIKEY) == secret:
            # has valid api key

            InitSql.clients()
            # pull all info from the body
            body = request.get_json()
            identification = MySqlite.get_next_client_id()
            name = body.get('name', '')
            ip = body.get('ipaddress', '')
            port = body.get('port', '')
            status = 'Installing'
            mac = body["macaddresses"]
            uid = body.get('uuid', '')
            installtype = body.get('type', '')
            
            # ensure no duplicate client ip's
            clients = MySqlite.load_clients()
            for client in clients:
                if client[2] == ip:
                    logger.log("ERROR", "check-installer", "403 - PC Already connected",403,FILENAME)
                    return make_response("403 - PC Already connected", 403)
            # reformat and send to msp

            payload = {
            "name":name,
            RX_UUID:uid,
            "client_id":identification,
            "ipaddress":ip,
            "port":port,
            "type":installtype,
            "macaddresses":mac,
            "installationKey":key
        }
            try:
                req = requests.request("POST", f"{MSP_PROTOCOL}{MySqlite.read_setting(MSP_SERVER_SETTING)}:{MySqlite.read_setting(MSP_PORT_SETTING)}/{CLIENT_REGISTRATION_PATH}",
                            timeout=TIMEOUT, headers={"Content-Type": "application/json",APIKEY:secret}, json=payload, verify=VERIFY)
            except TimeoutError:
                logger.log("ERROR", "check-installer", "Timeout connecting to MSP",500,FILENAME)
                return make_response("403 Timeout",403)
            except Exception as e:
                logger.log("ERROR", "check-installer", f"Failed to connect to MSP: {e}", 500, FILENAME)
                return make_response("Failed to contact MSP", 500)
            logger.log("INFO", "check-installer", f"Request to MSP: {req.status_code}", 200, FILENAME)
            # if msp returns 200 ok, then return 200 ok
            if req.status_code == 200:
                result = MySqlite.write_client(identification, name, ip, port, status, mac[0]['address'],uid)
                make_response(str(result), 200)
                if str(result) == str(200):
                    # verify for the MSP

                    return make_response("200 ok", 200, {"clientid": identification,MSP_API_SETTING:MySqlite.read_setting(MSP_API_SETTING)})
                else:
                    return make_response("500 Internal Server Error - CODE: 1000" + str(result), 403)
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
        secret = request.headers.get(APIKEY)
        key = request.json.get('key','')

        uuid = request.json.get('uuid', '')
        client_id = request.json.get('client_id', '')
        logger = Logger()
        if FlaskServer.auth(secret, logger, 0) == 200:
            headers = {
            "Content-Type": "application/json",
            APIKEY: secret
            }
            content = {
            "uninstallationKey": key,
            RX_UUID: uuid,
            "client_Id": client_id
            }
            try:
                response = requests.post(f"{MSP_PROTOCOL}{MySqlite.read_setting(MSP_SERVER_SETTING)}:{MySqlite.read_setting(MSP_PORT_SETTING)}{UNINSTALL_ROUTE}",
                                        headers=headers, json=content, timeout=TIMEOUT, verify=VERIFY)
                if response.status_code == 200:
                    return make_response(response.content, response.status_code)
                else:
                    logger.log("ERROR", "uninstall", f"Failed to connect to MSP: {response.status_code}", response.status_code, FILENAME)
                    return make_response(response.content, response.status_code)

            except Exception as e:
                logger.log("ERROR", "uninstall", f"Failed to connect to MSP: {e}", 500, FILENAME)
                return make_response("Failed to contact MSP", 500)
        else:
            logger.log("ERROR", "uninstall", "Access Denied",405,FILENAME)
            return make_response("401 Access Denied", 401)
    # HELPERS
    def run(self):
        """
        Runs the server
        """
        global RUN_JOB_OBJECT
        RUN_JOB_OBJECT = RunJob()
        self.website.run(port=PORT, host=HOST)
    def __init__(self):
        # load all clients from DB
        Logger.debug_print("flask server started")
        self.run()
