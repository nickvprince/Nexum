"""
# Program: Tenant-server
# File: conf.py
# Authors: 1. Danny Smith
#
# Date: 3/19/2024
# purpose: 
# This file contains the Configuration class. This class is used 
# as a entity to hold configuration information

# Class Types: 
#               1. Configuration - Entity

"""


class Configuration():
    """
    holds configuration information
    type: entity
    Relationship: Job -> Configuration 1..1
    @param id: the id of the configuration
    @param tenantSecret: the secret of the tenant
    @param Address: the address of the tenant server
    @return: None
    """
    id = None
    tenant_secret = None
    address = None

    def __init__(self, id_in, tenant_secret, address):
        """
        Initializes the configuration
        """
        self.id = id_in
        self.tenant_secret = tenant_secret
        self.address = address
    # pylint: disable=missing-function-docstring
    # Getters and setters

    # Getters

    def get_tenant_secret(self):
        return self.tenant_secret
    def get_id(self):
        return self.id
    def get_address(self):
        return self.address

    # Setters

    def set_tenant_secret(self, tenant_secret_in):
        self.tenant_secret = tenant_secret_in
    def set_address(self, address):
        self.address = address
    def set_id(self, incoming_id):
        self.id = incoming_id
    #pylint: enable=missing-function-docstring