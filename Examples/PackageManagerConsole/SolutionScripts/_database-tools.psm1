function Get-ConnectionStringFomProjectConfiguration($projectName)
{
	$project = Get-Project $projectName
	$appConfig = $project.ProjectItems.Item("App.config")

	$xml = New-Object -TypeName xml
	$xml.Load($appConfig.FileNames(0))

	$connectionStringNode = $xml.SelectSingleNode("/configuration/connectionStrings/add[@name = 'default']")

	return $connectionStringNode.Attributes["connectionString"].Value
}

function Get-ProjectFilePath($projectName, $fileName)
{
	$project = Get-Project $projectName

	$location = Split-Path $project.FileName -Parent
	$location = Join-Path $location $fileName

	if (Test-Path $location)
	{
		return $location
	}

	throw "$fileName not found in $projectName"
}

function Find-ProjectNames
{
    return Get-Project -All | %{ $_.ProjectName }
}
