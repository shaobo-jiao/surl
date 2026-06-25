# syntax=docker/dockerfile:1

# stage 1: build
FROM node:24-bookworm-slim AS build
WORKDIR /build

# build package first, which is less frequently updated to benefit from build cache;
COPY package.json package-lock.json ./
RUN npm install 

# build app
COPY . .
# more on ARG and ENV later, need match host
ENV VITE_APP_URL=http://localhost:5001
ENV VITE_API_BASE_URL=http://localhost:5000/api/short-urls
RUN npm run build


# stage 2: runtime
FROM nginx:stable-alpine AS final

# remove the default site
RUN rm -rf /usr/share/nginx/html/
RUN rm /etc/nginx/conf.d/default.conf

# copy over surl site
COPY --from=build /build/dist/ /usr/share/nginx/surl/
COPY nginx.conf /etc/nginx/conf.d/surl.conf
RUN nginx -t

EXPOSE 80