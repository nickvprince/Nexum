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
# 1109 - Error reading setting
# 1110 - Decryption Failure
# 1111 - Error removing file
# 1112 - Error creating file
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
import sys
import base64
import tkinter as tk
import winreg
import shutil
import subprocess
import time
import socket
import uuid
import os
import uuid
import sqlite3
from PIL import ImageTk, Image
import requests
from requests import get
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import win32com.shell.shell as shell
from task import client_persistance, server_persistance
import tempfile

CURRENT_VERSION = "alpha 1.0.0"
ASADMIN = 'asadmin'
job_settingsFile=os.path.join('/settings.db')
VERIFY_PATH="api/DataLink/verify"
#pylint: disable= bare-except, broad-except

# FILES
EXE_SERVICE_NAME = "nexumservice.exe"
EXE_NEXUM_NAME = "Nexum.exe"
EXE_SERVER_NAME = "nexserv.exe"

# Paths
REGISTRATION_PATH = "api/DataLink/Register"
CLIENT_REGISTRATION_PATH = "check-installer"
URLS_ROUTE="api/DataLink/Urls"
URLS_ROUTE_LOCAL="urls"
OS_FILE_PATH = os.path.join("C:\\", "Program Files", "Nexum")
IMAGE_PATH = './Nexum.png'
BEAT_PATH = "beat"
UNINSTALL_PATH = "api/DataLink/Uninstall"
current_dir = os.path.dirname(os.path.abspath(__file__)) # working director
# setting should be in %temp%/settings/settings.db such as C:\Users\teche\AppData\Local\Temp\settings
SETTINGS_PATH = os.path.join(tempfile.gettempdir(),"/settings","/settings.db") # path to the settings database
logpath = os.path.join(current_dir,'../logs','/log.db') # path to the log database

# Keys
AUTO_RUN_KEY = r"Software\Microsoft\Windows\CurrentVersion\Run"
APP_PATH_KEY = r"Software\Microsoft\Windows\CurrentVersion\App Paths"
STARTUP_APPROVED_KEY = r"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run"

# other
TITLE = "Nexum"
SECRET = "wQ75NoTBd/KF5fTvUTViY4IoXhdWL+zrJD0+DUtg6BD4l3XdB25elvUkNEKbIImFuM4o7ZCKrocWXEju26eNPoUbHVcmCxXNJNh2VN8tcxafMVgGGmcoaCJNhDj2bvIA" # TO BE DELETED 
WINDOW_GEOMETRY = "1000x600"
IMAGE_GEOMETRY = (800,200)
ENCODING = "utf-8"

# Network
PORT = 5002
SERVER_PROTOCOL = "https://"
CLIENT_PROTOCOL = "http://"
TIMEOUT = 30 # timeout in seconds for flask requests
SSL_CHECK=False
@staticmethod
def get_next_server_id():
    """
    Get the next server id
    """
    conn = sqlite3.connect(str(tempfile.gettempdir())+str("\\settings\\settings.db"))
    cursor = conn.cursor()
    cursor.execute('''SELECT MAX(id) FROM backup_servers''')
    result = cursor.fetchone()[0]
    if result is not None:
        identification = int(result) + 1
    else:
        identification = 1
    conn.close()
    return identification
@staticmethod
def write_backup_server(name, path, username, password):
    """
    Write a backup server to the database
    """
    try:
        id = get_next_server_id()
        conn = sqlite3.connect(str(tempfile.gettempdir())+str("\\settings\\settings.db"))
        cursor = conn.cursor()
        cursor.execute('''SELECT path FROM backup_servers WHERE path = ?''', (path,))
        existing_path = cursor.fetchone()
        if existing_path:
            return "-403"
        cursor.execute('''INSERT INTO backup_servers (id, path, username, password, name)
                        VALUES (?, ?, ?, ?, ?)''',
                        (id, path, username, password, name))
        conn.commit()
        conn.close()
        return id
    except Exception as e:
            write_log("ERROR", "MySqlite", "Error writing backup server - "+str(e), 500, time.localtime())
            return None
@staticmethod
def get_time():
    """ 
    Get the current time in the format of "YYYY-MM-DDtHH:MM:SS:ms"
    """
    return time.strftime("%Y-%m-%dt%H:%M:%S:%m", time.localtime())


# decrypt a string using AES
@staticmethod
def decrypt_string(password, string):
    """
    Decrypt a string with AES-256 bit decryption
    """
    # Pad the password to be 16 bytes long
    password_hashed = str(password).ljust(16).encode(ENCODING)

    # Create a new AES cipher with the password as the key
    cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
    decryptor = cipher.decryptor()

    # Decode the string from base64
    decoded_string = base64.b64decode(string)

    # Decrypt the string using AES
    decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()
    try:
        return decrypted_string.decode(ENCODING)
    except UnicodeDecodeError:
        write_log("ERROR", "Decryption", "Decryption failed", 1110, get_time())
        return "Decryption failed"
    except:
        write_log("ERROR", "Decryption", "Decryption failed", 1110, get_time())
        return "Decryption failed"
@staticmethod
def encrypt_string(password, string):
    """
    Encrypt a string with AES-256 bit encryption
    """
    # Pad the password to be 16 bytes long
    password_hashed = str(password).ljust(16).encode('utf-8')

    # Create a new AES cipher with the password as the key
    cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
    encryptor = cipher.encryptor()

    # Pad the string to be a multiple of 16 bytes long
    string = string.ljust((len(string) // 16 + 1) * 16).encode('utf-8')

    # Encrypt the string using AES
    encrypted_string = encryptor.update(string) + encryptor.finalize()

    # Encode the encrypted string in base64
    encoded_string = base64.b64encode(encrypted_string)

    return encoded_string.decode('utf-8')

@staticmethod
def get_uuid():
    """
    Get the UUID of the computer
    """
    output=subprocess.run(['wmic', 'csproduct', 'get', 'uuid'],
                    capture_output=True, text=True,check=True,shell=True) # enc with uuid
    output = output.stdout.strip()
    output = output.split('\n\n', 1)[-1]
    output = output[:32]
    return output
@staticmethod
def write_setting(setting, value):
    """
    Write a setting to the database
    """
    result =  get_uuid()

    value = encrypt_string(result,value)
    conn = sqlite3.connect(SETTINGS_PATH)
    cursor = conn.cursor()
    cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
    existing_value = cursor.fetchone()
    if existing_value:
        cursor.execute('''UPDATE settings SET value = ? WHERE setting = ?''', (value, setting))
    else:
        cursor.execute('''INSERT INTO settings (setting, value) VALUES (?, ?)''',
                           (setting, value))
    conn.commit()
    conn.close()

@staticmethod
def read_setting(setting):
    """
    Read a setting from the database
    """
    try:
        conn = sqlite3.connect(SETTINGS_PATH)
        cursor = conn.cursor()
        cursor.execute('''SELECT value FROM settings WHERE setting = ?''', (setting,))
        value = cursor.fetchone()[0]
        conn.close()
        output = get_uuid()
        value = decrypt_string(output,value)
        return value.rstrip()
    except Exception as e:
        write_log("ERROR", "Read Setting", "Could not read setting: " + str(e), 1109, get_time())
        return ""

def write_log(severity:str, subject:str, message:str, code:str, date:str):
    """ 
    Write a log to the database
    """
    try:
        conn = sqlite3.connect(logpath)
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
    except Exception as e:
        print("Could not write log: " + str(e))
        return

def uninstall_from_server(backserver:str, key:str):
    """
    Uninstall the program from the server by reaching out and requesting an uninstall --Client
    """
    write_log("INFO", "Uninstall", "Uninstall from server started", 0, get_time())
    try:
        payload = {
            "clientid":read_setting("CLIENT_ID")
        }
        request = requests.request("GET", f"{CLIENT_PROTOCOL}{backserver}/{UNINSTALL_PATH}",
                    timeout=TIMEOUT, headers={"Content-Type": "application/json",
                    "key":key, "clientSecret":SECRET}, json=payload, verify=SSL_CHECK) # ONCE THIS SECRET IS CHANGED DELETE FROM GLOBALS ################################
        if request.status_code == 200:
            write_log("INFO", "Uninstall", "Uninstall from server successful", 0, get_time())
            return 200
        else:
            write_log("ERROR", "Uninstall", "Uninstall from server failed", 1115, get_time())
            return 400
    except:
        write_log("ERROR", "Uninstall", "Could not connect to server: ", 1100, get_time())
        return 400

def completed(window:tk.Tk,message:str,state:str):
    """
    The screen that is shown to the user when an action is completed
    """
    write_log("INFO", "Completed",
    "completed window opened - "+str(message) + " - " + str(state), 0, get_time())
    window.destroy()

    new_window = tk.Tk()
    new_window.title("Nexum - Completed")
    new_window.geometry(WINDOW_GEOMETRY)
    new_window.resizable(False, False)  # Set resizable to False

    image = Image.open(os.path.join(current_dir,IMAGE_PATH))
    image = image.resize((IMAGE_GEOMETRY))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(new_window, image=photo)
    image_label.pack(pady=50)

    message_label = tk.Label(new_window, text=message)
    message_label.place(relx=0.5, rely=0.6, anchor=tk.CENTER)

    state_label = tk.Label(new_window, text=state)
    state_label.place(relx=0.5, rely=0.8, anchor=tk.CENTER)

    back_button = tk.Button(new_window, text="Home", width=25,height=5, 
                            command=lambda: main_window(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                            highlightthickness=0, highlightbackground="black",
                            highlightcolor="black", padx=10, pady=5, font=("Arial", 10),
                            overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.9, anchor=tk.CENTER)
    new_window.mainloop()

def uninstall_from_device(window:tk.Tk):
    not_installed_indentifiers:int = 0 # increases as parts are removed
    identifiers_count:int =0 # total number of identifiers

    identifiers_count += 1
    try:
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY, 0, winreg.KEY_ALL_ACCESS)
        winreg.DeleteValue(key, TITLE)
        not_installed_indentifiers += 1

        write_log("INFO", "Uninstall", "Run key deleted", 0, get_time())
    except FileNotFoundError:

        not_installed_indentifiers += 1
    except Exception as e:
        completed(window,"","uninstall failed deleting run key")
        write_log("ERROR", "Uninstall", "Run key could not be deleted : " + str(e),
                    1106, get_time())
        
 # remove app key
    identifiers_count += 1
    try:
        title_preappend = "\\Nexum"
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                                APP_PATH_KEY + title_preappend, 0, winreg.KEY_ALL_ACCESS)
        subkey_count = winreg.QueryInfoKey(key)[0]
        for i in range(subkey_count):
            subkey_name = winreg.EnumKey(key, 0)
            winreg.DeleteKey(key, subkey_name)
        winreg.DeleteKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY + title_preappend)
        not_installed_indentifiers += 1

        write_log("INFO", "Uninstall", "App key deleted", 0, get_time())
    except FileNotFoundError:
        not_installed_indentifiers += 1

    except Exception as e:
        completed(window,"","uninstall failed deleting app key")
        write_log("ERROR", "Uninstall", "App key could not be deleted : " + e,
                    1105, get_time())
        
    identifiers_count += 1
    try:
        subprocess.Popen(['sc', 'stop', 'nexumservice'],shell=True)
        subprocess.Popen(['sc', 'delete', 'nexumservice'],shell=True)

        not_installed_indentifiers += 1
    except:

        write_log("ERROR", "Uninstall", "Service could not be deleted", 0, get_time())

    identifiers_count += 1
    if os.path.exists(OS_FILE_PATH):

        try:

            subprocess.Popen(["taskkill", "/F", "/IM", EXE_NEXUM_NAME],shell=True)
            
            subprocess.Popen(["taskkill", "/F", "/IM", EXE_SERVER_NAME],shell=True)

            breakcount:int = 0
            time.sleep(1) # time to stop the processes retry 5 times until timeout
            shutil.rmtree(OS_FILE_PATH)
            while os.path.exists(OS_FILE_PATH):

                time.sleep(1)
                breakcount += 1
                if breakcount > 5:
                    break
                shutil.rmtree(OS_FILE_PATH)

            not_installed_indentifiers += 1
            write_log("INFO", "Uninstall", "Nexum folder deleted", 0, get_time())
        except Exception as e:
            write_log("ERROR", "Uninstall", "Nexum folder could not be deleted : " + e,
                        1111, get_time())
        else:

            write_log("INFO", "Uninstall", "Nexum folder does not exist", 0, get_time())
            not_installed_indentifiers += 1
        
    
            # delete scheduled tasks nexum and nexserv
    identifiers_count += 1
    del_count:int=0
    try:
        os.system('schtasks /delete /tn nexum /f')

        del_count += 1
    except Exception as e:
        write_log("ERROR", "Uninstall", "Scheduled task nexum could not be deleted",000, get_time())
    try:
        os.system('schtasks /delete /tn nexserv /f')
        del_count += 1
    except Exception as e:
        write_log("ERROR", "Uninstall", "Scheduled task nexserv could not be deleted",000, get_time())

    if del_count == 2:
        write_log("Error", "Uninstall", "Scheduled tasks both deleted, should only be one",
                    0, get_time())
        not_installed_indentifiers += 1

    elif del_count == 1:
        write_log("INFO", "Uninstall", "Scheduled task deleted", 0, get_time())
        not_installed_indentifiers += 1
        not_installed_indentifiers += 1
    else:
        completed(window,"","uninstall failed deleting scheduled tasks")
        write_log("Error", "Uninstall", "No tasks deleted", 0, get_time())

    identifiers_count += 1
        # remove startup keys
    try:
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                        STARTUP_APPROVED_KEY,
                        0, winreg.KEY_ALL_ACCESS)
        winreg.DeleteValue(key,TITLE)
        not_installed_indentifiers += 1

        write_log("INFO", "Uninstall", "Startup key deleted", 0, get_time())
    except FileNotFoundError:

        not_installed_indentifiers += 1
    except Exception as e:
        write_log("ERROR", "Uninstall", "Startup key could not be deleted : " + e,
                    1104, get_time())

    uninstall_percentage:float = (not_installed_indentifiers/identifiers_count) * 100
    write_log("INFO", "Uninstall", "Uninstall percentage: " + str(uninstall_percentage),
                0, get_time())
    completed(window,"","uninstall completed")

def uninstall_client(key:str,window:tk.Tk):
    #reach out to local server for permission
    headers = {
        "Content-Type": "application/json",
        "apikey": read_setting("apikey")
    }
    content = {
        "key": key,
        "uuid": read_setting("uuid"),
        "client_id": read_setting("CLIENT_ID")
    }
    try:
        response = requests.post(f"{CLIENT_PROTOCOL}{read_setting('server_address')}:{read_setting('server_port')}/uninstall",
                                headers=headers, json=content, timeout=TIMEOUT, verify=SSL_CHECK)
        if response.status_code == 200:
            write_log("INFO", "Uninstall", "Uninstall request sent", 0, get_time())
            uninstall_from_device(window)

        else:
            write_log("ERROR", "Uninstall", "Uninstall request failed", 1115, get_time())
    except Exception as e:
        write_log("ERROR", "Uninstall", f"Could not connect to server {e}", 1100, get_time())
    completed(window,"","uninstall failed fatel error")
def uninstall_program(key:str,window:tk.Tk):
    """
    The main loop to uninstall the program including registry, server, files, and task
    """
    if read_setting("type") == "client":
        uninstall_client(key,window)
    else:
        headers = {
        "Content-Type": "application/json",
        "apikey": read_setting("apikey")
        }
        content = {
        "uninstallationKey": key,
        "uuid": read_setting("uuid"),
        "client_Id": read_setting("CLIENT_ID")
        }
        try:
            response = requests.post(f"{SERVER_PROTOCOL}{read_setting('msp_server_address')}:{read_setting('msp-port')}/{UNINSTALL_PATH}",
                                headers=headers, json=content, timeout=TIMEOUT, verify=SSL_CHECK)
            if response.status_code == 200:
                write_log("INFO", "Uninstall", "Uninstall request sent", 0, get_time())
                # post to msp to check install key


            else:
                write_log("ERROR", "Uninstall", "Uninstall request failed", 1115, get_time())
        except Exception as e:
            write_log("ERROR", "Uninstall", f"Could not connect to server {e}", 1100, get_time())
        uninstall_from_device(window)




def uninstall(window:tk.Tk):
    """
    Window for uninstalling the program
    """
    write_log("INFO", "Uninstall", "Uninstall Started", 0, get_time())
    window.destroy()

    new_window = tk.Tk()
    new_window.title("Uninstall")
    new_window.geometry(WINDOW_GEOMETRY)
    new_window.resizable(False, False)  # Set resizable to False


    # image
    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs
    image = Image.open(working_dir)
    image = image.resize(IMAGE_GEOMETRY)  # Adjust the size of the image as needed
    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(new_window, image=photo)
    image_label.pack(pady=50)

    # data
    lock_label = tk.Label(new_window, text="Lock Code:")
    lock_label.place(relx=0.40, rely=0.6, anchor=tk.CENTER)

    lock_entry = tk.Entry(new_window)
    lock_entry.place(relx=0.52, rely=0.6, anchor=tk.CENTER)

    # Buttons
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
    Installs the file from the server
    """

    write_log("INFO", "Install server", "Installing nexserv.exe from server starting",
            0, get_time())

    request = requests.request("GET", f"{SERVER_PROTOCOL}{curl_route}",
            timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
            verify=SSL_CHECK)

    try:
        if not os.path.exists(OS_FILE_PATH): # double check path exists
            os.mkdir(OS_FILE_PATH)
    except:
        write_log("ERROR", "Install Server", "Could not create Nexum folder", 1112, get_time())
    try:
        win_path = os.path.join(OS_FILE_PATH, EXE_SERVER_NAME)
        with open(win_path, "wb") as file:
            file.write(request.content)
        file.close()
        write_log("INFO", "Install Server", "nexserv.exe installed", 0, get_time())
    except Exception as e:
        write_log("ERROR", "Install Server", "Could not write nexserv.exe: " + str(e),
                1112, get_time())

def install_nexum_file(curl_route:str,apikey:str):
    """
    Installs the nexum.exe file from the server
    """

    write_log("INFO", "Install", "Install nexum.exe starting ",
            0, get_time())

    request = requests.request("GET", f"{curl_route}",
            timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
            verify=SSL_CHECK)

    try:
        if not os.path.exists(OS_FILE_PATH): # double check path exists
            os.mkdir(OS_FILE_PATH)
    except Exception as e:
        write_log("ERROR", "Install nexum file", "Could not create Nexum folder: " + str(e),
                1111, get_time())

    try:
        win_path = os.path.join(OS_FILE_PATH , EXE_NEXUM_NAME)
        with open(win_path, "wb") as file:
            file.write(request.content)
        file.close()
        write_log("INFO", "Install nexum file", "nexum.exe installed", 0, get_time())
    except Exception as e:
        write_log("ERROR", "Install nexum file", "Could not write nexum.exe: " + str(e),
                1111, get_time())

def install_service(curl_route:str,apikey:str):
    """
    installs nexum service from the server
    """
    write_log("INFO", "Install Service", "Installing nexumservice.exe from server starting",
            0,get_time())


    request = requests.request("GET", f"{curl_route}",
                timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
                verify=SSL_CHECK)

    try:
        if not os.path.exists(OS_FILE_PATH): # double check path exists
            os.mkdir(OS_FILE_PATH)
    except:
        write_log("ERROR", "Install Server", "Could not create Nexum folder", 1111, get_time())

    try:
        win_path = os.path.join(OS_FILE_PATH,EXE_SERVICE_NAME)
        with open(win_path, "wb") as file:
            file.write(request.content)
        file.close()
        write_log("INFO", "Install Service", "nexumservice.exe installed", 0, get_time())
    except Exception as e:
        write_log("ERROR", "Install Server", "Could not write nexumservice.exe: " + str(e),
                1111, get_time())


    # install exe as service
    subprocess.Popen(['sc', 'create', 'nexumservice', 'binPath=' + str(win_path),
                    'start=', 'auto'],shell=True)
    # set 3 recovery options to restart at 60 seconds
    subprocess.Popen(['sc', 'failure', 'nexumservice', 'reset=', '60', 'actions=',
                    'restart/60000/restart/60000/restart/60000'],shell=True)
    #start the service
    subprocess.Popen(['sc', 'start', 'nexumservice'],shell=True)
    write_log("INFO", "Install Service", "nexumservice.exe created and started", 0, get_time())

def install_persistence(client_server:int):
    """
    Installs XML file to the scheduled tasks
    """
    if client_server == 0: # client
        client_persistance()
    else: # server
        server_persistance()

def notify_server(backupserver:str,id:int,apikey:str,uuid:str,key:str):
    """
    Notify the server that the installation is complete
    """
    write_log("INFO", "Install Server", "Notifying server", 0, get_time())
    try:
        payload = {
        "client_Id":id,
        "uuid":uuid,
        "installationKey":key
        }

        _ = requests.request("POST", f"{SERVER_PROTOCOL}{backupserver}/verify", 
                timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
                json=payload, verify=SSL_CHECK)

        write_log("INFO", "Install Server", "Server notified", 0, get_time())

    except:
        write_log("ERROR", "Install Server", "Could not notify server", 1100, get_time())  

def install_client_background(window:tk.Tk, backupserver:str, key:str,apikey:str):
    """
    The background process for installing the client on the server, 
    including the registry keys and the service, and the persistence
    """
    write_log("INFO", "Install Client", "Install Client Background Started", 0, get_time())
    server_address = backupserver.split(":")[0]
    server_port = backupserver.split(":")[1]
    write_setting("server_address",server_address)
    write_setting("server_port",server_port)
    write_setting("service_address","127.0.0.1:5004")
    write_setting("Status","Installing")
    write_setting("apikey",apikey)
    write_setting("version","1.0.0")
    write_setting("msp-port","7101")
    write_setting("POLLING_INTERVAL","10")
    write_setting("uuid",get_uuid())
    write_setting("versiontag","alpha")
    write_setting("job_status","NotStarted")
    write_setting("type","client")
    try:
        # get uuid
        output = get_uuid()

        # send information to local server
        payload = {
            "name":socket.gethostname(),
            "uuid":output[0:31],
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


        # initial registration request
        request = requests.request("GET",
                f"{CLIENT_PROTOCOL}{backupserver}/{CLIENT_REGISTRATION_PATH}",
                timeout=120, headers={"Content-Type": "application/json","apikey":apikey},
                json=payload, verify=SSL_CHECK)

        identification = request.headers.get('clientid')
        msp_api = request.headers.get('msp_api')
        write_setting("CLIENT_ID",identification)
        write_setting("msp_api",msp_api)


    except Exception as e:
        write_log("ERROR", "Install Client", "Could not connect to server "+str(e),
                1100, get_time())
        completed(window,"","client install failed - 1100")

    if request.status_code == 200:
        write_log("INFO", "Install Client",
                "Identification: " + str(identification) + " Client installed on the server",
                0, get_time())

        # Create a folder C:\Program Files\Nexum
        try:
            os.mkdir(OS_FILE_PATH)

        except:
            write_log("ERROR", "Install Client", "Could not create Nexum folder", 1111, get_time())


        # get urls
        try:
            request = requests.request("GET", f"{CLIENT_PROTOCOL}{backupserver}/{URLS_ROUTE_LOCAL}",
                    timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
                    verify=SSL_CHECK)

            request = request.json()
            service_url=CLIENT_PROTOCOL+backupserver+"/nexumservice"
            server_url=CLIENT_PROTOCOL+backupserver+"/nexum"
            portal_url=str("https://")+request["portalUrlLocal"]
            write_setting("TENANT_PORTAL_URL",portal_url)
            write_log("INFO","Install Server", f"service:{service_url} server:{server_url}",
                    0,get_time())

        except Exception as e:
            write_log("ERROR","Install Server", "Failed to get URLS " + str(e),"1009",get_time())


        # pass each url into each respective function to install files and
        # setup the service and persistence
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
            write_log("INFO", "Install Client", "Run key added", 0, get_time())

        # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\
        # CurrentVersion\App Paths\Nexum"
            app_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY)
            nexum_key = winreg.CreateKey(app_key, EXE_NEXUM_NAME)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_SZ, OS_FILE_PATH + "\\" + EXE_NEXUM_NAME)
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Client", "App key added", 0, get_time())


            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\
            # CurrentVersion\Explorer\StartupApproved\Run\Nexum"
            startup_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE,
                                        STARTUP_APPROVED_KEY)
            nexum_key = winreg.CreateKey(startup_key, EXE_NEXUM_NAME)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_BINARY, bytes.fromhex("02"))
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(startup_key)
            write_log("INFO", "Install Client", "Startup key added", 0, get_time())

            # Add key app path key
            winreg.SetValueEx(nexum_key, "Path", 0, winreg.REG_SZ, OS_FILE_PATH)
            winreg.SetValueEx(nexum_key, "Enable", 0, winreg.REG_SZ, "1")  # Enable the startup app
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Client", "App key added", 0, get_time())

        except Exception as e:
            write_log("ERROR", "Install Client", "Error adding registry keys: "
                     + str(e), 1102, get_time())



        # notify server that the installation is complete
        notify_server(backupserver,identification,apikey,output,key)

        # start the program
        subprocess.Popen([OS_FILE_PATH + "\\" + EXE_NEXUM_NAME],shell=True)
    #incorrect secret
    else:
        write_log("ERROR", "Install Client", "Incorrect Secret or key", 1101, get_time())
        window.destroy()
        completed(tk.Tk(),"","client install failed - 1101")
    window.destroy()
    completed(tk.Tk(),"","client install completed")

def install_client(window:tk.Tk):
    """
    Install client window
    """
    write_log("INFO", "Install Client", "Install Client Started", 0, get_time())
    window.destroy()

    new_window = tk.Tk()
    new_window.title("Nexum - install Client")
    new_window.geometry(WINDOW_GEOMETRY)
    new_window.resizable(False, False)  # Set resizable to False

    # image
    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs
    image = Image.open(working_dir)
    image = image.resize(IMAGE_GEOMETRY)  # Adjust the size of the image as needed
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

    # Create the buttons
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
    Main loop for installing the server in the backend
    """
    write_log("INFO", "Install Server", "Install Server process starting", 0, get_time())
    # split backupserver as 127.0.0.1:5000 as [127.0.0.1,5000]
    server_address = backupserver.split(":")[0]
    server_port = backupserver.split(":")[1]
    write_setting("CLIENT_ID",str(0))
    write_setting("msp_server_address",server_address)
    write_setting("server_port",server_port)
    write_setting("service_address","127.0.0.1:5004")
    write_setting("Status","Installing")
    write_setting("apikey",apikey)
    write_setting("version","1.0.0")
    write_setting("msp-port","7101")
    write_setting("POLLING_INTERVAL","10")
    write_setting("uuid",get_uuid())
    write_setting("versiontag","alpha")
    write_setting("job_status","NotStarted")
    write_setting("type","server")
    # create GUID
    msp_api = uuid.uuid4()
    ip = get('https://api.ipify.org',timeout=10).content.decode('utf8')
    ip = server_address # Use this for demo since its locally built
    write_setting("msp_api",str(msp_api))
    try:

        output = get_uuid()
        payload = {
            "name":socket.gethostname(),
            "client_id":0,
            "uuid":output,
            "apibaseurl":f"{CLIENT_PROTOCOL}{ip}",
            "apibaseport":PORT,
            "ipaddress":socket.gethostbyname(socket.gethostname()),
            "port":PORT,
            "type":0,
            "apikey":str(msp_api),
            "macaddresses":[
                {
                "address":':'.join(['{:02x}'.format((uuid.getnode() >> ele) & 0xff)
                            for ele in range(0,8*6,8)][::-1])
                }
            ],
            "installationkey":key
        }

        # initial registration request
        request = requests.request("POST", f"{SERVER_PROTOCOL}{backupserver}/{REGISTRATION_PATH}",
                timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
                json=payload, verify=SSL_CHECK)

        write_log("INFO", "Install server", "Identification: " + str(0), 0, get_time())

    except Exception as e:
        write_log("ERROR", "Install Server", "Could not connect to server (" + e, 1100, get_time())
        window.destroy()
        completed(tk.Tk(),"","Server install failed - 1100")

    if str(request.status_code) == str(200): # server accepted the request
        # Create a folder C:\Program Files\Nexum
        try:
            os.mkdir(OS_FILE_PATH)
            write_log("INFO", "Install Server", "Nexum folder created", 0, get_time())
        except:
            write_log("INFO", "Install Server",
            "Nexum folder already exists or could not be created",1111, get_time())
        # get urls
        try:

            request = requests.request("GET", f"{SERVER_PROTOCOL}{backupserver}/{URLS_ROUTE}",
                    timeout=TIMEOUT, headers={"Content-Type": "application/json","apikey":apikey},
                    verify=SSL_CHECK)
            request = request.json()
            service_url=str("https://")+request["nexumServiceUrlLocal"]
            server_url=request["nexumServerUrlLocal"]
            portal_url=str("https://")+request["portalUrlLocal"]
            write_setting("TENANT_PORTAL_URL",portal_url)
            write_log("INFO","Install Server", f"service:{service_url} server:{server_url}",
                    0,get_time())
        except:
            write_log("ERROR","Install Server", "Failed to get URLS","1009",get_time())

        # pass each url into each respective function to install files and
        # setup the service and persistence

        install_nexserv_file(server_url,apikey)
        install_persistence(1)
        install_service(service_url,apikey)


        try:
         # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\Nexum"
            run_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY)
            winreg.SetValueEx(run_key, TITLE, 0, winreg.REG_SZ, '"'+OS_FILE_PATH +
                               "\\" + EXE_SERVER_NAME+'"')
            winreg.CloseKey(run_key)
            write_log("INFO", "Install server", "Run key added", 0, get_time())

        # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\
        # Windows\CurrentVersion\App Paths\Nexum"
            app_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY)
            nexum_key = winreg.CreateKey(app_key, TITLE)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_SZ, (OS_FILE_PATH + "\\" + EXE_SERVER_NAME))
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Server", "App key added", 0, get_time())


        # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\
        # CurrentVersion\Explorer\StartupApproved\Run\Nexum"
            startup_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE,
                                        STARTUP_APPROVED_KEY)
            nexum_key = winreg.CreateKey(startup_key, TITLE)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_BINARY, bytes.fromhex("02"))

            write_log("INFO", "Install Server", "Startup key added", 0, get_time())

            winreg.SetValueEx(nexum_key, "Path", 0, winreg.REG_SZ, OS_FILE_PATH)
            winreg.SetValueEx(nexum_key, "Enable", 0, winreg.REG_SZ, "1")  # Enable the startup app
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Server", "App key added", 0, get_time())

        except Exception as e:
            write_log("ERROR", "Install Server", "Error adding registry keys: "
                     + str(e), 1102, get_time())

        # notify server that the installation is complete
        notify_server(backupserver,0,apikey,output,key)

        # start the program
        subprocess.Popen([OS_FILE_PATH + "\\" + EXE_SERVER_NAME],shell=True)

        #incorrect secret
    else:
        write_log("ERROR", "Install Client", "Incorrect Secret", 1101, get_time())

    window.destroy()
    completed(tk.Tk(),"","Server install completed")

def install_server(window:tk.Tk):
    """
    Install server window
    """
    write_log("INFO", "Install Server", "Install Server Started", 0, get_time())
    window.destroy()

    new_window = tk.Tk()
    new_window.title("Nexum - Install Server")
    new_window.geometry(WINDOW_GEOMETRY)
    new_window.resizable(False, False)  # Set resizable to False

    # image
    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs
    image = Image.open(working_dir)
    image = image.resize((IMAGE_GEOMETRY))  # Adjust the size of the image as needed
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

    # Create the buttons
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
    First window in the installer
    """
    write_log("INFO", "Main Window", "Main Window Opened", 0, get_time())


    # Create the main window
    window.destroy()
    window = tk.Tk()
    window.title("Nexum Installer")
    window.geometry(WINDOW_GEOMETRY)
    window.resizable(False, False)  # Set resizable to False

    working_dir = os.path.join(current_dir,IMAGE_PATH) # directory for logs

    # Create the image
    image = Image.open(working_dir)
    image = image.resize(IMAGE_GEOMETRY)  # Adjust the size of the image as needed
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
    Main Loop
    """
    # INITIALIZATION
    global SETTINGS_PATH
    global logpath
    logpath = str(tempfile.gettempdir())+str("\\logs\\logs.db")
    if not os.path.exists(str(tempfile.gettempdir())+str("\\settings")):
        os.mkdir(str(tempfile.gettempdir())+str("\\settings"))

    if not os.path.exists(str(tempfile.gettempdir())+str("\\logs")):
        os.mkdir(str(tempfile.gettempdir())+str("\\logs"))


    SETTINGS_PATH = str(tempfile.gettempdir())+str("\\settings\\settings.db")

    # create settings table
    conn = sqlite3.connect(SETTINGS_PATH)
    cursor = conn.cursor()
    cursor.execute('''CREATE TABLE IF NOT EXISTS settings
                            (setting TEXT, value TEXT)''')
        # Close connection
    conn.commit()
    conn.close()

    # create logs table

    conn = sqlite3.connect(str(tempfile.gettempdir())+str("\\logs\\logs.db"))
    cursor = conn.cursor()
    cursor.execute('''CREATE TABLE IF NOT EXISTS logs
            (id TEXT, severity TEXT, subject TEXT, message TEXT, code TEXT, date TEXT)''')
        # Close connection
    conn.commit()
    conn.close()

    # create backupserver tables
    try:
            # create db file settingsDirectory\\settings
        conn = sqlite3.connect(str(tempfile.gettempdir())+str("\\settings\\settings.db"))
        cursor = conn.cursor()
        # creat table for backup servers : id,smb path,user,pass,name
        cursor.execute('''CREATE TABLE IF NOT EXISTS backup_servers
                            (id TEXT, path TEXT, username TEXT, password TEXT, name TEXT)''')
        write_log("INFO", "MySqlite", "backup server table created", 200, time.localtime())
    except:
        write_log("ERROR", "MySqlite", "Backup servers table not created", 500, time.localtime())
    # INITIALIZATION END


    #TESTING Area
    main_window(tk.Tk())

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
