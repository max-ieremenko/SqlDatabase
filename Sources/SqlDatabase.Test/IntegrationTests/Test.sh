echo "----- create new database ---"
dotnet SqlDatabase.dll create \
      "-database=$connectionString" \
      -from=$test/New \
      -varJohnCity=London \
      -varMariaCity=Paris

echo "----- update database ---"
dotnet SqlDatabase.dll upgrade \
      "-database=$connectionString" \
      -from=$test/Upgrade \
      -varJohnSecondName=Smitt \
      -varMariaSecondName=X

echo "----- update database (modularity) ---"
dotnet SqlDatabase.dll upgrade \
      "-database=$connectionString" \
      -from=$test/UpgradeModularity \
      -configuration=$test/UpgradeModularity/SqlDatabase.exe.config

echo "----- export data ---"
dotnet SqlDatabase.dll export \
      "-database=$connectionString" \
      -from=$test/Export/export.sql \
      -toTable=dbo.ExportedData1

echo "----- execute script ---"
dotnet SqlDatabase.dll execute \
      "-database=$connectionString" \
      -from=$test/execute/drop.database.sql
