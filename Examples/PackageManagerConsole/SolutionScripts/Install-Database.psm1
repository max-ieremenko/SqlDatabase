function Install-Database
{
	[CmdletBinding()]
	param(
		[string]$ConfigurationFrom = "SomeApp",
		[string]$DatabaseName = "" # by default dabase name from connection string
	)

	begin
	{
		$ErrorActionPreference = "Stop"
	}

	process
	{
		$connectionString = Get-ConnectionStringFomProjectConfiguration $ConfigurationFrom
		$sqlDatabaseConfig = Get-ProjectFilePath "SolutionScripts" "SqlDatabase.config"

		$connectionStringBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder -ArgumentList $connectionString
		if ($DatabaseName)
		{
			$connectionStringBuilder["Initial Catalog"] = $DatabaseName
		}
		else
		{
			$DatabaseName = $connectionStringBuilder["Initial Catalog"]
		}

		$connectionString = $connectionStringBuilder.ToString()
		Write-Host "Connection string: $connectionString"

		$from = Get-ProjectFilePath "SolutionScripts" "..\..\CreateDatabaseFolder"

		Create-SqlDatabase -Database $connectionString `
			-From $from `
			-Configuration $sqlDatabaseConfig `
			-InformationAction Continue
	}
}

Register-TabExpansion Install-Database @{
    ConfigurationFrom = { Find-ProjectNames }
    DatabaseName = { <# Disabled #> }
}

Export-ModuleMember Install-Database
