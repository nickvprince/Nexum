"""
# Program: tenant-server
# File: main.py
# Authors: 1. Danny Smith
#
# Date: 3/192024
# purpose: This is the main file for the program A2. This program ensures connectivity
# to A3, recieves jobs from A3 and executes backups and heartbeats. It has a tray icon
# that the user may not interact with


"""

# pylint: disable= no-member,no-name-in-module, import-error


import time
from pyuac import main_requires_admin
from iconmanager import IconManager, image_path
from helperfunctions import logs, tenant_portal
from logger import Logger
from flaskserver import FlaskServer

# Global variables


@main_requires_admin
def main():
    """
    Main method of the program for testing and starting the program
    """
    logger = Logger()

    # create the IconManager
    icon_manager = IconManager(image_path, IconManager.create_menu(IconManager.get_status(),
    IconManager.get_percent(), IconManager.get_version(), logs, tenant_portal), "Nexum Server",logger)
    # run the icon
    icon_manager.run()
    # log a message

    logger.log("INFO", "Main", "Main has started", "000", time.asctime())
    flask = FlaskServer()
    flask.run()

if __name__ == "__main__":

    main()
