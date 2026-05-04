#!/bin/sh
cat > /usr/share/nginx/html/config.js <<EOF
window.__API_URL__ = "${VITE_API_URL:-http://localhost:5001}";
EOF

