set -e
echo "----- create new database ---"
dotnet SqlDatabase.dll create \
      "-database=$connectionString" \
      -from=$test/New \
      -varJohnCity=London \
      -varMariaCity=Paris \
      -log=/create.log

echo "----- update database ---"
dotnet SqlDatabase.dll upgrade \
      "-database=$connectionString" \
      -from=$test/Upgrade \
      -configuration=$test/Upgrade/SqlDatabase.exe.config \
      -varJohnSecondName=Smitt \
      -varMariaSecondName=X \
      -log=/upgrade.log

echo "----- update database (modularity) ---"
dotnet SqlDatabase.dll upgrade \
      "-database=$connectionString" \
      -from=$test/UpgradeModularity \
      -configuration=$test/UpgradeModularity/SqlDatabase.exe.config \
      -log=/upgrade.log

echo "----- export data ---"
dotnet SqlDatabase.dll export \
      "-database=$connectionString" \
      -from=$test/Export/export.sql \
      -toTable=public.sqldatabase_export1 \
      -log=/export.log

echo "----- execute script ---"
dotnet SqlDatabase.dll execute \
      "-database=$connectionString" \
      -from=$test/execute/drop.database.ps1 \
      -log=/execute.log
