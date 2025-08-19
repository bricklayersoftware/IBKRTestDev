import socket

try:
    # 1. Create a socket object
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # 2. Define server address and port
    server_address = ('127.0.0.1', 9999) # Replace with your server's IP and port

    # 3. Connect to the server
    client_socket.connect(server_address)
    print(f"Connected to {server_address}")

    breakout = False
    while (not breakout):
        # 4. Send data
        message = "EXIT"
        client_socket.sendall(message.encode('utf-8'))
        print(f"Sent: '{message}'")

        # 5. Receive data
        received_data = client_socket.recv(1024)
        print(f"Received: '{received_data.decode('utf-8')}'")

except ConnectionRefusedError:
    print("Connection refused. Make sure the server is running and listening on the correct address and port.")
except Exception as e:
    print(f"An error occurred: {e}")
finally:
    # 6. Close the connection
    if 'client_socket' in locals() and client_socket:
        client_socket.close()
        print("Connection closed.")
