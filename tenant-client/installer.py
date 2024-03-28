"""
Installer file for the program
"""

import winreg
import traceback
from logger import Logger

"""

@staticmethod
def check_first_run(arg):

    l = Logger()
    # check for registry entry Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Nexum\A2
    # to see if this is the first run if it exists return, else call first_run()
    try:
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\\Nexum\\Client")
        winreg.CloseKey(key)
        return True
    except FileNotFoundError:
        return first_run(arg)
    except PermissionError:
        l.log("ERROR", "check_first_run", "Permission Error checking registry",
        "1005", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    except:
        l.log("ERROR", "check_first_run", "General Error checking registry",
        "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
    return False
"""

def main(key:str, address:str, secret:str, port:int):
    """ 
    Runs the Installation
    """
    # Check if registry key exists for the program
       # if it does, the program is already installed
            # start the program
    # if not install the program
        # reach out to server to see if download key is valid
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

if __name__ == "__main__":
    main("Install Key","127.0.0.1","Secret",5000)
