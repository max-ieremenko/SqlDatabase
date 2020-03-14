export PATH="$PATH:/root/.dotnet/tools"
dotnet tool install -g --add-source $app SqlDatabase.GlobalTool --version $packageVersion

echo "----- create new database ---"
SqlDatabase create \
      "-database=$connectionString" \
      -from=$test/New \
      -varJohnCity=London \
      -varMariaCity=Paris

echo "----- update database ---"
SqlDatabase upgrade \
      "-database=$connectionString" \
      -from=$test/Upgrade \
      -varJohnSecondName=Smitt \
      -varMariaSecondName=X

echo "----- update database (modularity) ---"
SqlDatabase upgrade \
      "-database=$connectionString" \
      -from=$test/UpgradeModularity \
      -configuration=$test/UpgradeModularity/SqlDatabase.exe.config

echo "----- export data ---"
SqlDatabase export \
      "-database=$connectionString" \
      -from=$test/Export/export.sql \
      -toTable=dbo.ExportedData1

echo "----- execute script ---"
SqlDatabase execute \
      "-database=$connectionString" \
      -from=$test/execute/drop.database.sql
