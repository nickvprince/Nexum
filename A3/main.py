"""
# Program: A3
# File: main.py
# Authors: 1. Danny Smith
#
# Date: 3/142024
# purpose: This is the main file for the program A3. This program ensures connectivity
# to A2 devices, recieves jobs from A1 and executes backups and heartbeats. It has a tray icon
# that the user may not interact with

# Note. Requires admin to run



"""

# pylint: disable= no-member
# pylint: disable= no-name-in-module


import time
from pyuac import main_requires_admin
from iconmanager import IconManager, image_path
from helperfunctions import logs, tenant_portal
from logger import Logger

# Global variables


@main_requires_admin
def main():
    """
    Main method of the program for testing and starting the program
    """
    l = Logger()

    # create the IconManager
    i = IconManager(image_path, IconManager.create_menu(IconManager.get_status(),
    IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum Server",l)
    # run the icon
    i.run()
    # log a message

    l.log("INFO", "Main", "Main has started", "000", time.asctime())

if __name__ == "__main__":

    main()
