function Start-Container {
    param (
        [Parameter(Mandatory)]
        [string]
        $Image,

        [Parameter(Mandatory)]
        [int]
        $ContainerPort
    )
    
    [string]$containerId = exec { 
        docker run `
            -d `
            -p $ContainerPort `
            $Image
    }
    
    [string]$ip = exec { 
        docker inspect `
            --format '{{.NetworkSettings.Networks.bridge.IPAddress}}'  `
            $containerId
    }

    [int]$hostPort = exec { 
        docker inspect `
            --format "{{(index (index .NetworkSettings.Ports \""$ContainerPort/tcp\"") 0).HostPort}}"  `
            $containerId
    }

    @{
        containerId = $containerId
        ip          = $ip
        port        = $hostPort
    }
}