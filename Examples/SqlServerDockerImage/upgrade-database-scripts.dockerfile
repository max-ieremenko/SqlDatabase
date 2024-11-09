# take image with [SqlDatabaseDemo] version 1.0
FROM sqldatabase/mssql-server-linux-demo:create AS build

# copy scripts
COPY upgrade-database-scripts/ /sql-scripts/

# switch to root
USER root

# install .net 8.0 sdk
RUN apt-get update && \
   apt-get install -y apt-transport-https && \
   apt-get update && \
   apt-get install -y dotnet-sdk-8.0

# install SqlDatabase.GlobalTool
RUN dotnet tool install --global SqlDatabase.GlobalTool

# upgrade [SqlDatabaseDemo] database to version 2.0 from sql-scripts
# 1. start mssql server in the background and wait 20 secs
# 2. upgrade database
# 3. shatdown mssql server
RUN /opt/mssql/bin/sqlservr & \
   export PATH="$PATH:/root/.dotnet/tools" && \
   SqlDatabase upgrade  \
      "-database=Data Source=.;Initial Catalog=SqlDatabaseDemo;User Id=sa;Password=P@ssw0rd;ConnectRetryCount=20;ConnectRetryInterval=1"  \
      -from=/sql-scripts && \
   pkill sqlservr

# set mssql user as SqlServer files owner
RUN chown -R mssql /var/opt/mssql/data 

FROM mcr.microsoft.com/mssql/server:latest AS runtime

ENV ACCEPT_EULA=Y \
   SA_PASSWORD=P@ssw0rd \
   MSSQL_PID=Express

# copy mssql server content
COPY --from=build /var/opt/mssql/data/* /var/opt/mssql/data/