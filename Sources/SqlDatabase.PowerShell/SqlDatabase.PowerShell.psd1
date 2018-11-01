@{
	RootModule = "SqlDatabase.PowerShell"

	ModuleVersion = "{{ModuleVersion}}"
	GUID = "f19e643e-f998-4767-b2f9-958daf96137b"

	Author = "Max Ieremenko"
	Copyright = "© 2018 Maksym Ieremenko. All rights reserved."

	CmdletsToExport = (
		"Create-SqlDatabase",
		"Execute-SqlDatabase",
		"Upgrade-SqlDatabase",
	)
}

