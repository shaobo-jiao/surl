# syntax=docker/dockerfile:1

# Extends the official Postgres image so that the built-in "postgres" user also
# belongs to one extra, configurable group. The extra group matches host group 
# and gives postgres read permission to host files like secret files

# Build:
# docker build --build-arg PG_EXTRA_GROUP_ID=2000 -f docker/db.Dockerfile -t shaobojiao/surl-db:latest docker/
FROM postgres:18-bookworm
ARG PG_EXTRA_GROUP_ID

USER root
RUN set -eux; \
	if [ -z "${PG_EXTRA_GROUP_ID}" ]; then \
		echo >&2 'error: PG_EXTRA_GROUP_ID build arg is required'; \
		exit 1; \
    fi; \
    groupadd --gid "${PG_EXTRA_GROUP_ID}" surl-secrets; \
	usermod --append --groups "${PG_EXTRA_GROUP_ID}" postgres; \
    id postgres;
