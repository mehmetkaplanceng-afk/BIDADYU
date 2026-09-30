FROM node:24-alpine AS build
WORKDIR /app
COPY src/frontend/package*.json ./
RUN npm install
COPY src/frontend/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
