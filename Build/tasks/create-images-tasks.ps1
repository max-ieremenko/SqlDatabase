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

Enter-Build {
    $context = Join-Path $PSScriptRoot '..\..\Sources\Docker'
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
    $dockerfile = Join-Path $context 'image-mysql.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/mysql:9.4 `
            $context
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

task BuildDotnetSdk90 {
    $dockerfile = Join-Path $context 'image-dotnet-sdk-9.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:9.0-sdk `
            .
    }
}

task BuildDotnetRuntime90 {
    $dockerfile = Join-Path $context 'image-dotnet-runtime-9.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:9.0-runtime `
            .
    }
}

task BuildDotnetSdk100 {
    $dockerfile = Join-Path $context 'image-dotnet-sdk-10.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:10.0-sdk `
            .
    }
}

task BuildDotnetRuntime100 {
    $dockerfile = Join-Path $context 'image-dotnet-runtime-10.0.dockerfile'
    exec {
        docker build `
            --pull `
            -f $dockerfile `
            -t sqldatabase/dotnet_pwsh:10.0-runtime `
            .
    }
}