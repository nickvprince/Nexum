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

"""


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

#pylint: disable= bare-except, broad-except
REGISTRATION_PATH = "check-installer"
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
HYPER_PROTOCOL = "http://"
IMAGE_PATH = '../Data/Nexum.png'
TIMEOUT = 5
BEAT_PATH = "beat"

# pah directories
current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
logdirectory = os.path.join(current_dir,'../logs') # directory for logs
logpath = os.path.join('/log.db') # path to the log database


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

def uninstall_program():
    """
    Information
    """
    not_installed_indentifiers:int = 0
    identifiers_count:int =0

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

    identifiers_count += 1
    if os.path.exists(OS_FILE_PATH):

        try:
            subprocess.call(["taskkill", "/F", "/IM", EXE_WATCHDOG_NAME])
            subprocess.call(["taskkill", "/F", "/IM", EXE_NEXUM_NAME])
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
                            height=3, command=uninstall_program)
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

def install_nexum_file():
    """
    Information
    """
    # call to server to get install location and CURL to c:\Program Files\Nexum
    # OR
    # copy ./Nexum.exe to C:\Program Files\Nexum
    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    path = os.path.join(current_dir,EXE_NEXUM_NAME) # directory for logs
    shutil.copy(path, OS_FILE_PATH+"/"+EXE_NEXUM_NAME)
    write_log("INFO", "Install Nexum", "Nexum file installed", 0, time.time())

def install_watchdog_file():
    """
    Information
    """
        # call to server to get install location and CURL to c:\Program Files\Nexum
        # OR
        # copy ./watchdog.exe to C:\Program Files\Nexum
    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    path = os.path.join(current_dir,EXE_WATCHDOG_NAME) # directory for logs
    shutil.copy(path, OS_FILE_PATH+"/"+EXE_WATCHDOG_NAME)
    write_log("INFO", "Install Watchdog", "Watchdog file installed", 0, time.time())

def notify_server():
    """
    Information
    """
    # call to server to notify that the installation is complete
    write_log("INFO", "Notify Server", "Server notified", 0, time.time())

def install_client_background(window:tk.Tk, backupserver:str, key:str):
    """
    Information
    """
    write_log("INFO", "Install Client", "Install Client process starting", 0, time.time())
    write_log("INFO", "Install Client", "Backup Server: " + backupserver, 0, time.time())
    write_log("INFO", "Install Client", "Key: " + key, 0, time.time())
    request = requests.Response()
    try:

        payload = {
            "name":socket.gethostname(),
            "ip":socket.gethostbyname(socket.gethostname()),
            "port":PORT,
            "mac":':'.join(['{:02x}'.format((uuid.getnode() >> ele) & 0xff)
                            for ele in range(0,8*6,8)][::-1])
        }
        write_log("INFO", "Install Client", "Payload: " + str(payload), 0, time.time())
        request = requests.request("GET", f"{HYPER_PROTOCOL}{backupserver}/{REGISTRATION_PATH}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","key":key,
                                            "clientSecret":SECRET}, json=payload)

        identification = request.headers["clientid"]

        request = requests.request("POST", f"{HYPER_PROTOCOL}{backupserver}/{BEAT_PATH}", timeout=TIMEOUT,
            headers={"Content-Type": "application/json","clientSecret":SECRET,"id":identification})
        write_log("INFO", "Install Client", "Identification: " + identification, 0, time.time())
    except:
        print("Could not connect to server")
        write_log("ERROR", "Install Client", "Could not connect to server", 1100, time.time())
        return
    if request.status_code == 200:
        # Create a folder C:\Program Files\Nexum
        try:
            os.mkdir(OS_FILE_PATH)
            print("Nexum folder created")
        except:
            print("Nexum folder already exists or could not be created")

        install_nexum_file()
        install_watchdog_file()
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
            nexum_key = winreg.CreateKey(app_key, TITLE)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_SZ, OS_FILE_PATH + "\\" + EXE_NEXUM_NAME)
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Client", "App key added", 0, time.time())


            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run\Nexum"
            startup_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE,
                                        STARTUP_APPROVED_KEY)
            nexum_key = winreg.CreateKey(startup_key, TITLE)
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

        # run c:\Program Files\Nexum\Nexum.exe
        # run c:\Program Files\Nexum\watchdog.exe
        # Run Nexum.exe
        nexum_path = OS_FILE_PATH + "\\" + EXE_NEXUM_NAME
        subprocess.Popen(nexum_path, shell=True)
        write_log("INFO", "Install Client", "Nexum.exe ran", 0, time.time())

        # Run watchdog.exe
        watchdog_path = OS_FILE_PATH + "\\" + EXE_WATCHDOG_NAME
        subprocess.Popen(watchdog_path, shell=True)
        write_log("INFO", "Install Client", "Watchdog.exe ran", 0, time.time())

        # notify server that the installation is complete
        notify_server()

        #incorrect secret
    else:
        write_log("ERROR", "Install Client", "Incorrect Secret", 1101, time.time())
    window.destroy()
    main_window(tk.Tk())



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

    secret_label = tk.Label(new_window, text="Client Secret:")
    secret_label.pack(pady=(0, 10))
    secret_entry = tk.Entry(new_window)
    secret_entry.pack(pady=(0, 10))

    # Center the text fields and labels
    new_window.update()
    backup_label.place(relx=0.5, rely=0.4, anchor=tk.CENTER)
    backup_entry.place(relx=0.5, rely=0.45, anchor=tk.CENTER)
    secret_label.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
    secret_entry.place(relx=0.5, rely=0.55, anchor=tk.CENTER)
    enter_button = tk.Button(new_window, text="Enter", width=25, height=3,
                              command=lambda:
                              install_client_background(new_window, backup_entry.get(),
                                                                    secret_entry.get()))
    enter_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                            highlightthickness=0, highlightbackground="black",
                            highlightcolor="black",padx=10, pady=5, font=("Arial", 10),
                            overrelief=tk.RIDGE)
    enter_button.place(relx=0.5, rely=0.7, anchor=tk.CENTER)
    back_button = tk.Button(new_window, text="Back", width=25, height=3,
                            command=lambda: main_window(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID,
                          borderwidth=1, highlightthickness=0, highlightbackground="black",
                          highlightcolor="black", padx=10, pady=5, font=("Arial", 10),
                          overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.9, anchor=tk.CENTER)
    new_window.mainloop()

def notify_msp():
    """
    Information 
    """

def install_server_background(window:tk.Tk, backupserver:str, key:str):
    """
    Information
    """
    write_log("INFO", "Install Server", "Install Server process starting", 0, time.time())
    write_log("INFO", "Install Server", "Backup Server: " + backupserver, 0, time.time())
    write_log("INFO", "Install Server", "Key: " + key, 0, time.time())
    request = requests.Response()
    try:
        payload = {
            "name":socket.gethostname(),
            "ip":socket.gethostbyname(socket.gethostname()),
            "port":PORT,
            "mac":':'.join(['{:02x}'.format((uuid.getnode() >> ele) & 0xff)
                            for ele in range(0,8*6,8)][::-1])
        }
        write_log("INFO", "Install server", "Payload: " + str(payload), 0, time.time())
        request = requests.request("GET", f"{HYPER_PROTOCOL}{backupserver}/{REGISTRATION_PATH}",
                        timeout=TIMEOUT, headers={"Content-Type": "application/json","key":key,
                                            "clientSecret":SECRET}, json=payload)

        identification = request.headers["clientid"]

        request = requests.request("POST", f"{HYPER_PROTOCOL}{backupserver}/{BEAT_PATH}", timeout=TIMEOUT,
            headers={"Content-Type": "application/json","clientSecret":SECRET,"id":identification})
        write_log("INFO", "Install Client", "Identification: " + identification, 0, time.time())
    except:
        write_log("ERROR", "Install Server", "Could not connect to server", 1100, time.time())
        return
    if request.status_code == 200:
        # Create a folder C:\Program Files\Nexum
        try:
            os.mkdir(OS_FILE_PATH)
            write_log("INFO", "Install Server", "Nexum folder created", 0, time.time())
        except:
            write_log("INFO", "Install Server", "Nexum folder already exists or could not be created", 0, time.time())

        # call to server to get install location and CURL to c:\Program Files\Nexum
        # OR
        # copy ./nexserv.exe to C:\Program Files\Nexum
        current_dir = os.path.dirname(os.path.abspath(__file__))
        path = os.path.join(current_dir,EXE_SERVER_NAME)
        shutil.copy(path, OS_FILE_PATH+"/"+EXE_SERVER_NAME)
        write_log("INFO", "Install Server", "nexserv.exe installed", 0, time.time())
        shutil.copy(path, OS_FILE_PATH+"/"+EXE_SERV_WATCHDOG_NAME)
        try:
            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\Nexum"
            run_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY)
            winreg.SetValueEx(run_key, TITLE, 0, winreg.REG_SZ, OS_FILE_PATH +
                               "\\" + EXE_SERVER_NAME)
            winreg.CloseKey(run_key)
            write_log("INFO", "Install server", "Run key added", 0, time.time())

            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Nexum"
            app_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY)
            nexum_key = winreg.CreateKey(app_key, TITLE)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_SZ, OS_FILE_PATH + "\\" + EXE_SERVER_NAME)
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Server", "App key added", 0, time.time())


            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run\Nexum"
            startup_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE,
                                        STARTUP_APPROVED_KEY)
            nexum_key = winreg.CreateKey(startup_key, TITLE)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_BINARY, bytes.fromhex("02"))
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(startup_key)
            write_log("INFO", "Install Server", "Startup key added", 0, time.time())

            winreg.SetValueEx(nexum_key, "Path", 0, winreg.REG_SZ, r"'C:\Program Files\Nexum'")
            winreg.SetValueEx(nexum_key, "Enable", 0, winreg.REG_SZ, "1")  # Enable the startup app
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            write_log("INFO", "Install Server", "App key added", 0, time.time())

        except Exception as e:
            write_log("ERROR", "Install Server", "Error adding registry keys: "
                     + str(e), 1102, time.time())

        # run c:\Program Files\Nexum\Nexum.exe
        # run c:\Program Files\Nexum\watchdog.exe
        # Run Nexum.exe
        nexum_path = OS_FILE_PATH + "\\" + EXE_SERVER_NAME
        subprocess.Popen(nexum_path, shell=True)
        write_log("INFO", "Install SERVER", "nexserv.exe ran", 0, time.time())

        # Run watchdog.exe
        watchdog_path = OS_FILE_PATH + "\\" + EXE_SERV_WATCHDOG_NAME
        subprocess.Popen(watchdog_path, shell=True)
        write_log("INFO", "Install Client", "Watchdog_serv.exe ran", 0, time.time())

        # notify server that the installation is complete
        notify_msp()

        #incorrect secret
    else:
        write_log("ERROR", "Install Client", "Incorrect Secret", 1101, time.time())
    window.destroy()
    main_window(tk.Tk())

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

    secret_label = tk.Label(new_window, text="Tenant Secret:")
    secret_label.pack(pady=(0, 10))
    secret_entry = tk.Entry(new_window)
    secret_entry.pack(pady=(0, 10))

    enter_button = tk.Button(new_window, text="Enter", width=25, height=3,
                              command=lambda:
                              install_server_background(new_window, backup_entry.get(),
                                                                    secret_entry.get()))
    enter_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                            highlightthickness=0, highlightbackground="black",
                            highlightcolor="black",padx=10, pady=5, font=("Arial", 10),
                            overrelief=tk.RIDGE,)
    enter_button.place(relx=0.5, rely=0.7, anchor=tk.CENTER)
    # Center the text fields and labels
    new_window.update()
    backup_label.place(relx=0.5, rely=0.4, anchor=tk.CENTER)
    backup_entry.place(relx=0.5, rely=0.45, anchor=tk.CENTER)
    secret_label.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
    secret_entry.place(relx=0.5, rely=0.55, anchor=tk.CENTER)
    back_button = tk.Button(new_window, text="Back", width=25, height=3,
                            command=lambda: main_window(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1,
                           highlightthickness=0, highlightbackground="black",
                           highlightcolor="black",padx=10, pady=5, font=("Arial", 10),
                           overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.85, anchor=tk.CENTER)
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
    button4 = tk.Button(window, text="Exit",width=25,height=3,command=exit)
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
    t = tk.Tk()
    main_window(t)
    return 0

if __name__ == "__main__":
    main()
