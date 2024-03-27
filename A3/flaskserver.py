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

"""
# pylint: disable= import-error, global-statement,unused-argument, line-too-long
import os
import time
import requests
from flask import Flask, request,make_response
from security import Security, CLIENT_SECRET
from logger import Logger
from runjob import RunJob  

RUN_JOB_OBJECT = None
CLIENTS = (["127.0.0.1",0],["10.0.0.2",1])



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


    @staticmethod
    def get_local_files():
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
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        client_id = data.get('clientid', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if i[1] == client_id: # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[0]}:5000/start_job"
                    try:
                        if i[0]== "127.0.0.1":
                            return FlaskServer.get_local_files()
                        else:
                            return requests.post(url, json={"clientSecret": Security.encrypt_client_secret(Security.add_salt_pepper(Security.sha256_string(CLIENT_SECRET),"salt","pepricart","salt2")), "ID": identification},timeout=10)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "start_job", f"Timeout connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "start_job", f"Error connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500


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
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        client_id = data.get('clientid', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if i[1] == client_id: # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[0]}:5000/start_job"
                    try:

                        if i[0]== "127.0.0.1":
                            code = 200
                            msg= "local job triggered"

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
                        else:
                            return requests.post(url, json={"clientSecret": Security.encrypt_client_secret(Security.add_salt_pepper(Security.sha256_string(CLIENT_SECRET),"salt","pepricart","salt2")), "ID": identification},timeout=10)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "start_job", f"Timeout connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "start_job", f"Error connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500


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
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        client_id = data.get('clientid', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if i[1] == client_id: # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[0]}:5000/stop_job"
                    try:
                        if i[0]== "127.0.0.1":
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
                        else:
                            return requests.post(url, json={"clientSecret": Security.encrypt_client_secret(Security.add_salt_pepper(Security.sha256_string(CLIENT_SECRET),"salt","pepricart","salt2")), "ID": identification},timeout=10)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "start_job", f"Timeout connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "start_job", f"Error connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500


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
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        client_id = data.get('clientid', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if i[1] == client_id: # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[0]}:5000/kill_job"
                    try:
                        if i[0]== "127.0.0.1":
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
                        else:
                            return requests.post(url, json={"clientSecret": Security.encrypt_client_secret(Security.add_salt_pepper(Security.sha256_string(CLIENT_SECRET),"salt","pepricart","salt2")), "ID": identification},timeout=10)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "start_job", f"Timeout connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "start_job", f"Error connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500


        if code==0:
            return "200 OK"
        else:
            return make_response(msg, code)


    @website.route('/enable_job', methods=['POST'], )
    @staticmethod
    def enable_job():
        """
        Triggers the RunJob with the job assigned to this computer
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
        """

        logger=Logger()
        # get the json body
        data = request.get_json()
        # get the clientSecret from the json body
        recieved_client_secret = data.get('clientSecret', '')
        # get the ID from the json body
        identification = data.get('ID', '')
        client_id = data.get('clientid', '')
        code = 0
        msg = ""
        if FlaskServer.auth(recieved_client_secret, logger, identification) == 405:
            code = 401
            msg = "Access Denied"
        else:
            for i in CLIENTS:
                msg = "Client not found"
                code=5
                if i[1] == client_id: # find the client address to match the ID passed where 0 is localhost
                    url = f"http://{i[0]}:5000/kill_job"
                    try:
                        if i[0]== "127.0.0.1":
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
                        else:
                            return requests.post(url, json={"clientSecret": Security.encrypt_client_secret(Security.add_salt_pepper(Security.sha256_string(CLIENT_SECRET),"salt","pepricart","salt2")), "ID": identification},timeout=10)
                    except requests.exceptions.ConnectTimeout :
                        logger.log("ERROR", "start_job", f"Timeout connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Timeout connecting to {i[0]}"
                    except requests.exceptions.ConnectionError:
                        logger.log("ERROR", "start_job", f"Error connecting to {i[0]}",
                        "500", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
                        code=402
                        msg=f"Error connecting to {i[0]}"
                    except:
                        msg = "Internal server error"
                        code = 500


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
        return "200 OK"
    @website.route('/get_job', methods=['GET'], )
    @staticmethod
    def get_job():
        """
        Gives Current Job Information
        """
        return "200 OK"
    @website.route('/force_checkin', methods=['GET'], )
    @staticmethod
    def force_checkin():
        """
        Forces a heartbeat
        """
        return "200 OK"
    @website.route('/restore', methods=['POST'], )
    @staticmethod
    def restore():
        """
        Restores files or directories
        """
        return "200 OK"

    @website.route('/get_Status', methods=['GET'], )
    @staticmethod
    def get_status():
        """
        Gets the current status of running jobs or error state, version information etc
        """
        return "200 OK"

    @website.route('/force_update', methods=['PUT'], )
    @staticmethod
    def force_update():
        """
        Forces the client to pull an update from the server
        """
        return "200 OK"

    @website.route('/get_version', methods=['GET'], )
    @staticmethod
    def get_version():
        """
        Gets version information from the client
        """
        return "200 OK"

    @website.route('/get_heartbeats', methods=['GET'], )
    @staticmethod
    def get_heartbeats():
        """
        Gets version information from the client
        """
        return "200 OK"
 
    @website.route('/get_backup', methods=['GET'], )
    @staticmethod
    def get_backup():
        """
        Gets version information from the client
        """
        return "200 OK"

    @website.route('/get_jobs', methods=['GET'], )
    @staticmethod
    def get_jobs():
        """
        Gets version information from the client
        """
        return "200 OK"

    @website.route('/verify_backup', methods=['PUT'], )
    @staticmethod
    def verify_backup():
        """
        Gets version information from the client
        """
        return "200 OK"



    # PUT ROUTES





    # HELPERS
    def run(self):
        """
        Runs the server
        """
        global RUN_JOB_OBJECT
        RUN_JOB_OBJECT = RunJob()
        self.website.run()
    def __init__(self):
        Logger.debug_print("flask server started")
        self.run()
