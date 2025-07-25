param (
    [string]$Folder = "."
)

$folderPath = Resolve-Path -Path $Folder

$tfvarsFiles = Get-ChildItem -Path $folderPath -Filter *.tfvars -Recurse

foreach ($file in $tfvarsFiles) {
    Write-Output $file.Name
    $contentBytes = [System.IO.File]::ReadAllBytes($file.FullName)
    [Convert]::ToBase64String($contentBytes)
    Write-Output ""
}