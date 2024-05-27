from flask import Flask
import subprocess
import json
from flask import make_response,request
import requests
ASADMIN = 'asadmin'

app = Flask(__name__)

@app.route('/start_job_service',methods=['POST'])
def start_job(start_job_commands=""):
    """
    starts a job
    """
    # add auth
    try:
        start_job_commands = request.json['start_job_commands']
        print("wbadmin start backup "+str(start_job_commands))
        result =subprocess.Popen(["powershell.exe", "wbadmin start backup "+str(start_job_commands)],stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
        output = result.stdout
        response = {"result": "{"+str(output.readlines())+"}"}
        return response
    except Exception as e:
        return make_response(str(e), 500)

@app.route('/stop_job_service',methods=['POST'])
def stop_job():
    """
    Stops the job
    """
    # add auth
    try:
        command = "wbadmin stop job -quiet"
        p = subprocess.Popen(['powershell.exe', command],shell=True,stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        output = p.stdout
        response = {"result": "{"+str(output.readlines())+"}"}
        return response
    except Exception as e:
        return make_response(str(e), 500)
    # Code for stopping a job goes here
    return "Job stopped"

@app.route('/get_status',methods=['GET'])
def get_status():
    """
    returns the status of a job
    """
    # add auth
    try:
        result = subprocess.Popen(["powershell.exe", "wbadmin Get status"], stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
        output = result.stdout
        response = {"result": "{"+str(output.readlines())+"}"}
        return response
    except Exception as e:
        return make_response(str(e), 500)


if __name__ == "__main__":
    app.run(port=5004)
