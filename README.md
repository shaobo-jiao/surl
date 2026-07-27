# Overview
`SURL` is a URL Shortener project implemented with .NET Web API and React. 
A live preview is available at [https://surl.shaobo.dev](https://surl.shaobo.dev).

Features
- Generates a short URL that redirects to the originally provided URL. 
- User registration and URL management

# Tech stack of the project
- _.NET Minimal API_ to build the backend API
- _Postgres_ as the database
- _React_ + _TypeScript_ + _TailwindCSS_ to build the frontend
- _Docker_ and _Docker Compose_ for containerization
- _Cloudflare Tunnel_ to allow public access at [https://surl.shaobo.dev](https://surl.shaobo.dev)

# Try the application locally
Make sure you have the following installed in your computer:
- Docker Desktop
- .NET SDK 10.0

```bash
# clone the repo
$ git clone https://github.com/shaobo-jiao/surl.git
$ cd ./surl/

# install dotnet ef if it's not installed
$ dotnet tool install --global dotnet-ef --version 10.0.*

# prepare necessary secrets
$ cp ./secrets/postgres_password.example ./secrets/postgres_password
$ cp ./secrets/surl_owner_password.example ./secrets/surl_owner_password
$ cp ./secrets/surl_rw_password.example ./secrets/surl_rw_password

# generate migration script
$ dotnet restore ./backend/Surl.Api/
$ dotnet ef migrations script -i -p ./backend/Surl.Api/ -o ./backend/Surl.Api/bin/db_schema.sql

# start the containers
$ docker compose up -d
```

visit the application locally at [http://localhost:5001](http://localhost:5001)

# Project Details

## Version 2 (User Registration and URL Management)
...

Pending:
- logging
- review authentication and authorization: docs and current impl
- add url management api 
- add auth frontend
- add url management page frontend
- GitHub Actions for CI/CD
- explore rate limiting
- explore caching, Redis
- unit and integration testing for API 
- OIDC?
- mailing service?


## Version 1 (MVP)
API Endpoints:
- `POST /api/short-urls/`: accepts an URL and returns the shortened URL.
- `GET /api/short-urls/{code}`: searches and returns the original URL by given shortened URL code.

Frontend Pages:
- `/`: homepage that has a form to shorten a URL
- `/{code}`: redirection page that redirects to original URL or displays error

More:
- The Shortened URL ID is auto generated from 1 (later change to be from SnowflakeID)
- Base62 Encoding to convert a shortened URL ID to code
- producer-consumer pattern with background service for click event tracking
- logging? validation with endpoint filters? rate limiting? caching?

TODO:
- port over SnowflakeID
- port over ValidationFilter and add validations
- review and finish frontend pages
- explore caching
- explore rate limiting
- add logging