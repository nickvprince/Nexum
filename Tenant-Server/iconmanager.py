"""
# Program: Tenant-server
# File: iconmanager.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the IconManager class. This class is used
# to interact with the tray icon

# Class Types: 
#               1. IconManager - Controller

"""
# pylint: disable= import-error, unused-argument

import threading
import time
import os
import pystray
from PIL import Image
from logger import Logger
from api import API
from helperfunctions import logs, tenant_portal,POLLING_INTERVAL
from sql import current_dir
image_path = os.path.join(current_dir, 'n.png') # path to the icon image






class IconManager():
    """
    Class to manage the tray icon
    Type: Controller
    @param image: the path to the image to be used for the icon
    @param menu: the menu to be used for the icon
    @param title: the title of the icon
    @param l: the logger
    """
    logger = Logger()
    old_status = ""
    # change the status of the job every 5 seconds
    def change_status(self):
        """
        Changes the status of the job every 5 seconds
        """
        l = Logger()
        while True :
            time.sleep(POLLING_INTERVAL)
            percent = IconManager.get_percent()
            version = IconManager.get_version()
            new_status = IconManager.get_status()

            if self.old_status != new_status:
                l.log("INFO", "IconManager.change_status",
                  "Status changed to "+str(new_status) + f" from{self.old_status} :"+str(percent)+
                  ":"+str(version), "0","IconManager.py")
            status = new_status
            self.old_status = status
            menu = IconManager.create_menu(status,percent,version ,logs, tenant_portal)

            self.update_menu(menu)

    # stop the tray icon
    def stop(self):
        """
        Stops the icon
        """
        self.icon.stop()
    # actually run the icon
    def run_icon(self):
        """
        opens a thread and starts the icon
        """
        self.icon.run()
    # update currently running menu
    def update_menu(self,menu):
        """
        updates the menu of the icon
        """
        self.icon.menu = menu
        self.icon.update_menu()

    # open a thread and start the icon
    def run(self):
        """
        runs the icon
        """
        thread = threading.Thread(target=self.run_icon)
        thread2 = threading.Thread(target=self.change_status)
        thread.start()
        thread2.start()
    #change the image
    def set_image(self, image):
        """
        changes the image of the icon
        """
        self.icon.icon = image

    # create a menu from the given parameters
    # pylint: disable=no-self-argument
    def create_menu(status, percent, version, logs_function,tenant_portal_function):
        """
        Creates a menu that can be used from the menu paramaters
        """
        item1 = pystray.MenuItem("Status: "+status, lambda: None)
        item2 = pystray.MenuItem("Percent: "+str(percent), lambda: None)
        item3 = pystray.MenuItem("Version: "+version, lambda: None)
        item4 = pystray.MenuItem("Click to download logs", logs_function)
        item5 = pystray.MenuItem("Tenant Portal", tenant_portal_function)
        return (item1, item2, item3, item4,item5)
    #pylint: enable=no-self-argument

    # initialize the IconManager
    def __init__(self, image, menu, title,l):
        self.logger = l
        image = Image.open(image)
        self.icon = pystray.Icon(title, image, title, menu)

    # These 3 calls should get the status of the job assigned to this computer,
    # the percent complete, and the version of the program
    # pylint: disable=no-method-argument, disable=no-self-argument, disable=missing-function-docstring
    def get_status():
        return API.get_status()
    def get_percent():
        return API.get_percent()
    def get_version():
        return API.get_version()
    # pylint: enable=no-method-argument, enable=no-self-argument, enable=missing-function-docstring
