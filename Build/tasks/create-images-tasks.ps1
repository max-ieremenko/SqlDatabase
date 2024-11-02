task . `
    BuildDotnetSdk60 `
    , BuildDotnetSdk70 `
    , BuildDotnetSdk80 `
    , BuildDotnetRuntime60 `
    , BuildDotnetRuntime70 `
    , BuildDotnetRuntime80 `
    , BuildMsSqlDatabase `
    , BuildPgSqlDatabase `
    , BuildMySqlDatabase

Enter-Build {
    $context = Join-Path $PSScriptRoot '..\..\Sources\Docker'
}

task BuildMsSqlDatabase {
    $dockerfile = Join-Path $context 'image-mssql-2017.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/mssql:2017 `
            $context
    }
}

task BuildPgSqlDatabase {
    $dockerfile = Join-Path $context 'image-postgres-133.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/postgres:13.3 `
            $context
    }
}

task BuildMySqlDatabase {
    $dockerfile = Join-Path $context 'image-mysql-8025.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/mysql:8.0.25 `
            $context
    }
}

task BuildDotnetSdk60 {
    $dockerfile = Join-Path $context 'image-dotnet-sdk-6.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:6.0-sdk `
            .
    }
}

task BuildDotnetRuntime60 {
    $dockerfile = Join-Path $context 'image-dotnet-runtime-6.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:6.0-runtime `
            .
    }
}

task BuildDotnetSdk70 {
    $dockerfile = Join-Path $context 'image-dotnet-sdk-7.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:7.0-sdk `
            .
    }
}

task BuildDotnetRuntime70 {
    $dockerfile = Join-Path $context 'image-dotnet-runtime-7.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:7.0-runtime `
            .
    }
}

task BuildDotnetSdk80 {
    $dockerfile = Join-Path $context 'image-dotnet-sdk-8.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:8.0-sdk `
            .
    }
}

task BuildDotnetRuntime80 {
    $dockerfile = Join-Path $context 'image-dotnet-runtime-8.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:8.0-runtime `
            .
    }
}