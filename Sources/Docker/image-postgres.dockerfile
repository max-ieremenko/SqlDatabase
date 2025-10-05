FROM postgres:18.0-alpine

ENV POSTGRES_PASSWORD=qwerty

COPY pgsql.create-database.sql /docker-entrypoint-initdb.d/