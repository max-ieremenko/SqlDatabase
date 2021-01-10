@{
	RootModule = "SqlDatabase"

	ModuleVersion = "{{ModuleVersion}}"
	GUID = "f19e643e-f998-4767-b2f9-958daf96137b"

	Author = "Max Ieremenko"
	Copyright = "(C) 2018-2021 Max Ieremenko, licensed under MIT License"

	Description = "This a module for SQL Server, allows to execute scripts, database migrations and export data."

	# for the PowerShell Desktop edition only.
	DotNetFrameworkVersion = '4.5.2'

	PowerShellVersion = '5.1'
	CompatiblePSEditions = @('Desktop', 'Core')
	CLRVersion = '4.0'
	ProcessorArchitecture = 'None'

	CmdletsToExport = (
		"New-SqlDatabase",
		"Invoke-SqlDatabase",
		"Update-SqlDatabase",
		"Export-SqlDatabase",
		"Show-SqlDatabaseInfo"
	)

	AliasesToExport = @("Create-SqlDatabase", "Execute-SqlDatabase", "Upgrade-SqlDatabase")

	PrivateData = @{
		PSData = @{
			Tags = 'sql', 'SqlServer', 'sqlcmd', 'migration-tool', 'miration-step', 'sql-script', 'sql-database', 'database-migrations', 'export-data', 'PSEdition_Core', 'PSEdition_Desktop', 'Windows', 'Linux', 'macOS'
			LicenseUri = 'https://github.com/max-ieremenko/SqlDatabase/blob/master/LICENSE'
			ProjectUri = 'https://github.com/max-ieremenko/SqlDatabase'
			IconUri = 'https://raw.githubusercontent.com/max-ieremenko/SqlDatabase/master/icon-32.png'
			ReleaseNotes = 'https://github.com/max-ieremenko/SqlDatabase/releases'
		}
	 }
}