# build image
docker build `
    -f upgrade-database-scripts.dockerfile `
    -t sqldatabase/mssql-server-linux-demo:upgrade `
    .
