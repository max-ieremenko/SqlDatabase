Some tests require SqlServer with already existing database.

You can use docker:
```bash
$ cd .\SqlDatabase\Sources\SqlDatabase.Test\Docker
$ docker-compose up
```
If database is not created during container start-up, please increase sleep timeout in entrypoint.sh. 


Or create database manually on your SqlServer:
1. Docker\CreateDatabase.sql
2. Check connection string in app.config
