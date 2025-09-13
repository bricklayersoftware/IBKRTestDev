import socket
import threading
import sys
import logging
from datetime import datetime
import time

logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(levelname)s - %(message)s')

logging.debug("This is a debug message.")
logging.info("This is an informational message.")

# Configuration
HOST = '127.0.0.1'  # Standard loopback interface address (localhost)
PORT = 44444        # Port to listen on (non-privileged ports are > 1023)

conn = None

# Shared resources
shared_data = []  # Data to be sent/received
data_lock = threading.Lock() # Lock to protect shared_data

def receive_thread():
    """Handles a single client connection."""
    print(f"Connected by {addr}")
    while True:
        try:
            # Read data from the client
            data = conn.recv(1024)
            if not data:
                break
            message = data.decode()
            print(f"Received from {addr}: {message}")

            # Simulate some processing and add to shared data
            with data_lock:
                shared_data.append(message)
            
            # Send a response back to the client
            response = f"Server received: {len(message)}"
            conn.sendall(response.encode())

        except ConnectionResetError:
            print(f"Client {addr} disconnected unexpectedly.")
            break
        except Exception as e:
            print(f"Error handling client {addr}: {e}")
            break
    print(f"Client {addr} disconnected.")
    conn.close()

def send_thread():
    global conn
    
    while True:
        time.sleep(1)  

        with data_lock:
            if shared_data:
                message_to_send = " ".join(shared_data)
                shared_data.clear() # Clear after sending
            else:
                message_to_send = None

        if message_to_send:
            print(f"Sending to all clients: {message_to_send}")
            try:
                conn.sendall(message_to_send.encode())
            except Exception as e:
                print(f"Error sending to a client: {e}")


def main():
    global conn

    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen()
    print(f"Server listening on {HOST}:{PORT}")

    connected_clients = []
    
    # Start a separate thread for sending data to clients
    sender_thread = threading.Thread(target=send_thread, args=(conn,))
    sender_thread.daemon = True # Allows the main program to exit even if this thread is running
    sender_thread.start()

    while True:
        conn, addr = server_socket.accept()
        client_thread = threading.Thread(target=receive_thread, args=(conn, addr))
        client_thread.daemon = True
        client_thread.start()

if __name__ == "__main__":
    main()