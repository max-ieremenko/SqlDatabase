# wait for SQL Server to come up
sleep 10

# create database
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@ssw0rd -i ./CreateDatabase.sql