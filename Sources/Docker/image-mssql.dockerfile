FROM mcr.microsoft.com/mssql/server:2025-latest AS build

ENV ACCEPT_EULA=Y \
   SA_PASSWORD=P@ssw0rd \
   MSSQL_PID=Express

COPY mssql.create-database.sql /app/

RUN /opt/mssql/bin/sqlservr & \
   sleep 20 && \
   /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P P@ssw0rd -l 300 -C -i /app/mssql.create-database.sql && \
   ls /var/opt/mssql/data/ && \
   pkill sqlservr

FROM mcr.microsoft.com/mssql/server:2025-latest AS runtime

ENV ACCEPT_EULA=Y \
   SA_PASSWORD=P@ssw0rd \
   MSSQL_PID=Express

COPY --chown=mssql:mssql --from=build /var/opt/mssql/data/* /var/opt/mssql/data/