function Start-Mysql {
    param ()
    
    $port = 3306

    if (-not (Test-Connection -TargetName localhost -TcpPort $port -ErrorAction Continue)) {
        exec { mysqld.exe --console --initialize }

        $file = Join-Path $PSScriptRoot mysql.init.sql
        exec { mysqld.exe --console --init-file=$file & }

        for ($i = 0; $i -lt 3; $i++) {
            if (Test-Connection -TargetName localhost -TcpPort $port -ErrorAction Continue) {
                break
            }
    
            Start-Sleep 2
        }
    
        $file = Join-Path $PSScriptRoot '..\..\..\Sources\Docker\mysql.create-database.sql'
        $file = [System.IO.Path]::GetFullPath($file)
        Set-Content -Path $file -Value (Get-Content -Path $file -Raw)

        exec { mysql.exe -uroot -proot -e "source $file" }
    }

    return @{
        containerId      = ''
        connectionString = "Server=localhost;Port=$port;Database=sqldatabasetest;User ID=root;Password=root;Connection Timeout=5;"
    }
}