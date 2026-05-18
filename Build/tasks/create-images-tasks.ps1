task . `
    BuildDotnetSdk80 `
    , BuildDotnetSdk90 `
    , BuildDotnetSdk100 `
    , BuildDotnetRuntime80 `
    , BuildDotnetRuntime90 `
    , BuildDotnetRuntime100 `
    , BuildMsSqlDatabase `
    , BuildPgSqlDatabase `
    , BuildMySqlDatabase

Get-ChildItem -Path (Join-Path $PSScriptRoot '../scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

Enter-Build {
    $context = Join-Path $PSScriptRoot '../../Sources/Docker'
}

task BuildMsSqlDatabase {
    $dockerfile = Join-Path $context 'image-mssql.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/mssql:2025 `
            $context
    }
}

task BuildPgSqlDatabase {
    $dockerfile = Join-Path $context 'image-postgres.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/postgres:18.0 `
            $context
    }
}

task BuildMySqlDatabase {
    $dockerfile = Join-Path $context 'image-mysql.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/mysql:9.4 `
            $context
    }
}

task BuildDotnetSdk80 { Build-DotNetImage -Type 'sdk' -DotNetVersion '8.0' -PwshVersion '7.4.15' }

task BuildDotnetRuntime80 { Build-DotNetImage -Type 'runtime' -DotNetVersion '8.0' -PwshVersion '7.4.15' }

task BuildDotnetSdk90 { Build-DotNetImage -Type 'sdk' -DotNetVersion '9.0' -PwshVersion '7.5.5' }

task BuildDotnetRuntime90 { Build-DotNetImage -Type 'runtime' -DotNetVersion '9.0' -PwshVersion '7.5.5' }

task BuildDotnetSdk100 { Build-DotNetImage -Type 'sdk' -DotNetVersion '10.0' -PwshVersion '7.6.1' }

task BuildDotnetRuntime100 { Build-DotNetImage -Type 'runtime' -DotNetVersion '10.0' -PwshVersion '7.6.1' }