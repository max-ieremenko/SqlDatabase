function Get-CurentEnvironment {
    param ()
    
    # github actions environment
    if ($env:GITHUB_ACTIONS -eq 'true') {
        'github'
    }
    else {
        'local'
    }
}