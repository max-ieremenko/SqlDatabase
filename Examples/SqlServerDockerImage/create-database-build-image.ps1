# build image
docker build `
    -f create-database.dockerfile `
    -t sqldatabase/mssql-server-linux-demo:create `
    .
