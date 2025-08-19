const https = require('https');
const fs = require('fs');
const path = require('path');

// Path to your SSL/TLS certificate and key
const sslOptions = {
  key: fs.readFileSync(path.join(__dirname, 'privkey.pem')),
  cert: fs.readFileSync(path.join(__dirname, 'pubkey.pem')),
  // Enable all security features
  minVersion: 'TLSv1.2',
  // Recommended security settings
  secureOptions: require('constants').SSL_OP_NO_SSLv3 |
              require('constants').SSL_OP_NO_TLSv1 |
              require('constants').SSL_OP_NO_TLSv1_1
};

// Create the HTTPS server
const server = https.createServer(sslOptions, (req, res) => {
  // Security headers
  res.setHeader('Strict-Transport-Security', 'max-age=31536000; includeSubDomains');
  res.setHeader('X-Content-Type-Options', 'nosniff');
  res.setHeader('X-Frame-Options', 'SAMEORIGIN');
  res.setHeader('X-XSS-Protection', '1; mode=block');
  res.setHeader('Referrer-Policy', 'strict-origin-when-cross-origin');

  // Handle different routes
  if (req.url === '/') {
    res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
    res.end('<h1>Welcome to the Secure Server</h1><p>Your connection is encrypted!</p>');
  } else if (req.url === '/api/status') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ status: 'ok', time: new Date().toISOString() }));
  } else {
    res.writeHead(404, { 'Content-Type': 'text/plain' });
    res.end('404 Not Found');
  }
});

// Handle server errors
server.on('error', (error) => {
  console.error('Server error:', error);
});

// Start the server on port 3000 (HTTPS default is 443 but requires root)
const PORT = process.env.PORT || 3000;
server.listen(PORT, '0.0.0.0', () => {
  console.log(`Server running at https://localhost:${PORT}`);
  console.log('Press Ctrl+C to stop the server');
});
