# wait for SQL Server to come up
sleep 5s

# create database
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@ssw0rd -l 60 -i ./mssql.create-database.sql