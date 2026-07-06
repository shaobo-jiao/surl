# Database Initialization Script:
# This script sets up the database and roles for the application.
# This script is run when initializing the database. 
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- create database
    CREATE DATABASE surl;

    -- create roles
    CREATE ROLE surl_owner WITH LOGIN PASSWORD '${SURL_OWNER_PASSWORD}';
    CREATE ROLE surl_rw WITH LOGIN PASSWORD '${SURL_RW_PASSWORD}';

    -- allow connection
    GRANT CONNECT ON DATABASE surl TO surl_owner;
    GRANT CONNECT ON DATABASE surl TO surl_rw;

    -- switch database
    \c surl

    -- create schema
    CREATE SCHEMA surl AUTHORIZATION surl_owner;
    GRANT USAGE ON SCHEMA surl TO surl_rw;

    -- alter search_path
    ALTER ROLE surl_owner IN DATABASE surl SET search_path = surl;
    ALTER ROLE surl_rw IN DATABASE surl SET search_path = surl;

    -- alter privileges
    ALTER DEFAULT PRIVILEGES FOR ROLE surl_owner IN SCHEMA surl
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO surl_rw;

    ALTER DEFAULT PRIVILEGES FOR ROLE surl_owner IN SCHEMA surl
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO surl_rw;
EOSQL

# an example of running postgres with above init script; later move to a better place; later use docker compose
# docker run -d --name=postgres -p 5433:5432 \
#     --mount type=volume,src=postgres_vol,dst=/var/lib/postgresql \
#     --mount type=bind,src=./db_init.sh,dst=/docker-entrypoint-initdb.d/db_init.sh,ro \
#     -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres \
#     -e SURL_OWNER_PASSWORD=1 -e SURL_RW_PASSWORD=2 \
#     postgres:18-bookworm