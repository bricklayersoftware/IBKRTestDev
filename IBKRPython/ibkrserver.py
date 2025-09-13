import http.server
import ssl
import socketserver

# Define the server address and port
HOST = "127.0.0.1"
PORT = 443

# Specify the certificate and key file paths
CERT_FILE = "cert.pem"
KEY_FILE = "key.pem"

class SimpleHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        # This handler serves files from the current directory
        super().do_GET()

# Create an SSL context
context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
context.load_cert_chain(certfile=CERT_FILE, keyfile=KEY_FILE)

# Create the HTTPS server
with socketserver.TCPServer((HOST, PORT), SimpleHTTPRequestHandler) as httpd:
    httpd.socket = context.wrap_socket(httpd.socket, server_side=True)
    print(f"Serving HTTPS on {HOST}:{PORT}")
    httpd.serve_forever()
