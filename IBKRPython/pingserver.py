import sys
import time
import logging
from datetime import datetime
import threading
import sys
import time
import socket

logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(levelname)s - %(message)s')

logging.debug("This is a debug message.")
logging.info("This is an informational message.")

# Shared resources
shared_data = []  # Data to be sent/received
data_lock = threading.Lock() # Lock to protect shared_data

HOST = '127.0.0.1'  # Standard loopback interface address (localhost)
PORT = 44444        # Port to listen on (non-privileged ports are > 1023)

def timestamp():
    # Format the datetime object into a string
    now = datetime.now()
    ts = now.strftime("%Y-%m-%d %H:%M:%S")
    return ts

def reader_thread():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((HOST, PORT))
        s.listen()
        
        logging.info(f"Server listening on {HOST}:{PORT}")
        
        conn, addr = s.accept()
        
        with conn:
            logging.info(f"Connected by {addr}")
            while True:
                received_bytes = conn.recv(1024)  # Receive data in chunks of 1024 bytes -- will block until data is available
                if not received_bytes:
                    logging.info('empty buffer -- exiting')
                    break
                logging.info(f"Received from client: {received_bytes.decode('utf-8')}")

                with data_lock:
                    shared_data.append(received_bytes)


                message_string = "Hello, world! This is a UTF-8 string: "
                encoded_message = message_string.encode('utf-8')
                conn.sendall(encoded_message)  # Echo back the received data

    # Create a datetime object (e.g., current time)



# A global variable to store the input
input_queue = []
# A lock to protect access to the shared input_queue
input_lock = threading.Lock()
# A flag to signal the input thread to stop
stop_input_thread = threading.Event()

def writer_thread():
    """Function to be run in a separate thread to read from stdin."""
    logging.info("Input thread started. Enter lines, or Ctrl+D to stop.")
    while not stop_input_thread.is_set():
        try:
            line = sys.stdin.readline()
            if not line:  # EOF reached (e.g., Ctrl+D)
                stop_input_thread.set()
                break
            with input_lock:
                input_queue.append(line.strip())
        except EOFError:
            stop_input_thread.set()
            break
        except Exception as e:
            logging.info(f"Error in input thread: {e}")
            stop_input_thread.set()
            break
        

def main():
    
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen()
    print(f"Server listening on {HOST}:{PORT}")
    
    # Start a separate thread for sending data to clients
    sender_thread = threading.Thread(target=send_to_clients, args=(connected_clients,))
    sender_thread.daemon = True # Allows the main program to exit even if this thread is running
    sender_thread.start()

    while True:
        conn, addr = server_socket.accept()
        client_thread = threading.Thread(target=handle_client, args=(conn, addr))
        client_thread.daemon = True
        client_thread.start()

if __name__ == "__main__":
    main()