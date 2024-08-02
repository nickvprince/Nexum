"""
info
"""
import sys
import threading
import subprocess
from flask import Flask,make_response,request
import servicemanager
import win32event
import win32service
from pyuac import main_requires_admin
import win32serviceutil
from security import Security
import tempfile
import sqlite3
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import base64

app = Flask(__name__)

# decrypt a string using AES
@staticmethod
def decrypt_string(password, string):
    """
    Decrypt a string with AES-256 bit decryption
    """
    # Pad the password to be 16 bytes long
    password_hashed = str(password).ljust(16).encode('utf-8')

    # Create a new AES cipher with the password as the key
    cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
    decryptor = cipher.decryptor()

    # Decode the string from base64
    decoded_string = base64.b64decode(string)

    # Decrypt the string using AES
    decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()
    try:
        return decrypted_string.decode('utf-8')
    except UnicodeDecodeError:
        return "Decryption failed"
    except:
        return "Decryption failed"
    
@staticmethod
def read_setting(setting):
    """
    Read a setting from the database
    """
    # get temp directory from cmd %temp%



    try:
        conn = sqlite3.connect(tempfile.gettempdir()+'\\settings\\settings.db')
        cursor = conn.cursor()
        cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
        value = cursor.fetchone()[0]
        conn.close()
        result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
                                    capture_output=True, text=True,check=True,shell=True) # enc with uuid
        output = result.stdout.strip()
        output = output.split('\n\n', 1)[-1]
        output = output[:32]
        value = decrypt_string(output,value).rstrip()
        return value
    except Exception as e:
        return e

def auth(recieved_client_secret):
    """     
    Authenticate with the APIKEY
    """
    if str(recieved_client_secret)== read_setting("apikey"):
        return 200
    return 405

@app.route('/start_job_service',methods=['POST'])
def start_job(start_job_commands=""):
    """
    starts a job
    """
    # add auth
    recieved_client_secret = request.headers.get('apikey')
    if(auth(recieved_client_secret) == 200):
        try:
            start_job_commands = request.json['start_job_commands']
            print("wbadmin start backup "+str(start_job_commands))
            result =subprocess.Popen(["powershell.exe", "wbadmin start backup "+str(start_job_commands)],stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
            output = result.stdout
            response = {"result": "{"+str(output.readlines())+"}"}
            print(response)
            return response
        except Exception as e:
            return make_response(str(e) + "\n"+str(read_setting("apikey")+"\n" + str(recieved_client_secret)), 500)
    else:
        return make_response("401 Unauthorized", 401)

@app.route('/stop_job_service',methods=['POST'])
def stop_job():
    """
    Stops the job
    """
    # add auth
    recieved_client_secret = request.headers.get('apikey')
    if(auth(recieved_client_secret) == 200):
        try:
            command = "wbadmin stop job -quiet"
            p = subprocess.Popen(['powershell.exe', command],shell=True,stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            output = p.stdout
            response = {"result": "{"+str(output.readlines())+"}"}
            print(response)
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
    recieved_client_secret = request.headers.get('apikey')
    if(auth(recieved_client_secret) == 200):
        try:
            result = subprocess.Popen(["powershell.exe", "wbadmin Get status"], stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
            output = result.stdout
            resp = ""
            value =output.read(160)

            response = {"result": "{"+str(value)+"}"}
            print(response)
            return response
        except Exception as e:
            return make_response(str(e), 500)
    else:
        return make_response("405 Unauthorized", 405)
    
@main_requires_admin
def main():
    flask_thread = threading.Thread(target=app.run, kwargs={'host':'0.0.0.0','port': 5004})
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