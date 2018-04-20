# Variables
$ModFolder = "$env:LOCALAPPDATA\Colossal Order\Cities_Skylines\Addons\Mods\Tango"
$BuildFolder = "$env:TargetDir"

Write-Host "Clearing and creating mod folder"

# Delete folder and contents
Remove-Item $ModFolder -Recurse -ErrorAction Ignore

# Make sure the game mod folder exists
New-Item -ItemType directory -Path $ModFolder | Out-Null

Write-Host $env:SolutionDir