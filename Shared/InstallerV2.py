import tkinter as tk
from PIL import ImageTk, Image
import os
import winreg
import shutil
import subprocess
import time

AUTO_RUN_KEY = r"Software\Microsoft\Windows\CurrentVersion\Run"
APP_PATH_KEY = r"Software\Microsoft\Windows\CurrentVersion\App Paths"
TITLE = "Nexum"

def uninstall_program():

    not_installed_indentifiers:int = 0
    identifiers_count:int =0

    identifiers_count += 1
    try:
        winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY)
        print("Deleting auto run key")
        winreg.DeleteKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY)
        not_installed_indentifiers += 1
    except:
        not_installed_indentifiers += 1

    identifiers_count += 1
    try:
        winreg.DeleteKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY + "\\Nexum")
        not_installed_indentifiers += 1
    except Exception as e:
        print(e)
        not_installed_indentifiers += 1

    identifiers_count += 1
    if os.path.exists("C:\\Program Files\\Nexum"):

        print("Deleting Nexum folder")
        try:
            subprocess.call(["taskkill", "/F", "/IM", "watchdog.exe"])
            subprocess.call(["taskkill", "/F", "/IM", "nexum.exe"])
            breakcount:int = 0
            while os.path.exists("C:\\Program Files\\Nexum"):
                time.sleep(1)
                breakcount += 1
                if breakcount > 5:
                    break
            shutil.rmtree("C:\\Program Files\\Nexum")
            not_installed_indentifiers += 1
        except:
            pass
    else:
        not_installed_indentifiers += 1
    
    uninstall_percentage:float = (not_installed_indentifiers/identifiers_count) * 100

    print("Program uninstalled : " + str(uninstall_percentage) + "%")

def uninstall(window:tk.Tk):
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Uninstall")
    new_window.geometry("1000x600")
    new_window.resizable(False, False)  # Set resizable to False

    lock_label = tk.Label(new_window, text="Lock Code:")
    lock_label.place(relx=0.40, rely=0.6, anchor=tk.CENTER)

    lock_entry = tk.Entry(new_window)
    lock_entry.place(relx=0.52, rely=0.6, anchor=tk.CENTER)

    uninstall_button = tk.Button(new_window, text="Uninstall", width=25, height=3, command=uninstall_program)
    uninstall_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=0, highlightbackground="black", highlightcolor="black", padx=10, pady=5, font=("Arial", 10), overrelief=tk.RIDGE)
    uninstall_button.place(relx=0.75, rely=0.6, anchor=tk.CENTER)

    back_button = tk.Button(new_window, text="Back", width=25, height=3, command=lambda: mainWindow(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=0, highlightbackground="black", highlightcolor="black", padx=10, pady=5, font=("Arial", 10), overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.7, anchor=tk.CENTER)

    new_window.mainloop()

def installnexumfile():
        # call to server to get install location and CURL to c:\Program Files\Nexum
        # OR
        # copy ./nexum.exe to C:\Program Files\Nexum
        current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
        path = os.path.join(current_dir,'nexum.exe') # directory for logs
        shutil.copy(path, "C:/Program Files/Nexum/nexum.exe")
        print("Installing Nexum file")

def installwatchdogfile():
        # call to server to get install location and CURL to c:\Program Files\Nexum
        # OR
        # copy ./watchdog.exe to C:\Program Files\Nexum
        current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
        path = os.path.join(current_dir,'watchdog.exe') # directory for logs
        shutil.copy(path, "C:/Program Files/Nexum/watchdog.exe")
        print("Installing Watchdog file")

def notifyServer():
    # call to server to notify that the installation is complete
    print("Notifying Server")
    pass

def installClientbackground(window:tk.Tk, backupserver:str, secret:str):
    print(backupserver)
    print(secret)

    # check with server if the secret is correct and get a client id back

    # CORRECT SECRET
    if secret == "1234":
        # Create a folder C:\Program Files\Nexum
        try:
            os.mkdir("C:\\Program Files\\Nexum")
            print("Nexum folder created")
        except:
            print("Nexum folder already exists or could not be created")
            pass

        installnexumfile()
        installwatchdogfile()
        # create keys in registry
        try:
            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\nexum"
            run_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, AUTO_RUN_KEY)
            winreg.SetValueEx(run_key, TITLE, 0, winreg.REG_SZ, r"C:\Program Files\Nexum\Nexum.exe")
            winreg.CloseKey(run_key)
            print("Run key added")

            # Add key "Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\nexum"
            app_key = winreg.CreateKey(winreg.HKEY_LOCAL_MACHINE, APP_PATH_KEY)
            nexum_key = winreg.CreateKey(app_key, TITLE)
            winreg.SetValueEx(nexum_key, "", 0, winreg.REG_SZ, r"C:\Program Files\Nexum\nexum.exe")
            winreg.SetValueEx(nexum_key, "Path", 0, winreg.REG_SZ, r"'C:\Program Files\Nexum'")
            winreg.SetValueEx(nexum_key, "Enable", 0, winreg.REG_SZ, "1")  # Enable the startup app
            winreg.CloseKey(nexum_key)
            winreg.CloseKey(app_key)
            print("App key added")
            # q: How do I toggle the startup app to enabled?
            # a: You can use the registry key to enable or disable the startup app
            # q: I have added this but it is toggled as disabled?

        except Exception as e:
            pass

        # run c:\Program Files\Nexum\nexum.exe
        # run c:\Program Files\Nexum\watchdog.exe
        # Run nexum.exe
        nexum_path = r'"C:\Program Files\Nexum\nexum.exe"'
        subprocess.Popen(nexum_path, shell=True)
        print("Nexum.exe ran")

        # Run watchdog.exe
        watchdog_path = r'"C:\Program Files\Nexum\watchdog.exe"'
        subprocess.Popen(watchdog_path, shell=True)
        print("Watchdog.exe ran")

        # notify server that the installation is complete
        notifyServer()

        #incorrect secret
    else:
        print("Incorrect Secret")



def installClient(window:tk.Tk):
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Install Client")
    new_window.geometry("1000x600")
    new_window.resizable(False, False)  # Set resizable to False

    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    dir = os.path.join(current_dir,'../Data/nexum.png') # directory for logs
    image = Image.open(dir)  # Replace "path_to_image.jpg" with the actual path to your image file
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(new_window, image=photo)
    image_label.pack(pady=5)
    
    # Create the text fields and labels
    backup_label = tk.Label(new_window, text="Backup Server Address:")
    backup_label.pack(pady=(0, 10))
    backup_entry = tk.Entry(new_window)
    backup_entry.pack(pady=(0, 10))

    secret_label = tk.Label(new_window, text="Client Secret:")
    secret_label.pack(pady=(0, 10))
    secret_entry = tk.Entry(new_window)
    secret_entry.pack(pady=(0, 10))

    # Center the text fields and labels
    new_window.update()
    backup_label.place(relx=0.5, rely=0.4, anchor=tk.CENTER)
    backup_entry.place(relx=0.5, rely=0.45, anchor=tk.CENTER)
    secret_label.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
    secret_entry.place(relx=0.5, rely=0.55, anchor=tk.CENTER)
    enter_button = tk.Button(new_window, text="Enter", width=25, height=3, command=lambda: installClientbackground(new_window, backup_entry.get(), secret_entry.get()))
    enter_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=0, highlightbackground="black", highlightcolor="black", padx=10, pady=5, font=("Arial", 10), overrelief=tk.RIDGE)
    enter_button.place(relx=0.5, rely=0.7, anchor=tk.CENTER)
    back_button = tk.Button(new_window, text="Back", width=25, height=3, command=lambda: mainWindow(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=0, highlightbackground="black", highlightcolor="black", padx=10, pady=5, font=("Arial", 10), overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.9, anchor=tk.CENTER)
    new_window.mainloop()

def installServer(window:tk.Tk):
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Install Server")
    new_window.geometry("1000x600")
    new_window.resizable(False, False)  # Set resizable to False

    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    dir = os.path.join(current_dir,'../Data/nexum.png') # directory for logs
    image = Image.open(dir)  # Replace "path_to_image.jpg" with the actual path to your image file
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(new_window, image=photo)
    image_label.pack(pady=5)

    # Create the text fields and labels
    backup_label = tk.Label(new_window, text="Data Manager Server Address:")
    backup_label.pack(pady=(0, 10))
    backup_entry = tk.Entry(new_window)
    backup_entry.pack(pady=(0, 10))

    secret_label = tk.Label(new_window, text="Tenant Secret:")
    secret_label.pack(pady=(0, 10))
    secret_entry = tk.Entry(new_window)
    secret_entry.pack(pady=(0, 10))

    # Center the text fields and labels
    new_window.update()
    backup_label.place(relx=0.5, rely=0.4, anchor=tk.CENTER)
    backup_entry.place(relx=0.5, rely=0.45, anchor=tk.CENTER)
    secret_label.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
    secret_entry.place(relx=0.5, rely=0.55, anchor=tk.CENTER)
    back_button = tk.Button(new_window, text="Back", width=25, height=3, command=lambda: mainWindow(new_window))
    back_button.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=0, highlightbackground="black", highlightcolor="black", padx=10, pady=5, font=("Arial", 10), overrelief=tk.RIDGE)
    back_button.place(relx=0.5, rely=0.7, anchor=tk.CENTER)
    new_window.mainloop()

def mainWindow(window:tk.Tk):
    # Create the main window
    window.destroy()
    window = tk.Tk()
    window.title("My App")
    window.geometry("1000x600")
    window.resizable(False, False)  # Set resizable to False

    current_dir = os.path.dirname(os.path.abspath(__file__)) # working directory
    dir = os.path.join(current_dir,'../Data/nexum.png') # directory for logs
    image = Image.open(dir)  # Replace "path_to_image.jpg" with the actual path to your image file
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(window, image=photo)
    image_label.pack(pady=50)

    # Create the buttons
    button1 = tk.Button(window, text="Install Server", width=25, height=3, command=lambda: installServer(window))
    button1.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=0, highlightbackground="black", highlightcolor="black", padx=10, pady=1, font=("Arial", 10), overrelief=tk.RIDGE)
    button1.pack(pady=10)
    button2 = tk.Button(window, text="Install Client",width=25,height=3, command=lambda: installClient(window))
    button2.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=0, highlightbackground="black", highlightcolor="black", padx=10, pady=1, font=("Arial", 10), overrelief=tk.RIDGE)
    button2.pack(pady=10)
    button3 = tk.Button(window, text="Uninstall",width=25,height=3, command=lambda: uninstall(window))
    button3.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=0, highlightbackground="black", highlightcolor="black", padx=10, pady=1, font=("Arial", 10), overrelief=tk.RIDGE)
    button3.pack(pady=10)
    button4 = tk.Button(window, text="Exit",width=25,height=3,command=exit)
    button4.configure(bg="purple", fg="black", bd=1, relief=tk.SOLID, borderwidth=1, highlightthickness=2, highlightbackground="black", highlightcolor="black", padx=10, pady=1, font=("Arial", 10), overrelief=tk.RIDGE)
    button4.pack(pady=10)

    # Start the main loop
    window.mainloop()

def main():
    t = tk.Tk()
    mainWindow(t)
    return 0

if __name__ == "__main__":
    main()
