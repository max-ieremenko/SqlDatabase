set -e
export PATH="$PATH:/root/.dotnet/tools"
dotnet tool install -g --add-source $app SqlDatabase.GlobalTool --version $packageVersion

echo "----- create new database ---"
SqlDatabase create \
      "-database=$connectionString" \
      -from=$test/New \
      -varJohnCity=London \
      -varMariaCity=Paris \
      -log=/create.log

echo "----- update database ---"
SqlDatabase upgrade \
      "-database=$connectionString" \
      -from=$test/Upgrade \
      -configuration=$test/Upgrade/SqlDatabase.exe.config \
      -varJohnSecondName=Smitt \
      -varMariaSecondName=X \
      -log=/upgrade.log

echo "----- update database (modularity) ---"
SqlDatabase upgrade \
      "-database=$connectionString" \
      -from=$test/UpgradeModularity \
      -configuration=$test/UpgradeModularity/SqlDatabase.exe.config \
      -log=/upgrade.log

echo "----- export data ---"
SqlDatabase export \
      "-database=$connectionString" \
      -from=$test/Export/export.sql \
      -toTable=public.sqldatabase_export1 \
      -log=/export.log

echo "----- execute script ---"
SqlDatabase execute \
      "-database=$connectionString" \
      -from=$test/execute/drop.database.ps1 \
      -log=/execute.log
