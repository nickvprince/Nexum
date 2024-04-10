"""
# Program: Tenant-Client
# File: logger.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the Logger class. This class is used
# to log messages to the database file

# Class Types: 
#               1. Logger - File IO

"""
# pylint: disable= import-error, unused-argument

from initsql import InitSql,sqlite3,logdirectory,logpath

class Logger():

    """
    Logger class to log messages to the database file
    Type: File IO
    Relationship: NONE
    """
    conn = None
    cursor = None

    # create the database file if it does not exist and connect to it then create the table
    def __init__(self):

        #ensure log files are ready
        InitSql.log_files()      

        #create connection
        self.conn = sqlite3.connect(logdirectory+logpath)
        self.cursor = self.conn.cursor()

    # log a message to the database
    def log(self, severity, subject, message, code, date):
        """
        Information
        """
        self.cursor.execute('''INSERT INTO logs (severity, subject, message, code, date)
                    VALUES (?, ?, ?, ?, ?)''', (severity, subject, message, code, date))
        self.conn.commit()
    @staticmethod
    def debug_print(message):
        """
        Used to print information in debugging to be easily switched during implementation
        @param message: the message to be printed
        """
        print(message)
