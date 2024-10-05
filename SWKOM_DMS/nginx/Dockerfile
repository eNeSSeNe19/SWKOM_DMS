# Use the official NGINX image from the Docker Hub
FROM nginx:latest

# Copy your custom NGINX configuration file
# COPY nginx.conf /etc/nginx/nginx.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy your frontend files (HTML, CSS, JS) into the NGINX container
COPY . /usr/share/nginx/html
