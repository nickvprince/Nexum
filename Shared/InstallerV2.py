import tkinter as tk
from PIL import ImageTk, Image




def uninstall(window:tk.Tk):
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Uninstall")
    new_window.geometry("1000x600")

    back_button = tk.Button(new_window, text="Back", width=25, height=3, command=lambda: mainWindow(new_window))
    back_button.pack(pady=10)

    new_window.mainloop()





def installClient(window:tk.Tk):
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Install Client")
    new_window.geometry("1000x600")
    back_button = tk.Button(new_window, text="Back", width=25, height=3, command=lambda: mainWindow(new_window))
    back_button.pack(pady=10)
    new_window.mainloop()




def installServer(window:tk.Tk):
    window.destroy()
    new_window = tk.Tk()
    new_window.title("Install Server")
    new_window.geometry("1000x600")
    back_button = tk.Button(new_window, text="Back", width=25, height=3, command=lambda: mainWindow(new_window))
    back_button.pack(pady=10)
    new_window.mainloop()



def mainWindow(window:tk.Tk):
    # Create the main window
    window.destroy()
    window = tk.Tk()
    window.title("My App")
    window.geometry("1000x600")
    image = Image.open("C:\\Users\\teche\\OneDrive\\Desktop\\Nexum\\code\\Data\\nexum.png")  # Replace "path_to_image.jpg" with the actual path to your image file
    image = image.resize((800, 200))  # Adjust the size of the image as needed

    photo = ImageTk.PhotoImage(image)
    image_label = tk.Label(window, image=photo)
    image_label.pack(pady=50)

        # Create the buttons
    button1 = tk.Button(window, text="Install Server", width=25, height=3, command=lambda: installServer(window))
    button1.pack(pady=10)
    button2 = tk.Button(window, text="Install Client",width=25,height=3, command=lambda: installClient(window))
    button2.pack(pady=10)
    button3 = tk.Button(window, text="Uninstall",width=25,height=3, command=lambda: uninstall(window))
    button3.pack(pady=10)
    button4 = tk.Button(window, text="Exit",width=25,height=3,command=exit)
    button4.pack(pady=10)

        # Start the main loop
    window.mainloop()




def main():
    t = tk.Tk()
    mainWindow(t)
    return 0



if __name__ == "__main__":
    main()
