function Start-Pgsql {
    param ()
    
    $port = 5432

    if (-not (Test-Connection -TargetName localhost -TcpPort $port -ErrorAction Continue)) {
        Set-Service 'postgresql-x64-14' -StartupType Manual
        Start-Service 'postgresql-x64-14'

        for ($i = 0; $i -lt 3; $i++) {
            if (Test-Connection -TargetName localhost -TcpPort $port -ErrorAction Continue) {
                break
            }
    
            Start-Sleep 2
        }

        $file = Join-Path $PSScriptRoot '..\..\..\Sources\Docker\pgsql.create-database.sql'
        $file = [System.IO.Path]::GetFullPath($file)
        Set-Content -Path $file -Value (Get-Content -Path $file -Raw)

        $env:PGPASSWORD = 'root'

        $psql = Join-Path $env:PGBIN 'psql.exe'
        exec { & $psql -U postgres -a -f $file }
    }

    @{
        containerId      = ''
        connectionString = "Host=localhost;Port=$port;Database=sqldatabasetest;Username=postgres;Password=root;Timeout=5;"
    }
}