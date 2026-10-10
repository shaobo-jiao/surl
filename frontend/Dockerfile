# syntax=docker/dockerfile:1
# build:
# docker build -t shaobojiao/surl-web --platform linux/amd64,linux/arm64 \
# --build-arg VITE_API_BASE_URL=https://surl-api.shaobo.dev  --build-arg VITE_APP_URL=https://surl.shaobo.dev .\frontend\

# stage 1: build
FROM node:24-bookworm-slim AS build
WORKDIR /build

# build package first, which is less frequently updated to benefit from build cache;
COPY package.json package-lock.json ./
RUN npm install 

# build app
COPY . .
ARG VITE_APP_URL
ENV VITE_APP_URL=$VITE_APP_URL
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build


# stage 2: runtime
FROM nginx:stable-alpine AS final

# remove the default site
RUN rm -rf /usr/share/nginx/html/
RUN rm /etc/nginx/conf.d/default.conf

# copy over surl site
COPY --from=build /build/dist/ /usr/share/nginx/surl/

# copy over nginx file
COPY ./nginx.conf /etc/nginx/conf.d/surl.conf
RUN nginx -t

EXPOSE 80