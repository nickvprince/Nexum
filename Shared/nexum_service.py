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
from security import Security

app = Flask(__name__)



def auth(recieved_client_secret):
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
    temp = Security.sha256_string(Security.get_client_secret())
    temp = Security.add_salt_pepper(temp, "salt", "pepricart", "salt2")
    print("temp: "+str(temp))
    print("recieved_client_secret: "+str(recieved_client_secret))

    if str(recieved_client_secret) == temp:
        return 200
    return 405

@app.route('/start_job_service',methods=['POST'])
def start_job(start_job_commands=""):
    """
    starts a job
    """
    # add auth
    recieved_client_secret = request.headers.get('clientSecret')
    if(auth(recieved_client_secret) == 200):
        try:
            start_job_commands = request.json['start_job_commands']
            print("wbadmin start backup "+str(start_job_commands))
            result =subprocess.Popen(["powershell.exe", "wbadmin start backup "+str(start_job_commands)],stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
            output = result.stdout
            response = {"result": "{"+str(output.readlines())+"}"}
            return response
        except Exception as e:
            return make_response(str(e), 500)
    else:
        return make_response("405 Unauthorized", 405)

@app.route('/stop_job_service',methods=['POST'])
def stop_job():
    """
    Stops the job
    """
    # add auth
    recieved_client_secret = request.headers.get('clientSecret')
    if(auth(recieved_client_secret) == 200):
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
    else:
        return make_response("405 Unauthorized", 405)

@app.route('/get_status',methods=['GET'])
def get_status():
    """
    returns the status of a job
    """
    # add auth
    #get clientSecret from header
    recieved_client_secret = request.headers.get('clientSecret')
    if(auth(recieved_client_secret) == 200):
        try:
            result = subprocess.Popen(["powershell.exe", "wbadmin Get status"], stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
            output = result.stdout
            resp = ""
            value =output.read(160)

            response = {"result": "{"+str(value)+"}"}
            return response
        except Exception as e:
            return make_response(str(e), 500)
    else:
        return make_response("405 Unauthorized", 405)
    
@main_requires_admin
def main():
    flask_thread = threading.Thread(target=app.run, kwargs={'port': 5004})
    flask_thread.daemon=True
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
       