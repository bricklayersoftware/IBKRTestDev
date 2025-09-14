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
HOST = '0.0.0.0'  # 52.188.185.179
PORT = 44444        # Port to listen on (non-privileged ports are > 1023)

conn = None
client_addr = None
client_ip = None
client_port = None
server_socket = None

shared_data = []  # Data to be sent/received
data_lock = threading.Lock() # Lock to protect shared_data

def timestamp():
    # Format the datetime object into a string
    now = datetime.now()
    ts = now.strftime("%Y-%m-%d %H:%M:%S")
    return ts

def receive_thread():
    global conn
    
    logging.info(f"receive_thread: "+timestamp())

    while True:
        if conn == None:
            logging.info("receive_thread: no connection, nothing to receive from")
            time.sleep(1)
            continue
        
        try:
            data = conn.recv(1024) # blocks until something received (sent by client)
            
            if not data:
                break
            
            message = data.decode('utf-8')

            response = f"received message: [{message}] {len(data)} bytes " +timestamp()

            logging.info(response)

            # simulate some processing and add to shared data
            with data_lock:
                shared_data.append(message)
            
        except Exception as e:
            logging.info(f"receive_thread: exception: {e}")
            conn = None
            continue
        
    if conn:
        conn.close()
        
    conn = None

def send_thread():
    global conn
    
    logging.info(f"send_thread: "+timestamp())

    while True:
        time.sleep(1)  

        message_to_send = "server says hello, word! "+timestamp()

        if conn == None:
            logging.info("send_thread: connection is none")
            continue
        
        logging.info(f"sending to client: [{message_to_send}]")
        try:
            conn.sendall(message_to_send.encode('utf-8'))
        except Exception as e:
            logging.info(f"Error sending to a client: {e}")
            conn = None


def accept_conn_thread():
    global server_socket
    global conn
    global client_addr
    global client_ip
    global client_port
    
    while True:    

        if conn == None:
            server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            server_socket.bind((HOST, PORT))
            server_socket.listen()

            logging.info(f"listening on {HOST}:{PORT}")
            logging.info("waiting for new connection...")
            conn, client_addr = server_socket.accept() # wait for client to connect
            client_ip, client_port = client_addr

            logging.info(f"client connected with IP:port {client_ip}:{client_port}")

        else:
            time.sleep(1)
            logging.info("connection active")
            
    
def main():
    global conn
    global client_ip
    global client_port
    global client_addr
    global server_socket

    conn_thread = threading.Thread(target=accept_conn_thread)
    conn_thread.daemon = True
    conn_thread.start()

    client_thread = threading.Thread(target=receive_thread)
    client_thread.daemon = True
    client_thread.start()

    sender_thread = threading.Thread(target=send_thread)
    sender_thread.daemon = True
    sender_thread.start()

    while True:
        logging.info("pulse "+timestamp())
        time.sleep(1)
        
if __name__ == "__main__":
    main()