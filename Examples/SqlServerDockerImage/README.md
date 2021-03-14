Two steps example how to create and upgrade database in ms sql server linux container
=================

To run scripts switch your docker to linux containers.

## Step 1: create an image with sql server and SqlDatabaseDemo database version 1.0.

Database scripts are located in `create-database-scripts` folder.

Build image `sqldatabase/mssql-server-linux-demo:create`:

```powershell
PS> .\create-database-build-image.ps1
```

Test image:

```powershell
PS> docker run -it --rm -p 1433:1433 sqldatabase/mssql-server-linux-demo:create
```

## Step 2: create an image with sql server and SqlDatabaseDemo database version 2.0.

During the build process we update SqlDatabaseDemo database from step 1 to version 2.0.
Database scripts are located in `upgrade-database-scripts` folder.

Build image `sqldatabase/mssql-server-linux-demo:upgrade`:

```powershell
PS> .\upgrade-database-build-image.ps1
```

Test image:

```powershell
PS> docker run -it --rm -p 1433:1433 sqldatabase/mssql-server-linux-demo:upgrade
```
