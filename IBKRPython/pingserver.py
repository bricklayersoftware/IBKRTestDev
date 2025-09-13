import sys
import time
import logging
from datetime import datetime
import threading
import sys
import time
import socket

logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(levelname)s - %(message)s')

HOST = '127.0.0.1'  # Standard loopback interface address (localhost)
PORT = 44444        # Port to listen on (non-privileged ports are > 1023)

def launch_reader():
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
                message_string = "Hello, world! This is a UTF-8 string."
                encoded_message = message_string.encode('utf-8')
                (data)  # Echo back the received data

    # Create a datetime object (e.g., current time)


logging.debug("This is a debug message.")
logging.info("This is an informational message.")

# A global variable to store the input
input_queue = []
# A lock to protect access to the shared input_queue
input_lock = threading.Lock()
# A flag to signal the input thread to stop
stop_input_thread = threading.Event()

def read_stdin_thread():
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
        
while True:
    now = datetime.now()

    input_thread = threading.Thread(target=read_stdin_thread)
    input_thread.start()

    # Format the datetime object into a string
    timestamp = now.strftime("%Y-%m-%d %H:%M:%S")

    print("starting up: "+timestamp)
    time.sleep(1)  # Pause execution for 1 second

    with input_lock:
        while input_queue:
            received_input = input_queue.pop(0)
            logging.info(f"Main thread processed input: '{received_input}'")

print("Finished reading from stdin.")
