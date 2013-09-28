param($installPath, $toolsPath, $package, $project)

$vsSolution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])
$vsProject = $vsSolution.AddSolutionFolder("Database")
$projectItems = Get-Interface $vsProject.ProjectItems ([EnvDTE.ProjectItems])
$projectItems.AddFromFile("pathToFileToAdd.txt")