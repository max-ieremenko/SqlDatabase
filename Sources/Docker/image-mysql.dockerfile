FROM mysql:9.4

ENV MYSQL_ROOT_PASSWORD=qwerty

COPY mysql.create-database.sql /docker-entrypoint-initdb.d/