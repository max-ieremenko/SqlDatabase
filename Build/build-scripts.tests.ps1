#Install-Module -Name Pester
#Requires -Modules @{ModuleName='Pester'; RequiredVersion='4.9.0'}

. .\build-scripts.ps1

Describe "Get-AssemblyVersion" {
    $testCases = @{ version = "1.0.2.0"; expected = "1.0.2" } `
        ,@{ version = "1.0.2.1"; expected = "1.0.2" } `
        ,@{ version = "1.0.2"; expected = "1.0.2" } `
        ,@{ version = "1.0.0"; expected = "1.0.0" } `
        ,@{ version = "1.0"; expected = "1.0.0" }

    It "Extract version from <version>" -TestCases $testCases {
        param ($version, $expected)

        $content = "[assembly: AssemblyVersion(""" + $version + """)]"

        Mock -CommandName Get-Content `
            -MockWith { return @($content) } `
            -ParameterFilter { $Path -eq "AssemblyInfo.cs" }
        
        $actual = Get-AssemblyVersion "AssemblyInfo.cs"
        
        $actual | Should -Be $expected
    }
}