function Wait-Connection {
    param (
        [Parameter(Mandatory)]
        [string]
        $ConnectionName,

        [Parameter(Mandatory)]
        [string]
        $ConnectionString,

        [Parameter()]
        [int]
        $Timeout = 50
    )
 
    function Test-Connection {
        $connection = New-Object -TypeName $ConnectionName -ArgumentList $ConnectionString
        try {
            $connection.Open()
        }
        finally {
            $connection.Dispose()
        }
    }

    for ($i = 0; $i -lt $timeout; $i++) {
        try {
            Test-Connection
            return
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    try {
        Test-Connection
    }
    catch {
        throw "$ConnectionName $ConnectionString"
    }    
}