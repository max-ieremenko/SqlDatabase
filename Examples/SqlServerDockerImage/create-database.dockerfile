FROM microsoft/mssql-server-linux:latest AS build

ENV ACCEPT_EULA=Y \
   SA_PASSWORD=P@ssw0rd \
   MSSQL_PID=Express

# copy scripts
COPY create-database-scripts/ /sql-scripts/

# install .net 5.0 sdk
RUN apt-get update && \
   apt-get install -y apt-transport-https && \
   apt-get update && \
   apt-get install -y dotnet-sdk-5.0

# install SqlDatabase.GlobalTool
RUN dotnet tool install --global SqlDatabase.GlobalTool

# create [SqlDatabaseDemo] database from sql-scripts
# 1. start mssql server in the background and wait 20 secs
# 2. create database
# 3. shatdown mssql server
RUN /opt/mssql/bin/sqlservr & \
   export PATH="$PATH:/root/.dotnet/tools" && \
   SqlDatabase create  \
      "-database=Data Source=.;Initial Catalog=SqlDatabaseDemo;User Id=sa;Password=P@ssw0rd;ConnectRetryCount=20;ConnectRetryInterval=1"  \
      -from=/sql-scripts && \
   pkill sqlservr

FROM microsoft/mssql-server-linux:latest AS runtime

ENV ACCEPT_EULA=Y \
   SA_PASSWORD=P@ssw0rd \
   MSSQL_PID=Express

# copy mssql server content
COPY --from=build /var/opt/mssql/data/* /var/opt/mssql/data/