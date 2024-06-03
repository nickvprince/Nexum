from flask import Flask
import subprocess
from flask import make_response,request
import threading
import winreg
import time
import servicemanager
import socket
import sys
import win32event
import win32service
from pyuac import main_requires_admin
import win32serviceutil


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
        resp = ""
        value =output.read(160)

        response = {"result": "{"+str(value)+"}"}
        return response
    except Exception as e:
        return make_response(str(e), 500)

@main_requires_admin
def main():
    flask_thread = threading.Thread(target=app.run, kwargs={'port': 5004})
    flask_thread.setDaemon(True)
    flask_thread.start()

class NexumService(win32serviceutil.ServiceFramework):
    _svc_name_ = "NexumService"
    _svc_display_name_ = "Nexum Service"
    _svc_description_ = "Service to run Nexum Flask application and watchdogs"
    
    def __init__(self, args):
        win32serviceutil.ServiceFramework.__init__(self, args)
        self.hWaitStop = win32event.CreateEvent(None, 0, 0, None)
        self.is_running = True

    def SvcStop(self):
   
        self.ReportServiceStatus(win32service.SERVICE_STOP_PENDING)
        self.is_running = False
        win32event.SetEvent(self.hWaitStop)
        
        sys.exit(0)

   

    def SvcDoRun(self):

        servicemanager.LogMsg(servicemanager.EVENTLOG_INFORMATION_TYPE,
                              servicemanager.PYS_SERVICE_STARTED,
                              (self._svc_name_, ''))
        self.ReportServiceStatus(win32service.SERVICE_RUNNING)
        self.main()
     

    def main(self):
        while self.is_running:
            win32event.WaitForSingleObject(self.hWaitStop, win32event.INFINITE)
            main()

if __name__ == '__main__':
    main()
    if len(sys.argv) > 1:
       # Called by Windows shell. Handling arguments such as: Install, Remove, etc.
       win32serviceutil.HandleCommandLine(NexumService)
    else:
       # Called by Windows Service. Initialize the service to communicate with the system operator
       servicemanager.Initialize()
       servicemanager.PrepareToHostSingle(NexumService)
       servicemanager.StartServiceCtrlDispatcher()