FROM postgres:13.3-alpine

ENV POSTGRES_PASSWORD=qwerty

COPY pgsql.create-database.sql /docker-entrypoint-initdb.d/