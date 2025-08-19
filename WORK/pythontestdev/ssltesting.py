import socket
import ssl

def https_get_request(host, path="/", port=443):
    """
    Issues an HTTPS GET request to the specified host and path.

    Args:
        host (str): The hostname or IP address of the target server.
        path (str): The path on the server to request (e.g., "/index.html").
        port (int): The port number for the HTTPS connection (default is 443).
    """
    try:
        # 1. Create a TCP socket
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

        # 2. Wrap the socket with SSL/TLS
        # context = ssl.create_default_context() creates a secure SSL context
        # that handles certificate validation and other security features.
        context = ssl.create_default_context()
        sslsock = context.wrap_socket(sock, server_hostname=host)

        # 3. Connect to the server
        sslsock.connect((host, port))

        # 4. Construct the HTTP GET request
        request = f"GET {path} HTTP/1.1\r\nHost: {host}\r\nConnection: close\r\n\r\n"
        sslsock.sendall(request.encode('utf-8'))

        # 5. Receive the response
        response_data = b""
        while True:
            data = sslsock.recv(4096)
            if not data:
                break
            response_data += data

        # 6. Close the connection
        sslsock.close()

        print(response_data.decode('utf-8', errors='ignore'))

    except socket.gaierror as e:
        print(f"Error resolving host '{host}': {e}")
    except ssl.SSLError as e:
        print(f"SSL/TLS error: {e}")
    except ConnectionRefusedError:
        print(f"Connection refused by {host}:{port}. Ensure the server is running and accessible.")
    except Exception as e:
        print(f"An unexpected error occurred: {e}")

if __name__ == "__main__":
    target_host = "www.example.com"  # Replace with your desired host
    target_path = "/"                 # Replace with your desired path

    print(f"Issuing HTTPS GET request to {target_host}{target_path}")
    https_get_request(target_host, target_path)
