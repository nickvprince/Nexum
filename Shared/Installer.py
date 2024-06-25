"""
# Program: Tenant-server, Tenant-client
# File: InstallerV2.py
# Authors: 1. Danny Smith
#
# Date: 5/13/2024
# purpose: 
# This file contains the Installer program for the Tenant-server and Tenant-client.
# it allows the user to install the server, client, or uninstall the program.

# Class Types: 
#               1. Installer

# Error Codes
# -----------
# 1100 - Could not connect to server
# 1101 - Incorrect Secret
# 1102 - Error adding registry keys
# 1103 - Could not delete registry keys
# 1104 - Startup key could not be deleted
# 1105 - App key could not be deleted
# 1106 - Run key could not be deleted
# 1107 - Nexum folder could not be deleted
# 1108 - Nexum folder does not exist
# 1115 - Could not uninstall from server



# Functions:
#               1. uninstall_program()
#               2. uninstall(window:tk.Tk)
#               3. install_nexum_file()
#               4. install_watchdog_file()+
#               5. notify_server()
#               6. install_client_background(window:tk.Tk, backupserver:str, key:str)
#               7. install_client(window:tk.Tk)
#               8. install_server(window:tk.Tk)
#               9. main_window(window:tk.Tk)
#               10. main()
#               11. install_server_background(window:tk.Tk, backupserver:str, key:str)
#               12. install_persistence(client_server:int)
#               13. notify_msp()
#               14. install_client_background(window:tk.Tk, backupserver:str, key:str)
#               15. install_client(window:tk.Tk)
#               16. install_server(window:tk.Tk)
#               17. main_window(window:tk.Tk)

# Note. This file will fail. current_directory pulls from a temp directory 
# in programdata and not the actual directory, this will be rectified when 
# pulling the files from the msp

"""

import base64
import tkinter as tk
import winreg
import shutil
import subprocess
import time
import socket
import uuid
import os
import sqlite3
from PIL import ImageTk, Image
import requests
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import sys
import win32com.shell.shell as shell
from task import client_persistance, server_persistance
import requests


ASADMIN = 'asadmin'
job_settingsFile=os.path.join('/settings.db')
VERIFY_PATH="api/DataLink/verify"
#pylint: disable= bare-except, broad-except
EXE_SERVICE_NAME = "nexum_service.exe"
REGISTRATION_PATH = "api/DataLink/Register"
CLIENT_REGISTRATION_PATH = "check-installer"
AUTO_RUN_KEY = r"Software\Microsoft\Windows\CurrentVersion\Run"
APP_PATH_KEY = r"Software\Microsoft\Windows\CurrentVersion\App Paths"
STARTUP_APPROVED_KEY = r"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run"
TITLE = "Nexum"
TITLE_APPENDED = "\\Nexum"
SECRET = "wQ75NoTBd/KF5fTvUTViY4IoXhdWL+zrJD0+DUtg6BD4l3XdB25elvUkNEKbIImFuM4o7ZCKrocWXEju26eNPoUbHVcmCxXNJNh2VN8tcxafMVgGGmcoaCJNhDj2bvIA"
OS_FILE_PATH = r"C:\Program Files\Nexum"
EXE_NEXUM_NAME = "Nexum.exe"
EXE_SERVER_NAME = "nexserv.exe"
EXE_WATCHDOG_NAME = "watchdog.exe"
EXE_SERV_WATCHDOG_NAME = "watchdogserv.exe"
WINDOW_GEOMETRY = "1000x600"
PORT = 5000
HYPER_PROTOCOL = "https://"
IMAGE_PATH = '../Data/Nexum.png'
TIMEOUT = 30
BEAT_PATH = "beat"
UNINSTALL_PATH = "uninstall"
current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
settingsDirectory = os.path.join(current_dir, '..\\settings') # directory for settings
SETTINGS_PATH= os.path.join(current_dir, 
    settingsDirectory+'\\settings.db') # path to the settings database
# pah directories
current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
logdirectory = os.path.join(current_dir,'../logs') # directory for logs
logpath = os.path.join('/log.db') # path to the log database
URLS_ROUTE="api/DataLink/Urls"
SSL_CHECK=False

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

    conn = sqlite3.connect(SETTINGS_PATH)
    cursor = conn.cursor()
    cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
    value = cursor.fetchone()[0]
    conn.close()
    result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
                                capture_output=True, text=True,check=True,shell=True) # enc with uuid
    output = result.stdout.strip()
    output = output.split('\n\n', 1)[-1]
    output = output[:24]
    value = decrypt_string(output,value)
    return value.rstrip()

def write_log(severity, subject, message, code, date):
    """ 
    Write a log to the database
    """
    conn = sqlite3.connect(logdirectory+logpath)
    identification= 0
    cursor = conn.cursor()


    cursor.execute('''SELECT MAX(id) FROM logs''')
    result = cursor.fetchone()[0]
    if result is not None:
        identification = int(result) + 1
    else:
        identification = 1


    cursor.execute('''INSERT INTO logs (id,severity, subject, message, code, date)
                    VALUES (?,?, ?, ?, ?, ?)''',
                    (identification, severity, subject, message, code, date))
    conn.commit()
    conn.close()

def uninstall_from_server(backserver:str, key:str):
    """
    Information
    """
    write_log("INFO", "Uninstall", "Uninstall from server started", 0, time.time())
    try:
        payload = {
            "clientid":read_setting("CLIENT_ID")
        }
        write_log("INFO", "Uninstall", "Payload: " + str(payload), 0, time.time())
        write_log("INFO", "Uninstall", "Request: " + f"http://{backserver}/{UNINSTALL_PATH}", 0, time.time())
        request = requests.request("GET", f"http://{backserver}/{UNINSTALL_PATH}",
                                    timeout=TIMEOUT, headers={"Content-Type": "application/json",
                                    "key":key, "clientSecret":SECRET}, json=payload, verify=SSL_CHECK)
        write_log("INFO", "Uninstall", "Request: " + str(request.content), 0, time.time())
        if request.status_code == 200:
            write_log("INFO", "Uninstall", "Uninstall from server successful", 0, time.time())
            return 200
        else:
            write_log("ERROR", "Uninstall", "Uninstall from server failed", 1115, time.time())
            return 400
    except:
        write_log("ERROR", "Uninstall", "Could not connect to server: ", 1115, time.time())
        return 400
    
    
def completed(window:tk.Tk,message:str,state:str):
    """
    Information 
    """
    write_log("INFO", "Completed", "completed window opened - "+str(message) + " - " + str(state), 0, time.time())
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Nexum - Completed")
    new_window.geometry(WINDOW_GEOMETRY)
    new_window.resizable(False, False)  # Set resizable to False
    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs
    image = Image.open(working_dir)
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(new_window, image=photo)
    image_label.pack(pady=50)

    message_label = tk.Label(new_window, text=message)
    message_label.place(relx=0.5, rely=0.6, anchor=tk.CENTER)

    state_label = tk.Label(new_window, text=state)
    state_label.place(relx=0.5, rely=0.8, anchor=tk.CENTER)

    back_button = tk.Button(new_window, text="Home", width=25,height=5, command=lambda: main_window(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                            highlightthickness=0, highlightbackground="black",
                            highlightcolor="black", padx=10, pady=5, font=("Arial", 10),
                            overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.9, anchor=tk.CENTER)
    new_window.mainloop()
def uninstall_program(key:str,window:tk.Tk):
    """
    Information
    """
    not_installed_indentifiers:int = 0
    identifiers_count:int =0

    identifiers_count += 1
    if uninstall_from_server(read_setting("server_address")+":"+read_setting("server_port"),key) == 200:
        not_installed_indentifiers += 1

        identifiers_count += 1
        try:
            key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY, 0, winreg.KEY_ALL_ACCESS)
            winreg.DeleteValue(key, TITLE)
            not_installed_indentifiers += 1
            write_log("INFO", "Uninstall", "Run key deleted", 0, time.time())
        except FileNotFoundError:
            not_installed_indentifiers += 1
        except Exception as e:
            write_log("ERROR", "Uninstall", "Run key could not be deleted : " + e, 1106, time.time())


        identifiers_count += 1
        try:
            key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                                APP_PATH_KEY + TITLE_APPENDED, 0, winreg.KEY_ALL_ACCESS)
            subkey_count = winreg.QueryInfoKey(key)[0]
            for i in range(subkey_count):
                subkey_name = winreg.EnumKey(key, 0)
                winreg.DeleteKey(key, subkey_name)
            winreg.DeleteKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY + TITLE_APPENDED)
            not_installed_indentifiers += 1
            write_log("INFO", "Uninstall", "App key deleted", 0, time.time())
        except FileNotFoundError:
            not_installed_indentifiers += 1
        except Exception as e:
            write_log("ERROR", "Uninstall", "App key could not be deleted : " + e, 1105, time.time())
        try:
            subprocess.Popen(['sc', 'stop', 'nexumservice'],shell=True)
            subprocess.Popen(['sc', 'delete', 'nexumservice'],shell=True)
        except:
            write_log("ERROR", "Uninstall", "Service could not be deleted", 0, time.time())

        identifiers_count += 1
        if os.path.exists(OS_FILE_PATH):

            try:
                subprocess.Popen(["taskkill", "/F", "/IM", EXE_WATCHDOG_NAME],shell=True)
                subprocess.call(["taskkill", "/F", "/IM", EXE_NEXUM_NAME],shell=True)
                subprocess.call(["taskkill", "/F", "/IM", EXE_SERV_WATCHDOG_NAME],shell=True)
                subprocess.call(["taskkill", "/F", "/IM", EXE_SERVER_NAME],shell=True)
                breakcount:int = 0
                time.sleep(1) # time to stop the processes
                shutil.rmtree(OS_FILE_PATH)
                while os.path.exists(OS_FILE_PATH):
                    time.sleep(1)
                    breakcount += 1
                    if breakcount > 5:
                        break
                    shutil.rmtree(OS_FILE_PATH)
                not_installed_indentifiers += 1
                write_log("INFO", "Uninstall", "Nexum folder deleted", 0, time.time())
            except Exception as e:
                write_log("ERROR", "Uninstall", "Nexum folder could not be deleted : " + e,
                        0, time.time())
        else:
            write_log("INFO", "Uninstall", "Nexum folder does not exist", 0, time.time())
            not_installed_indentifiers += 1
        # delete scheduled tasks nexum and nexserv
        del_count:int=0
        try:
            os.system('schtasks /delete /tn nexum /f')
        except:
            del_count += 1
        try:
            os.system('schtasks /delete /tn nexserv /f')
        except:
            del_count += 1
        if del_count == 2:
            write_log("Error", "Uninstall", "Scheduled tasks both deleted, should only be one", 0, time.time())
        elif del_count == 1:
            write_log("INFO", "Uninstall", "Scheduled task deleted", 0, time.time())
        else:
            write_log("Error", "Uninstall", "No tasks deleted", 0, time.time())

        identifiers_count += 1
        try:
            key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                        STARTUP_APPROVED_KEY,
                        0, winreg.KEY_ALL_ACCESS)
            winreg.DeleteValue(key,"Nexum")
            not_installed_indentifiers += 1
            write_log("INFO", "Uninstall", "Startup key deleted", 0, time.time())
        except FileNotFoundError:
            not_installed_indentifiers += 1
        except Exception as e:
            write_log("ERROR", "Uninstall", "Startup key could not be deleted : " + e, 1104, time.time())




        uninstall_percentage:float = (not_installed_indentifiers/identifiers_count) * 100
        write_log("INFO", "Uninstall", "Uninstall percentage: " + str(uninstall_percentage),
                0, time.time())

    
        try:
            subprocess.Popen(['schtasks', '/delete', '/tn', 'nexum', '/f'],shell=True)
        except:
            write_log("ERROR", "Uninstall", "Scheduled task nexum could not be deleted", 0, time.time())
        try:
            subprocess.Popen(['schtasks', '/delete', '/tn', 'nexserv', '/f'],shell=True)
        except:
            write_log("ERROR", "Uninstall", "Scheduled task nexserv could not be deleted", 0, time.time())
        #delete service

            # delete service nexum_service
        completed(window,"","uninstall completed")
    else:
        write_log("ERROR", "Uninstall", "Could not uninstall from server", 1115, time.time())
    # delete scheduled tasks nexum and nexserv



def uninstall(window:tk.Tk):
    """
    Information
    """
    write_log("INFO", "Uninstall", "Uninstall Started", 0, time.time())
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Uninstall")
    new_window.geometry(WINDOW_GEOMETRY)
    new_window.resizable(False, False)  # Set resizable to False
    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs
    image = Image.open(working_dir)
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(new_window, image=photo)
    image_label.pack(pady=50)
    lock_label = tk.Label(new_window, text="Lock Code:")
    lock_label.place(relx=0.40, rely=0.6, anchor=tk.CENTER)

    lock_entry = tk.Entry(new_window)
    lock_entry.place(relx=0.52, rely=0.6, anchor=tk.CENTER)

    uninstall_button = tk.Button(new_window, text="Uninstall", width=25,
                            height=3, command=lambda:uninstall_program(lock_entry.get(),new_window))
    uninstall_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID,
                            borderwidth=1, highlightthickness=0, highlightbackground="black",
                            highlightcolor="black", padx=10, pady=5, font=("Arial", 10),
                            overrelief=tk.RIDGE)
    uninstall_button.place(relx=0.75, rely=0.6, anchor=tk.CENTER)

    back_button = tk.Button(new_window, text="Back", width=25,
                            height=3, command=lambda: main_window(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID,
                          borderwidth=1, highlightthickness=0, highlightbackground="black",
                          highlightcolor="black", padx=10, pady=5, font=("Arial", 10),
                          overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.7, anchor=tk.CENTER)

    new_window.mainloop()
def install_nexserv_file(curl_route:str,apikey:str):
    """
    Information
    """
    payload = {
        }
    write_log("debug", "Install server", "Payload: " + str(payload), 0, time.time())
    write_log("debug", "Install server", "curl_route: " + str(curl_route), 0, time.time())
    write_log("debug", "Install server", "apikey: " + str(apikey), 0, time.time())
    request = requests.request("GET", f"{curl_route}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey}, json=payload, verify=SSL_CHECK)
    write_log("debug", "Install server", "request: " + str(request.content), 0, time.time())
    try:
        if not os.path.exists(OS_FILE_PATH): # double check path exists
            os.mkdir(OS_FILE_PATH)
    except:
        write_log("ERROR", "Install Server", "Could not create Nexum folder", 0, time.time())
    try:
        win_path = os.path.join("C:\\", "Program Files", "Nexum", "nexserv.exe")
        with open(win_path, "wb") as file:
            file.write(request.content)
        file.close()
    except Exception as e:
        write_log("ERROR", "Install Server", "Could not write nexserv.exe: " + str(e), 0, time.time())
    write_log("INFO", "Install Server", "nexserv.exe installed", 0, time.time())







def install_nexum_file(curl_route:str,apikey:str):
    """
    Information
    """
    payload = {
        }
    write_log("INFO", "Install nexum file", "Payload: " + str(payload), 0, time.time())
    request = requests.request("GET", f"{curl_route}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey}, json=payload, verify=SSL_CHECK)
    
    try:
        if not os.path.exists(OS_FILE_PATH): # double check path exists
            os.mkdir(OS_FILE_PATH)
    except Exception as e:
        write_log("ERROR", "Install nexum file", "Could not create Nexum folder: " + str(e), 0, time.time())
    try:
        win_path = os.path.join("C:\\", "Program Files", "Nexum", "nexum.exe")
        with open(win_path, "wb") as file:
            file.write(request.content)
        file.close()
    except Exception as e:
        write_log("ERROR", "Install nexum file", "Could not write nexum.exe: " + str(e), 0, time.time())
    write_log("INFO", "Install nexum file", "nexum.exe installed", 0, time.time())
def install_service(curl_route:str,apikey:str):
    """
    Information
    """
    payload = {
        }
    write_log("INFO", "Install server", "Payload: " + str(payload), 0, time.time())
    request = requests.request("GET", f"{curl_route}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey}, json=payload, verify=SSL_CHECK)
    
    try:
        if not os.path.exists(OS_FILE_PATH): # double check path exists
            os.mkdir(OS_FILE_PATH)
    except:
        write_log("ERROR", "Install Server", "Could not create Nexum folder", 0, time.time())

    try:
        win_path = os.path.join("C:\\", "Program Files", "Nexum", "nexumservice.exe")
        with open(win_path, "wb") as file:
            file.write(request.content)
        file.close()
    except Exception as e:
        write_log("ERROR", "Install Server", "Could not write nexumservice.exe: " + str(e), 0, time.time())
    write_log("INFO", "Install Service", "nexumservice.exe installed", 0, time.time())

    # install exe as service
    subprocess.Popen(['sc', 'create', 'nexumservice', 'binPath=' + str(win_path), 'start=', 'auto'],shell=True)
    # set 3 recovery options to restart at 60 seconds
    subprocess.Popen(['sc', 'failure', 'nexumservice', 'reset=', '60', 'actions=', 'restart/60000/restart/60000/restart/60000'],shell=True)
    #start the service
    subprocess.Popen(['sc', 'start', 'nexumservice'],shell=True)
    write_log("INFO", "Install Service", "nexumservice.exe created and started", 0, time.time())

def install_persistence(client_server:int):
    """
    Information
    """
    if client_server == 0:
        client_persistance()
        write_log("INFO", "Install Service", "Client Scheduled task setup", 0, time.time())
    else:
        server_persistance()
        write_log("INFO", "Install Service", "Server scheduled task setup", 0, time.time())

def notify_server(backupserver:str,id:int,apikey:str,uuid:str,key:str):
    """
    Information
    """
    try:
        
        payload = {
        "client_Id":id,
        "uuid":uuid,
        "installationKey":key
        }
        write_log("INFO", "Install server", "Payload: " + str(payload), 0, time.time())
        request = requests.request("POST", f"{HYPER_PROTOCOL}{backupserver}/{VERIFY_PATH}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey}, json=payload, verify=SSL_CHECK)
        write_log("INFO", "Install Server", "Server notified", 0, time.time())
        write_log("debug", "Install Server", "Server response: " + str(request.content), 0, time.time())
    except:
        write_log("ERROR", "Install Server", "Could not notify server", 1100, time.time())

def install_client_background(window:tk.Tk, backupserver:str, key:str,apikey:str):
    """
    Information
    """
    conn = sqlite3.connect(settingsDirectory+job_settingsFile)
    cursor = conn.cursor()
    cursor.execute('''SELECT MAX(id) FROM clients''')
    result = cursor.fetchone()[0]
    if result is not None:
        identification = int(result) + 1
    else:
         identification = 1
    conn.close()
    write_log("INFO", "Install Client", "Install Client process starting", 0, time.time())
    write_log("INFO", "Install Client", "Backup Server: " + backupserver, 0, time.time())
    write_log("INFO", "Install Client", "Key: " + key, 0, time.time())
    request = requests.Response()
    try:
        result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
                                capture_output=True, text=True,check=True,shell=True) # enc with uuid
        output = result.stdout.strip()
        output = output.split('\n\n', 1)[-1]
        output = output[:24]
        payload = {
            "name":socket.gethostname(),
            "uuid":output,
            "client_Id":identification,
            "ipaddress":socket.gethostbyname(socket.gethostname()),
            "port":PORT,
            "type":1,
            "macaddresses":[
                {
                "id":0,
                "address":':'.join(['{:02x}'.format((uuid.getnode() >> ele) & 0xff)
                            for ele in range(0,8*6,8)][::-1])
                }
            ],
            "installationKey":key
        }
        write_log("INFO", "Install server", "Payload: " + str(payload), 0, time.time())
        write_log("INFO", "Install server", "path: " + f"{"http://"}{backupserver}/{CLIENT_REGISTRATION_PATH}", 0, time.time())
        request = requests.request("GET", f"{"http://"}{backupserver}/{CLIENT_REGISTRATION_PATH}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey}, json=payload, verify=SSL_CHECK)
        write_log("INFO", "Install Client", request.status_code, 0, time.time())

        write_log("INFO", "Install Client", f"{"http://"}{backupserver}/{BEAT_PATH}",0, time.time())
        request = requests.request("POST", f"{"http://"}{backupserver}/{BEAT_PATH}", timeout=TIMEOUT,
            headers={"Content-Type": "application/json","secret":apikey,"id":str(identification)}, verify=SSL_CHECK)
        
        write_log("INFO", "Install Client", "Identification: " + str(identification), 0, time.time())
    except Exception as e:
        print("Could not connect to server")
        write_log("ERROR", "Install Client", "Could not connect to server "+str(e), 1100, time.time())
        return
    if request.status_code == 200:
        # Create a folder C:\Program Files\Nexum
        try:
            os.mkdir(OS_FILE_PATH)
            print("Nexum folder created")
        except:
            print("Nexum folder already exists or could not be created")
                # get urls
        try:
            payload = {
                
            }
            write_log("INFO", "Install client", "Payload: " + str(payload), 0, time.time())
            write_log("INFO", "Install client", "path: " + f"{"http://"}{backupserver}/urls", 0, time.time())
            request = requests.request("GET", f"{"http://"}{backupserver}/urls",
                            timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey}, json=payload, verify=SSL_CHECK)
            write_log("INFO", "Install Client", "Request: " + str(request.content), 0, time.time())
            write_log("INFO", "Install Client", "Request: " + str(request.json()), 0, time.time())
            request = request.json()
            service_url=request["nexumServiceUrl"]
            server_url=request["nexumUrl"]
            write_log("INFO","Install Server", f"service:{service_url} server:{server_url}",0,time.time())
        except Exception as e: 
            write_log("ERROR","Install Server", "Failed to get URLS " + str(e),"1009",time.time())
        # pass each url into each respective function
        install_nexum_file(server_url,apikey)
        install_persistence(0)
        install_service(service_url,apikey)
        # create keys in registry
        try:
            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\Nexum"
            run_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY)
            winreg.SetValueEx(run_key, TITLE, 0, winreg.REG_SZ, OS_FILE_PATH +
                               "\\" + EXE_NEXUM_NAME)
            winreg.CloseKey(run_key)
            write_log("INFO", "Install Client", "Run key added", 0, time.time())

            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Nexum"
            app_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY)
            nexum_key = winreg.CreateKey(app_key, EXE_NEXUM_NAME)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_SZ, OS_FILE_PATH + "\\" + EXE_NEXUM_NAME)
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Client", "App key added", 0, time.time())


            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run\Nexum"
            startup_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE,
                                        STARTUP_APPROVED_KEY)
            nexum_key = winreg.CreateKey(startup_key, EXE_NEXUM_NAME)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_BINARY, bytes.fromhex("02"))
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(startup_key)
            write_log("INFO", "Install Client", "Startup key added", 0, time.time())

            winreg.SetValueEx(nexum_key, "Path", 0, winreg.REG_SZ, r"'C:\Program Files\Nexum'")
            winreg.SetValueEx(nexum_key, "Enable", 0, winreg.REG_SZ, "1")  # Enable the startup app
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Client", "App key added", 0, time.time())

        except Exception as e:
            write_log("ERROR", "Install Client", "Error adding registry keys: "
                     + str(e), 1102, time.time())



        # notify server that the installation is complete
        notify_server(backupserver,identification,apikey,output,key)

        #incorrect secret
    else:
        write_log("ERROR", "Install Client", "Incorrect Secret", 1101, time.time())
    window.destroy()
    completed(tk.Tk(),"","client install completed")



def install_client(window:tk.Tk):
    """
    Information
    """
    write_log("INFO", "Install Client", "Install Client Started", 0, time.time())
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Nexum - install Client")
    new_window.geometry(WINDOW_GEOMETRY)
    new_window.resizable(False, False)  # Set resizable to False

    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs
    image = Image.open(working_dir)
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(new_window, image=photo)
    image_label.pack(pady=5)

    # Create the text fields and labels
    backup_label = tk.Label(new_window, text="Backup Server Address:")
    backup_label.pack(pady=(0, 10))
    backup_entry = tk.Entry(new_window)
    backup_entry.pack(pady=(0, 10))
    key_label = tk.Label(new_window, text="Install Key:")
    key_label.pack(pady=(0, 10))
    key_entry = tk.Entry(new_window)
    key_entry.pack(pady=(0, 10))

    secret_label = tk.Label(new_window, text="api key:")
    secret_label.pack(pady=(0, 10))
    secret_entry = tk.Entry(new_window)
    secret_entry.pack(pady=(0, 10))

    # Center the text fields and labels
    new_window.update()
    backup_label.place(relx=0.5, rely=0.4, anchor=tk.CENTER)
    backup_entry.place(relx=0.5, rely=0.45, anchor=tk.CENTER)
    key_label.place(relx=0.5, rely=0.6, anchor=tk.CENTER)
    key_entry.place(relx=0.5, rely=0.65, anchor=tk.CENTER)
    secret_label.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
    secret_entry.place(relx=0.5, rely=0.55, anchor=tk.CENTER)
    enter_button = tk.Button(new_window, text="Enter", width=25, height=3,
                              command=lambda:
                              install_client_background(new_window, backup_entry.get(),
                                                                    key_entry.get(),secret_entry.get()))
    enter_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                            highlightthickness=0, highlightbackground="black",
                            highlightcolor="black",padx=10, pady=5, font=("Arial", 10),
                            overrelief=tk.RIDGE)
    enter_button.place(relx=0.5, rely=0.8, anchor=tk.CENTER)
    back_button = tk.Button(new_window, text="Back", width=25, height=3,
                            command=lambda: main_window(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID,
                          borderwidth=1, highlightthickness=0, highlightbackground="black",
                          highlightcolor="black", padx=10, pady=5, font=("Arial", 10),
                          overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.95, anchor=tk.CENTER)
    new_window.mainloop()


def install_server_background(window:tk.Tk, backupserver:str, key:str,apikey:str):
    """
    Information
    """
    write_log("INFO", "Install Server", "Install Server process starting", 0, time.time())
    write_log("INFO", "Install Server", "Backup Server: " + backupserver, 0, time.time())
    write_log("INFO", "Install Server", "Key: " + key, 0, time.time())
    write_log("INFO", "Install Server", "api: " + apikey, 0, time.time())
    request = requests.Response()
    try:
        result = subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
                                capture_output=True, text=True,check=True,shell=True) # enc with uuid
        output = result.stdout.strip()
        output = output.split('\n\n', 1)[-1]
        output = output[:24]
        payload = {
            "name":socket.gethostname(),
            "client_id":0,
            "uuid":output,
            "ipaddress":socket.gethostbyname(socket.gethostname()),
            "port":PORT,
            "type":0,
            "macaddresses":[
                {
                "address":':'.join(['{:02x}'.format((uuid.getnode() >> ele) & 0xff)
                            for ele in range(0,8*6,8)][::-1])
                }
            ],
            "installationkey":key
        }
        write_log("INFO", "Install server", "Payload: " + str(payload), 0, time.time())
        write_log("INFO", "Install server", "path: " + f"{HYPER_PROTOCOL}{backupserver}/{REGISTRATION_PATH}", 0, time.time())
        request = requests.request("POST", f"{HYPER_PROTOCOL}{backupserver}/{REGISTRATION_PATH}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey}, json=payload, verify=SSL_CHECK)

        identification = str(request.json()["client_Id"])

        write_log("INFO", "Install server", "Identification: " + identification, 0, time.time())
    except Exception as e:
        write_log("ERROR", "Install Server", "Could not connect to server (" + e, 1100, time.time())
        window.destroy()
        completed(tk.Tk(),"","Server install failed")
    write_log("INFO", "Install Server", "Request status code: " + str(request.status_code), 0, time.time())
    if str(request.status_code) == str(200):
        # Create a folder C:\Program Files\Nexum
        try:
            os.mkdir(OS_FILE_PATH)
            write_log("INFO", "Install Server", "Nexum folder created", 0, time.time())
        except:
            write_log("INFO", "Install Server", "Nexum folder already exists or could not be created", 0, time.time())

        write_log("INFO", "Install Server", "Nexum Folder Created", 0, time.time())
        # get urls
        try:
            payload = {
                
            }
            write_log("INFO", "Install server", "Payload: " + str(payload), 0, time.time())
            request = requests.request("GET", f"{HYPER_PROTOCOL}{backupserver}/{URLS_ROUTE}",
                            timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey}, json=payload, verify=SSL_CHECK)
            request = request.json()
            service_url=request["nexumServiceUrl"]
            server_url=request["nexumServerUrl"]
            write_log("INFO","Install Server", f"service:{service_url} server:{server_url}",0,time.time())
        except:
            write_log("ERROR","Install Server", "Failed to get URLS","1009",time.time())
        # pass each url into each respective function
        install_nexserv_file(server_url,apikey)
        install_persistence(1)
        install_service(service_url,apikey)
        try:
            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\Nexum"
            run_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY)
            winreg.SetValueEx(run_key, TITLE, 0, winreg.REG_SZ, '"'+OS_FILE_PATH +
                               "\\" + EXE_SERVER_NAME+'"')
            winreg.CloseKey(run_key)
            write_log("INFO", "Install server", "Run key added", 0, time.time())

            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Nexum"
            app_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY)
            nexum_key = winreg.CreateKey(app_key, TITLE)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_SZ, (OS_FILE_PATH + "\\" + EXE_SERVER_NAME))
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Server", "App key added", 0, time.time())


            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run\Nexum"
            startup_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE,
                                        STARTUP_APPROVED_KEY)
            nexum_key = winreg.CreateKey(startup_key, TITLE)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_BINARY, bytes.fromhex("02"))

            write_log("INFO", "Install Server", "Startup key added", 0, time.time())

            winreg.SetValueEx(nexum_key, "Path", 0, winreg.REG_SZ, r"'C:\Program Files\Nexum'")
            winreg.SetValueEx(nexum_key, "Enable", 0, winreg.REG_SZ, "1")  # Enable the startup app
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Server", "App key added", 0, time.time())

        except Exception as e:
            write_log("ERROR", "Install Server", "Error adding registry keys: "
                     + str(e), 1102, time.time())

        # notify server that the installation is complete
        notify_server(backupserver,identification,apikey,output,key)

        #incorrect secret
    else:
        write_log("ERROR", "Install Client", "Incorrect Secret", 1101, time.time())
    window.destroy()
    completed(tk.Tk(),"","Server install completed")

def install_server(window:tk.Tk):
    """
    Information
    """
    write_log("INFO", "Install Server", "Install Server Started", 0, time.time())
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Nexum - Install Server")
    new_window.geometry(WINDOW_GEOMETRY)
    new_window.resizable(False, False)  # Set resizable to False

    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs
    image = Image.open(working_dir)
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(new_window, image=photo)
    image_label.pack(pady=5)

    # Create the text fields and labels
    backup_label = tk.Label(new_window, text="Data Manager Server Address:")
    backup_label.pack(pady=(0, 10))
    backup_entry = tk.Entry(new_window)
    backup_entry.pack(pady=(0, 10))

    key_label = tk.Label(new_window, text="Install Key:")
    key_label.pack(pady=(0, 10))
    key_entry = tk.Entry(new_window)
    key_entry.pack(pady=(0, 10))

    secret_label = tk.Label(new_window, text="ApiKey:")
    secret_label.pack(pady=(0, 10))
    secret_entry = tk.Entry(new_window)
    secret_entry.pack(pady=(0, 10))

    enter_button = tk.Button(new_window, text="Enter", width=25, height=3,
                              command=lambda:
                              install_server_background(new_window, backup_entry.get(),
                                                                    key_entry.get(),secret_entry.get()))
    enter_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                            highlightthickness=0, highlightbackground="black",
                            highlightcolor="black",padx=10, pady=5, font=("Arial", 10),
                            overrelief=tk.RIDGE,)
    enter_button.place(relx=0.5, rely=0.8, anchor=tk.CENTER)
    # Center the text fields and labels
    new_window.update()
    backup_label.place(relx=0.5, rely=0.4, anchor=tk.CENTER)
    backup_entry.place(relx=0.5, rely=0.45, anchor=tk.CENTER)
    
    key_label.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
    key_entry.place(relx=0.5, rely=0.55, anchor=tk.CENTER)

    secret_label.place(relx=0.5, rely=0.6, anchor=tk.CENTER)
    secret_entry.place(relx=0.5, rely=0.65, anchor=tk.CENTER)

    back_button = tk.Button(new_window, text="Back", width=25, height=3,
                            command=lambda: main_window(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                           highlightthickness=0, highlightbackground="black",
                           highlightcolor="black",padx=10, pady=5, font=("Arial", 10),
                           overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.95, anchor=tk.CENTER)
    new_window.mainloop()

def main_window(window:tk.Tk):
    """
    Information
    """
    write_log("INFO", "Main Window", "Main Window Opened", 0, time.time())
    # Create the main window
    window.destroy()
    window = tk.Tk()
    window.title("Nexum Installer")
    window.geometry(WINDOW_GEOMETRY)
    window.resizable(False, False)  # Set resizable to False

    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs
    image = Image.open(working_dir)
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(window, image=photo)
    image_label.pack(pady=50)

    # Create the buttons
    button1 = tk.Button(window, text="Install Server", width=25, height=3,
                        command=lambda: install_server(window))
    button1.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                    highlightthickness=0, highlightbackground="black", highlightcolor="black",
                     padx=10, pady=1, font=("Arial", 10), overrelief=tk.RIDGE)
    button1.pack(pady=10)
    button2 = tk.Button(window, text="Install Client",width=25,height=3,
                        command=lambda: install_client(window))
    button2.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                    highlightthickness=0, highlightbackground="black", highlightcolor="black",
                    padx=10, pady=1, font=("Arial", 10), overrelief=tk.RIDGE)
    button2.pack(pady=10)
    button3 = tk.Button(window, text="Uninstall",width=25,height=3,
                        command=lambda: uninstall(window))
    button3.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID,
                    borderwidth=1, highlightthickness=0, highlightbackground="black",
                    highlightcolor="black", padx=10, pady=1, font=("Arial", 10),
                    overrelief=tk.RIDGE)
    button3.pack(pady=10)
    button4 = tk.Button(window, text="Exit",width=25,height=3,command=lambda:sys.exit())
    button4.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                    highlightthickness=2, highlightbackground="black", highlightcolor="black",
                    padx=10, pady=1, font=("Arial", 10), overrelief=tk.RIDGE)
    button4.pack(pady=10)

    # Start the main loop
    window.mainloop()

def main():
    """
    Information
    """  
    if sys.argv[-1] != ASADMIN:
        script = os.path.abspath(sys.argv[0])
        params = ' '.join([script] + sys.argv[1:] + [ASADMIN])
        shell.ShellExecuteEx(lpVerb='runas', lpFile=sys.executable, lpParameters=params)
    else:
        t = tk.Tk()
        main_window(t)
    return 0

if __name__ == "__main__":
    main()

