"""
Installer file for the program
"""
#pylint: disable= bare-except, import-error, wrong-import-order
import winreg
import traceback
from logger import Logger
import time
import requests
import shutil
import os

REGISTRATION_PATH = "check-installer"

def install(key:str, address:str, secret:str, port:int):
    l = Logger()
    l.debug_print("Installing")
     # reach out to server to see if download key is valid
    T = requests.Response()
    try:
        T = requests.request("GET", f"http://{address}:{port}/{REGISTRATION_PATH}?key={key}", timeout=5, headers={"Content-Type":"application/json", "secret":secret})
    except:
        l.debug_print("Error in reaching out to server")
    if T.status_code == 200:
          # if it is valid
            # Curl the executable from the server

        # currently copy since its local
        # Copy file to c:/program files/Nexum/
        shutil.copy(os.path.join("C:\\Users\\teche\\Conestoga College\\Nicholas Prince - Capstone Collaboration\\Danny vs-code\\tenant-server\\", "nexum.exe"), "C:/Program Files/Nexum/nexum.exe")

        # Create .bat file with command "timeout 5\ndel C:/Users/teche/Conestoga College/Nicholas Prince - Capstone Collaboration/Danny vs-code/tenant-server/nexum.exe"
        with open("C:/Program Files/Nexum/nexum.bat", "w") as file:
            file.write("timeout 5\n")
            file.write("del \"C:\\Users\\teche\\Conestoga College\\Nicholas Prince - Capstone Collaboration\\Danny vs-code\\tenant-server\\nexum.exe\"\n")
            file.write("del \"%~f0\"")
            file.close()
            # run the bat file
        os.system("\"C:/Program Files/Nexum/nexum.bat\"")
            # exit the program
        
        # Add registry keys
        try:
            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\nexum"
            run_key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", 0, winreg.KEY_WRITE)
            winreg.SetValueEx(run_key, "nexum", 0, winreg.REG_SZ, r"C:\Program Files\Nexum.exe")
            winreg.CloseKey(run_key)

            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\nexum"
            app_key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths", 0, winreg.KEY_WRITE)
            nexum_key = winreg.CreateKey(app_key, "nexum")
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_SZ, r"C:\Program Files\Nexum\nexum.exe")
            winreg.SetValueEx(nexum_key, "Path", 0, winreg.REG_SZ, r"C:\Program Files\Nexum")
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)

            l.debug_print("Registry keys added successfully")
        except Exception as e:
            l.debug_print(f"Error adding registry keys: {str(e)}")
        l.debug_print("Valid Key")
    elif T.status_code == 401:
        l.debug_print("Invalid Secret")
    elif T.status_code == 403:
        l.debug_print("Invalid Key")
    else:
        l.debug_print("Invalid Key")

def main(key:str, address:str, secret:str, port:int):
    """ 
    Runs the Installation
    """
    l = Logger()
    # Check if registry key exists for the program "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\nexum"
    try:
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\\microsoft\\windows\\currentversion\\app paths\\nexum")
        winreg.CloseKey(key)
        l.debug_print("key exists")
        #start program here
        return True
    except FileNotFoundError:
        # if not installed install the program
        return install(key, address, secret, port)
    except PermissionError:
        l.log("ERROR", "check_first_run", "Permission Error checking registry",
        "1005", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        l.log("ERROR", "check_first_run", "General Error checking registry",
        "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))


    
       
    pass

if __name__ == "__main__":
    main("LJA;HFLASBFOIASH[jfnW.FJPIH","127.0.0.1","ASDFGLKJHTQWERTYUIOPLKJHGFVBNMCD",5000)