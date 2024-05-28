from flask import Flask
import subprocess
from flask import make_response,request
import threading
import winreg
import time


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

def nexclient_watchdog():
    print("nexclient_watchdog")
    while True:
        #check if nexum.exe is running
        try:
            nexum = subprocess.Popen(["tasklist", "/fi", "imagename eq nexum.exe"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)

            output = nexum.stdout.readline()
            if "specified" in str(output):
                # start nexum.exe
                try:
                    subprocess.Popen(["start", "nexum"], shell=True)
                    print("nexum.exe starting")
                except:
                    pass
                
            else:
                print("nexum.exe is running")   
                
        except Exception as e:
            print(str(e))
        time.sleep(5)


def nexserv_watchdog():
    print("nexserv_watchdog")
    while True:
        #check if nexum.exe is running
        try:
            nexum = subprocess.Popen(["tasklist", "/fi", "imagename eq nexserv.exe"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)

            output = nexum.stdout.readline()
            if "specified" in str(output):
                # start nexum.exe
                try:
                    subprocess.Popen(["start", "nexserv"], shell=True)
                    print("nexumserv.exe starting")
                except:
                    pass
                
            else:
                print("nexumserv.exe is running")   
                
        except Exception as e:
            print(str(e))
        time.sleep(5)


def watchdog():
    try:
        key_path = r"Software\Microsoft\Windows\CurrentVersion\Run"
        nexum_key = "nexum"
        nexserv_key = "nexserv"
        nexserv_key_reg = None
        nexum_key_reg = None
        # Check if the nexum key exists
        run_key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, key_path, 0, winreg.KEY_READ)
        try:
            nexum_key_reg = winreg.QueryValueEx(run_key, nexum_key)
        except:
            pass
        try:
            nexserv_key_reg = winreg.QueryValueEx(run_key, nexserv_key)
        except:
            pass
        winreg.CloseKey(run_key)
        if nexum_key_reg is None:
            print("nexum key not found")
        else:
            nexclient_watchdog()
        if nexserv_key_reg is None:
            print("nexserv key not found")
        else:
            nexserv_watchdog()
    except Exception as e:
        print(str(e))
if __name__ == "__main__":
    # Create a new thread for the Flask app
    flask_thread = threading.Thread(target=app.run, kwargs={'port': 5004})
    watchdog_thread = threading.Thread(target=watchdog)
    # Start the Flask app thread
    flask_thread.start()
    watchdog_thread.start()

    # Wait for the Flask app thread to finish
    flask_thread.join()
    watchdog_thread.join()