FROM mysql:8.0.25

ENV MYSQL_ROOT_PASSWORD=qwerty

COPY mysql.create-database.sql /docker-entrypoint-initdb.d/