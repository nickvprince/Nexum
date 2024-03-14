"""
Information
"""
from logger import Logger
class API():
    """
    Class to interact with the API. Used for local API calls and 
    easy integration when API calls are changed
    Type: Connector
    Relationship: NONE
    """

    @staticmethod
    def get_tenant_portal_url():
        """
        Gets the tenant portal URL from the tenant server device
        """
        Logger.debug_print("Getting tenant portal url")
        # call the API from tenant server to get the tenant portal URL
        return "https://nexum.com/tenant_portal"
    @staticmethod
    def get_status():
        """
        Call the API from tenant server to get the status of the client
        """
        Logger.debug_print("Getting status")
        # call the API from tenant server to get the status of the client
        return "running"
    @staticmethod
    def get_percent():
        """
        call the API from tenant server to get the percent complete of the job
        """
        Logger.debug_print("Getting percent")
        # call the API from tenant server to get the percent complete of the job
        return 70
    @staticmethod
    def get_version():
        """
        Call the API from tenant server to get the version of the program
        """
        Logger.debug_print("Getting version")
        # call the API from tenant server to get the version of the program
        return "1.2.7"
    @staticmethod
    def get_job():
        """
        Call the API from tenant server to get the job assigned to this computer
        """
        Logger.debug_print("Getting job")
        # call the API from tenant server to get the job assigned to this computer
        return "backup"
    @staticmethod
    def get_client_id():
        """
        Call the API from tenant server to get the client id
        """
        Logger.debug_print("Getting client id")
        # call the API from tenant server to get the client id
        return 1
    @staticmethod
    def get_tenant_id():
        """
        Call the API from tenant server to get the tenant id
        """
        Logger.debug_print("Getting tenant id")
        # call the API from tenant server to get the tenant id
        return 1
    @staticmethod
    def get_download_key():
        """
        call the API from tenant server to get the download key
        """
        Logger.debug_print("Getting download key")
        # call the API from tenant server to get the download key
        return "1234"

    @staticmethod
    def send_success_install(client_id,tenant_id,client_secret):
        """
        Call the API from tenant server to send the success install
        """
        Logger.debug_print("Sending success install")
        # call the API from tenant server to send the success install
        return True
