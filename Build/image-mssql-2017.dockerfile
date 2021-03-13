FROM mcr.microsoft.com/mssql/server:2017-latest AS build

ENV ACCEPT_EULA=Y \
   SA_PASSWORD=P@ssw0rd \
   MSSQL_PID=Express

COPY CreateDatabase.sql /db/

RUN /opt/mssql/bin/sqlservr & \
   sleep 20 && \
   /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@ssw0rd -l 300 -i /db/CreateDatabase.sql && \
   ls /var/opt/mssql/data/ && \
   pkill sqlservr

FROM mcr.microsoft.com/mssql/server:2017-latest AS runtime

ENV ACCEPT_EULA=Y \
   SA_PASSWORD=P@ssw0rd \
   MSSQL_PID=Express

COPY --from=build /var/opt/mssql/data/* /var/opt/mssql/data/