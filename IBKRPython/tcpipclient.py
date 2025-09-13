import socket

# Define the server's IP address and port
SERVER_IP = '52.188.185.179'  # Use 'localhost' or the actual IP of your server
SERVER_PORT = 44444           # The port the server is listening on

# Create a TCP/IP socket
client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

try:
    # Connect to the server
    client_socket.connect((SERVER_IP, SERVER_PORT))
    print(f"Connected to server at {SERVER_IP}:{SERVER_PORT}")

    while True:
        # Send data to the server
        message = "type hello"
        client_socket.sendall(message.encode('utf-8'))
        print(f"Sent: {message}")

        # Receive data from the server
        data = client_socket.recv(1024)  # Receive up to 1024 bytes
        print(f"Received: {data.decode('utf-8')}")

except ConnectionRefusedError:
    print(f"Connection refused. Ensure the server is running and listening on {SERVER_IP}:{SERVER_PORT}")
except Exception as e:
    print(f"An error occurred: {e}")
finally:
    # Close the client socket
    client_socket.close()
    print("Client socket closed.")