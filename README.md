# Overview
This project implements the URL Shortener idea in the book "System Design Interview". 
- Backend: .NET API, PostgreSQL
- Frontend: React

### Version 1 (MVP)
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