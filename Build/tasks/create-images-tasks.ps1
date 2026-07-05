task . Dotnet, Database, Pwsh

task Dotnet DotnetSdk80, DotnetRuntime80, DotnetSdk90, DotnetRuntime90, DotnetSdk100, DotnetRuntime100
task Database MsSqlDatabase, PgSqlDatabase, MySqlDatabase
task Pwsh Pwsh720, Pwsh730, Pwsh740, Pwsh74Latest, Pwsh750, Pwsh75Latest, Pwsh760, Pwsh76Latest, Pwsh770

Get-ChildItem -Path (Join-Path $PSScriptRoot '../scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

Enter-Build {
    $context = Join-Path $PSScriptRoot '../../Sources/Docker'
}

task MsSqlDatabase {
    $dockerfile = Join-Path $context 'image-mssql.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/mssql:2025 `
            $context
    }
}

task PgSqlDatabase {
    $dockerfile = Join-Path $context 'image-postgres.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/postgres:18.0 `
            $context
    }
}

task MySqlDatabase {
    $dockerfile = Join-Path $context 'image-mysql.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/mysql:9.4 `
            $context
    }
}

task DotnetSdk80 { Build-DotNetImage -Type 'sdk' -DotNetVersion '8.0' -PwshVersion '7.4.17' }
task DotnetRuntime80 { Build-DotNetImage -Type 'runtime' -DotNetVersion '8.0' -PwshVersion '7.4.17' }

task DotnetSdk90 { Build-DotNetImage -Type 'sdk' -DotNetVersion '9.0' -PwshVersion '7.5.8' }
task DotnetRuntime90 { Build-DotNetImage -Type 'runtime' -DotNetVersion '9.0' -PwshVersion '7.5.8' }

task DotnetSdk100 { Build-DotNetImage -Type 'sdk' -DotNetVersion '10.0' -PwshVersion '7.6.3' }
task DotnetRuntime100 { Build-DotNetImage -Type 'runtime' -DotNetVersion '10.0' -PwshVersion '7.6.3' }

task Pwsh720 { Build-PwshImage -PwshVersion '7.2.0' -OsVersion '20.04' }
task Pwsh730 { Build-PwshImage -PwshVersion '7.3.0' -OsVersion '22.04' }
task Pwsh740 { Build-PwshImage -PwshVersion '7.4.0' -OsVersion '22.04' }
task Pwsh74Latest { Build-PwshImage -PwshVersion '7.4.17' -OsVersion '22.04' }
task Pwsh750 { Build-PwshImage -PwshVersion '7.5.0' -OsVersion '24.04' }
task Pwsh75Latest { Build-PwshImage -PwshVersion '7.5.8' -OsVersion '24.04' }
task Pwsh760 { Build-PwshImage -PwshVersion '7.6.0' -OsVersion '24.04' }
task Pwsh76Latest { Build-PwshImage -PwshVersion '7.6.3' -OsVersion '24.04' }
task Pwsh770 { Build-PwshImage -PwshVersion '7.7.0-preview.2' -OsVersion '24.04' }