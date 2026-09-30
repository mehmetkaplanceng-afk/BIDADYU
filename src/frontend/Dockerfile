# Multi-stage Dockerfile for Frontend (React + Vite + NGINX)
FROM node:20-alpine AS build
WORKDIR /app

COPY src/frontend/package*.json ./
RUN npm ci

COPY src/frontend/ ./
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html

# NGINX Custom Config for SPA Routing
RUN echo 'server { \
    listen 80; \
    location / { \
        root /usr/share/nginx/html; \
        index index.html index.htm; \
        try_files $uri $uri/ /index.html; \
    } \
    location /api/ { \
        proxy_pass http://backend:5000/api/; \
        proxy_set_header Host $host; \
        proxy_set_header X-Real-IP $remote_addr; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
