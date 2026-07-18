# Database Initialization Script:
# This script sets up the database and roles for the application.
# This script is run when initializing the database. 
set -e

SURL_OWNER_PASSWORD="$(cat /run/secrets/surl-owner-password)"
SURL_RW_PASSWORD="$(cat /run/secrets/surl-rw-password)"

psql --username "$POSTGRES_USER" \
    --dbname "$POSTGRES_DB" \
    --set ON_ERROR_STOP=1 \
    --set surl_owner_password="$SURL_OWNER_PASSWORD" \
    --set surl_rw_password="$SURL_RW_PASSWORD" <<-EOSQL
    -- create database
    CREATE DATABASE surl;

    -- create roles
    CREATE ROLE surl_owner WITH LOGIN PASSWORD :'surl_owner_password';
    CREATE ROLE surl_rw WITH LOGIN PASSWORD :'surl_rw_password';

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
