task Default BuildDotnetSdk22 `
     , BuildDotnetRuntime22 `
     , BuildDotnetSdk31 `
     , BuildDotnetRuntime31 `
     , BuildDotnetSdk50 `
     , BuildDotnetRuntime50 `
     , BuildDatabase

task BuildDatabase {
    $context = Join-Path $PSScriptRoot "..\Sources\SqlDatabase.Test\Docker"
    exec {
        docker build `
            -f image-mssql-2017.dockerfile `
            -t sqldatabase/mssql:2017 `
            $context
    }
}

task BuildDotnetSdk22 {
    exec {
        docker build `
            -f image-dotnet-sdk-2.2.dockerfile `
            -t sqldatabase/dotnet_pwsh:2.2-sdk `
            .
    }
}

task BuildDotnetRuntime22 {
    exec {
        docker build `
            -f image-dotnet-runtime-2.2.dockerfile `
            -t sqldatabase/dotnet_pwsh:2.2-runtime `
            .
    }
}

task BuildDotnetSdk31 {
    exec {
        docker build `
            -f image-dotnet-sdk-3.1.dockerfile `
            -t sqldatabase/dotnet_pwsh:3.1-sdk `
            .
    }
}

task BuildDotnetRuntime31 {
    exec {
        docker build `
            -f image-dotnet-runtime-3.1.dockerfile `
            -t sqldatabase/dotnet_pwsh:3.1-runtime `
            .
    }
}

task BuildDotnetSdk50 {
    exec {
        docker build `
            -f image-dotnet-sdk-5.0.dockerfile `
            -t sqldatabase/dotnet_pwsh:5.0-sdk `
            .
    }
}

task BuildDotnetRuntime50 {
    exec {
        docker build `
            -f image-dotnet-runtime-5.0.dockerfile `
            -t sqldatabase/dotnet_pwsh:5.0-runtime `
            .
    }
}