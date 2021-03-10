Task default -Depends BuildDotnetSdk22 `
     , BuildDotnetRuntime22 `
     , BuildDotnetSdk31 `
     , BuildDotnetRuntime31 `
     , BuildDotnetSdk50 `
     , BuildDotnetRuntime50

Task BuildDotnetSdk22 {
    Exec {
        docker build `
            -f dotnet-sdk-2.2.dockerfile `
            -t sqldatabase/dotnet_pwsh:2.2-sdk `
            .
    }
}

Task BuildDotnetRuntime22 {
    Exec {
        docker build `
            -f dotnet-runtime-2.2.dockerfile `
            -t sqldatabase/dotnet_pwsh:2.2-runtime `
            .
    }
}

Task BuildDotnetSdk31 {
    Exec {
        docker build `
            -f dotnet-sdk-3.1.dockerfile `
            -t sqldatabase/dotnet_pwsh:3.1-sdk `
            .
    }
}

Task BuildDotnetRuntime31 {
    Exec {
        docker build `
            -f dotnet-runtime-3.1.dockerfile `
            -t sqldatabase/dotnet_pwsh:3.1-runtime `
            .
    }
}

Task BuildDotnetSdk50 {
    Exec {
        docker build `
            -f dotnet-sdk-5.0.dockerfile `
            -t sqldatabase/dotnet_pwsh:5.0-sdk `
            .
    }
}

Task BuildDotnetRuntime50 {
    Exec {
        docker build `
            -f dotnet-runtime-5.0.dockerfile `
            -t sqldatabase/dotnet_pwsh:5.0-runtime `
            .
    }
}
