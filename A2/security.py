import base64
import hashlib
import time
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
from logger import Logger
CLIENT_SECRET = None # secret for the client to communicate with A2
# pylint: disable= bare-except

class Security():
    """
    Class to manage security
    Type: Security
    Relationship: NONE
    """
    @staticmethod
    def set_client_secret(secret):
        """
        Set the client secret
        """
        global CLIENT_SECRET
        CLIENT_SECRET = secret
    @staticmethod
    def split_string(string):
        """
        split a string in half with right side majority
        """
        length = len(string)
        half_length = length // 2
        right_side = str(string)[half_length:]
        left_side = str(string)[:half_length]
        return left_side, right_side
    #sha256 a string
    @staticmethod
    def sha256_string(string):
        """
        SHA256 a string
        """
        # Convert the string to bytes
        string_bytes = str(string).encode('utf-8')

            # Compute the SHA-256 hash
        sha256_hash = hashlib.sha256(string_bytes).hexdigest()

        return sha256_hash

    # encrypt a string using AES
    @staticmethod
    def encrypt_string(password, string):
        """
        Encrypt a string with AES-256 bit encryption
        """
        # Pad the password to be 16 bytes long
        password_hashed = str(password).ljust(16).encode('utf-8')

        # Create a new AES cipher with the password as the key
        cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
        encryptor = cipher.encryptor()

        # Pad the string to be a multiple of 16 bytes long
        string = string.ljust((len(string) // 16 + 1) * 16).encode('utf-8')

        # Encrypt the string using AES
        encrypted_string = encryptor.update(string) + encryptor.finalize()

        # Encode the encrypted string in base64
        encoded_string = base64.b64encode(encrypted_string)

        return encoded_string.decode('utf-8')

    # decrypt a string using AES
    @staticmethod
    def decrypt_string(password, string):
        """
        Decrypt a string with AES-256 bit decryption
        """
        l = Logger()
        # Pad the password to be 16 bytes long
        password_hashed = str(password).ljust(16).encode('utf-8')

        # Create a new AES cipher with the password as the key
        cipher = Cipher(algorithms.AES(password_hashed), modes.ECB(), backend=default_backend())
        decryptor = cipher.decryptor()

        # Decode the string from base64
        decoded_string = base64.b64decode(string)

        # Decrypt the string using AES
        decrypted_string = decryptor.update(decoded_string) + decryptor.finalize()
        try:
            return decrypted_string.decode('utf-8')
        except UnicodeDecodeError:
            l.log("ERROR", "decrypt_string", "Decryption failed", "1004", time.asctime())
            return "Decryption failed"
        except:
            l.log("ERROR", "decrypt_string", "General Error decrypting string", "1002", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            return "Decryption failed"

    # add salt, pepper, and salt2 to the string
    @staticmethod
    def add_salt_pepper(string="", salt="", pepper="", salt2=""):
        """
        Adds salt, pepper, and salt2 to the string in a specific way
        """
        salt = salt[-1] + salt[1:-1] + salt[0]  # Swap first and last letter of salt
        pepper = pepper[:-2] + pepper[-1] + pepper[-2]
        temp = salt2[:2]  # Save the first 2 letters of salt2
        salt2 = pepper[:2] + salt2[2:]  # Write the first two letters of pepper to salt2
        pepper = temp[:2] + pepper[2:]  # Copy the temp values to the first 2 letters of pepper

        string_return = Security.split_string(string)


        return salt + string_return[0] + pepper +string_return[1]+ salt2
    # remove salt, pepper, and salt2 from the string
    @staticmethod
    def remove_salt_pepper(string, salt, pepper, salt2):
        """
        Removes salt, pepper, and salt2 from the string in a specific way
        """
        salt = salt[-1] + salt[1:-1] + salt[0]
        pepper = pepper[:-2] + pepper[-1] + pepper[-2]
        temp = salt2[:2]
        salt2 = pepper[:2] + salt2[2:]
        pepper = temp[:2] + pepper[2:]

        # remove salt from front of string
        string2 = str(string)[len(salt):]
        # remove all trailing whitespace
        string3 = str(string2).rstrip()
        #remove salt2 from end of string
        string4 = str(string3)[:-len(salt2)]
        # find pepper in the remaining string and remove it
        pepper_index = str(string4).find(pepper)
        string5 = string4[:pepper_index] + string4[pepper_index+len(pepper):]
        return string5

    # decrypt the clientSecret
    # clientSecret is the global clientSecret
    # given clientSecret is 32 characters long set the password to [0][31][1][30][2][29]...[15][16]
    @staticmethod
    def decrypt_client_secret(client_secret_in):
        """
        Uses a specific sequence to decrypt the client secret
        """
        password = ""
        for i in range(16):
            print(CLIENT_SECRET)
            password += str(CLIENT_SECRET)[i] + str(CLIENT_SECRET)[31-i]
        try:
            return Security.decrypt_string(password, client_secret_in).strip()
        except ValueError:
            l = Logger()
            l.log("ERROR", "decrypt_client_secret", "Decryption failed", "1004", time.strftime("%Y-%m-%d %H:%M:%S:%m", time.localtime()))
            return "Decryption failed"
        except:
            return ""
    # uses plaintext clientSecret to encrypt the clientSecret
    # clientSecret is the global clientSecret
    # given clientSecret is 32 characters long set the password to [0][31][1][30][2][29]...[15][16]
    @staticmethod
    def encrypt_client_secret(client_secret_in):
        """
        Uses a specific sequence to encrypt the client secret
        """
        password = ""
        for i in range(16):
            password += str(CLIENT_SECRET)[i] + str(CLIENT_SECRET)[31-i]
        return Security.encrypt_string(password, client_secret_in)
