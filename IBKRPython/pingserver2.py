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
client_addr = None
client_ip = None
client_port = None

shared_data = []  # Data to be sent/received
data_lock = threading.Lock() # Lock to protect shared_data

def receive_thread():

    logging.info(f"receive_thread")

    while True:
        try:
            data = conn.recv(1024)
            if not data:
                break
            
            message = data.decode('utf-8')

            response = f"received bytes: {len(message)}"

            logging.info(response)

            # simulate some processing and add to shared data
            with data_lock:
                shared_data.append(message)
            
            # Send a response back to the client
            conn.sendall(response.encode('utf-8')) # text string: response.encode('utf-8'))

        except ConnectionResetError:
            logging.info(f"client disconnected unexpectedly.")
            break
        except Exception as e:
            logging.info(f"Error handling client: {e}")
            break

    logging.info(f"Client disconnected.")
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
                logging.info(f"Error sending to a client: {e}")


def main():
    global conn
    global client_ip
    global client_port
    global client_addr

    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen()

    logging.info(f"listening on {HOST}:{PORT}")

    connected_clients = []
    
    sender_thread = threading.Thread(target=send_thread, args=(conn,))
    sender_thread.daemon = True # Allows the main program to exit even if this thread is running
    sender_thread.start()

    conn, client_addr = server_socket.accept() # wait for client to connect
    client_ip, client_port = client_addr

    logging.info(f"connection from {client_ip}:{client_port}")

    client_thread = threading.Thread(target=receive_thread, args=(conn,))
    client_thread.daemon = True
    client_thread.start()

if __name__ == "__main__":
    main()