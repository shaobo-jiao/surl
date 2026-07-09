# Overview
This file is the documentation about this `surl` project.

The project has the following components and tech stacks:
- Frontend: built with React, TypeScript, Tailwind CSS, shadcn/ui, etc
- API: built with ASP.NET Core Minimal API, FluentValidation, etc
- Database: Postgres

Several deployment methods are examined throughout the project, 
and the details can be referred later in this documentation.


# Deployment and Hosting
Several deployment methods are examined in this project, from manual deployment 
to automation:
- manual deployment
- docker 
- docker compose
- CI/CD with GitHub Action 

Ubuntu Server is used to host the application. It is chosen because I want to make use of my old laptop. Cloud hosting like AWS is also examined for learning purpose.

Server info:
- Ubuntu Server 26.04 LTS

## Manual Deployment
In my first attempt to deploy the application, I tried to set up everything manually, including 
- installing anc configuring `nginx` to host the frontend
- installing and configuring `postgres` and `dotnet` to run backend as `systemd` services

### Installing Nginx and Deploying the Frontend
Install Nginx
```bash
$ sudo apt update
$ sudo apt install nginx 
```

After the installation, check Nginx is running by visiting localhost
```bash
curl http://localhost/
```

Delete the default Nginx at port 80:
```bash
$ sudo rm /etc/nginx/sites-enabled/default
```

Build the React app (using Vite) on local machine:
```bash
$ npm install
$ npm run build
```

Upload `dist\` to the server using `scp`:
```bash
$ scp -r dist\ user@server:
```

Place the contents under `/var/www/`:
```bash
# create directory surl/ 
$ sudo mkdir /var/www/surl

# move over the contents
$ sudo mv dist/* /var/www/surl
```

Create Linux user for the app (which is later used for API as well):
```bash
$ sudo adduser surl-user
```

Change owners and file permission:
```bash
# change owner
$ sudo chown -R surl-user:www-data /var/www/surl

# change file permission
$ sudo find /var/www/surl -type d -exec chmod 755 {} \;
$ sudo find /var/www/surl -type f -exec chmod 644 {} \;
```

Register and enable the website in Nginx:
```bash
# create the site,  the contents are as below
$ sudo nano /etc/nginx/sites-available/surl

# enable the site
$ sudo ln -s /etc/nginx/sites-available/surl /etc/nginx/sites-enabled/

# reload nginx
$ sudo systemctl reload nginx
```

```text
server {
    listen 5000;
    server_name surl.shaobo.dev;

    root /var/www/surl;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

For subsequent deployments, `rsync` can be used instead:
```bash
$ sudo rsync -av --delete --chown=surl-user:www-data --chmod=D755,F644 dist/ /var/www/surl/
```

Reference:
- [Nginx: Beginner's Guide](https://nginx.org/en/docs/beginners_guide.html)
- [Ubuntu: Install and Configure Nginx](https://ubuntu.com/tutorials/install-and-configure-nginx)

### Installing Postgres, ASP.NET Core Runtime and Deploying the Backend
Install Postgres 18:
```bash
% sudo apt update
$ sudo apt install postgresql-18 # resolve to 18
```

Login to the server with default user `postgres`, and create a user that maps to the admin Linux user for convenience:
```bash
# login as postgres
$ sudo -u postgres psql

# create a superuser that maps to the Linux user:
$ CREATE ROLE shaobo LOGIN SUPERUSER
```

Modify client authentication in `pg_hba.conf` file:
- Peer Authentication for Linux admin user
- Password Authentication otherwise

```text
# admin (keep):
local all postgres peer

# local:
local all shaobo peer

# IPv4:
host all all 127.0.0.1/32 scram-sha-256

# IPv6:
host all all ::1/128      scram-sha-256
```

Prepare the database for the application `surl`:
```sql
-- create database
CREATE DATABASE surl;

-- create roles: one for migration, the other for normal app use
CREATE ROLE surl_owner WITH LOGIN PASSWORD 'XXX';
CREATE ROLE surl_rw WITH LOGIN PASSWORD 'XXX';

-- allow connection
GRANT CONNECT ON DATABASE surl TO surl_owner;
GRANT CONNECT ON DATABASE surl TO surl_rw;

-- switch to database surl
-- $ \c surl

-- create schema and grant schema usage
CREATE SCHEMA surl AUTHORIZATION surl_owner;
GRANT USAGE ON SCHEMA surl TO surl_rw;

-- alter search_path in the db
ALTER ROLE surl_owner IN DATABASE surl SET search_path = surl;
ALTER ROLE surl_rw IN DATABASE surl SET search_path = surl;
ALTER ROLE shaobo IN DATABASE surl SET search_path = surl;

-- alter default privileges in the schema
ALTER DEFAULT PRIVILEGES FOR ROLE surl_owner IN SCHEMA surl
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO surl_rw;

ALTER DEFAULT PRIVILEGES FOR ROLE surl_owner IN SCHEMA surl
GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO surl_rw;
```

Install ASP.NET Core runtime 10:
```bash
$ sudo apt update
$ sudo apt install aspnetcore-runtime-10.0
```

Build the API project and prepare schema update script in local machine:
```bash
# build the package at bin/Release/net10.0/
$ dotnet publish -c Release -o ./bin/build/

# build the schema script at /bin/Release/ 
$ dotnet ef migrations script --idempotent -o bin/script/
```

Upload the build artifacts and schema script to the server via `scp`:
```bash
$ scp -r ./bin/build/ username@server:
$ scp ./bin/script/migration.sql username@server:
```

Run schema script as `surl_owner`:
```bash
$ psql -h localhost -p 5432 -U surl_owner -d surl -f migration.sql
```

Move the build artifacts to be under `/opt` and change file permissions:
```bash
# move over the build artifacts
$ sudo mkdir /opt/surl-api
$ sudo mv build/* /opt/surl-api/

# change owner
$ sudo chown -R surl-user:surl-user /opt/surl-api/

# change file permission
$ sudo find /opt/surl-api/ -type d -exec chmod 755 {} \;
$ sudo find /opt/surl-api/ -type f -exec chmod 744 {} \;
```

Create `.env` file under `/etc` to specify environment variables:
```bash
# create env file, contents as below
$ sudo mkdir /etc/surl-api
$ sudo nano /etc/surl-api/.env
```
```text
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://localhost:5002
ConnectionStrings__Default = Host=localhost;Database=surl;Username=surl_rw;Password=XXX
ConnectionStrings__Migration = Host=localhost;Database=surl;Username=surl_owner;Password=XX
```

Create `systemd` service file and enable the service:
```bash
# create service file, contents as below
$ sudo nano /etc/systemd/system/surl-api.service

# reload and enable the service
$ sudo systemctl daemon-reload
$ sudo systemctl enable --now surl-api
```
```text
[Unit]
Description=surl (URL Shortener) API
After=network.target

[Service]
WorkingDirectory=/opt/surl-api/
ExecStart=/usr/bin/dotnet /opt/surl-api/UrlShortener.Api.dll
Restart=always
User=surl-user
EnvironmentFile=/etc/surl-api/.env

[Install]
WantedBy=multi-user.target
```

Reference: 
- [Install .NET SDK or .NET Runtime on Ubuntu](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install?tabs=dotnet10&pivots=os-linux-ubuntu-2604)
- [Postgres 18 Docs](https://www.postgresql.org/docs/current/)

## Docker
Docker provides containerization for applications. Before starting with Docker Compose, standalone docker images and containers are first examined to host the application.




## Docker Compose









