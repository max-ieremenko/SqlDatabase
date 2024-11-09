function Start-Container {
    param (
        [Parameter(Mandatory)]
        [string]
        $Image,

        [Parameter(Mandatory)]
        [int]
        $ContainerPort
    )
    
    $containerId = exec { 
        docker run `
            -d `
            -p $ContainerPort `
            $Image
    }
    
    $ip = exec { 
        docker inspect `
            --format '{{.NetworkSettings.Networks.bridge.IPAddress}}'  `
            $containerId
    }

    $hostPort = exec { 
        docker inspect `
            --format "{{(index (index .NetworkSettings.Ports ""$ContainerPort/tcp"") 0).HostPort}}"  `
            $containerId
    }

    return @{
        containerId = $containerId
        ip          = $ip
        port        = $hostPort
    }
}