function Start-Mssql {
    param ()

    exec { sqllocaldb.exe s MSSQLLocalDB }

    $dataSource = '(localdb)\MSSQLLocalDB'
    $database = 'SqlDatabaseTest'

    $databases = exec { sqlcmd.exe -S $dataSource -l 15 -Q 'select Name from sys.databases' }
    $databaseExists = $databases | Where-Object { $_.Trim() -eq $database }
    if (-not $databaseExists) {
        $file = Join-Path $PSScriptRoot '..\..\..\Sources\Docker\mssql.create-database.sql'
        $file = [System.IO.Path]::GetFullPath($file)
        #Set-Content -Path $file -Value (Get-Content -Path $file -Raw)

        exec { sqlcmd.exe -S $dataSource -i $file }
    }

    @{
        containerId      = ''
        connectionString = "Data Source=$dataSource;Initial Catalog=$database;Integrated Security=true;Connect Timeout=5"
    }
}