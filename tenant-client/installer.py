"""
Installer file for the program
"""
#pylint: disable= bare-except, import-error, wrong-import-order
import winreg
import traceback
from logger import Logger
import time
import requests
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
            # continue with the installation process
            pass
        else:
            # handle the invalid download key case
            pass
        # if it is valid
            # Curl the executable from the server
            # copy file to c:/program files/Nexum/
            # run commands
               # 1. cd.
               # 2. nexum.exe install
               # 3. sc config NexumClientServices_Second start=auto DisplayName="Nexum Client second" password=Nexum
               # 4. sc failure NexumClientServices_Second reset= 30 actions= restart/1000/restart/1000/restart/1000
               # 5. sc start NexumClientServices_Second
            # Create .bat file with command "timeout 2\ndel installer.exe"
            # run the bat file
            # exit the program
    pass
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
    main("Install Key","127.0.0.1","Secret",5000)

