param($installPath, $toolsPath, $package, $project)

function HasStartAction ($item)
{
    foreach ($property in $item.Properties)
    {
       if ($property.Name -eq "StartAction")
       {
           return $true
       }            
    } 

    return $false
}

function ModifyConfigurations
{
    $configurationManager = $project.ConfigurationManager
	$path = [System.IO.Path]
    $projectAbsolutePath = $path::GetDirectoryName($project.FileName)
	$startUrl = "http://localhost:71710"
	$outputFile = $path::GetFileNameWithoutExtension($project.FileName) + ".dll"

    foreach ($name in $configurationManager.ConfigurationRowNames)
    {
        $projectConfigurations = $configurationManager.ConfigurationRow($name)

        foreach ($projectConfiguration in $projectConfigurations)
        {                

            if (HasStartAction $projectConfiguration)
            {
                $newStartAction = 1
                $newWorkingDirectory = $path::Combine($projectAbsolutePath, "bin\" + $name)
                $newStartProgram = $path::Combine($newWorkingDirectory, "xss.exe")
				$newArguments = "http://localhost:1080 " + '"' + $outputFile + '"' + " ..\..\client\app"

                write-host "StartAction - " $newStartAction
                write-host "StartProgram - " $newStartProgram
                write-host "StartArguments - " $newArguments

                $projectConfiguration.Properties.Item("StartAction").Value = $newStartAction
                $projectConfiguration.Properties.Item("StartProgram").Value = $newStartProgram
                $projectConfiguration.Properties.Item("StartWorkingDirectory").Value = $newWorkingDirectory
                $projectConfiguration.Properties.Item("StartArguments").Value = $newArguments
                $projectConfiguration.Properties.Item("StartUrl").Value = $startUrl
            }
        }
    }

    $project.Save
}

write-host "Modifying Configurations..."
ModifyConfigurations