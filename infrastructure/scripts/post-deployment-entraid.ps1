[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SubscriptionId,

    [Parameter(Mandatory = $true)]
    [string] $TenantId,

    [Parameter(Mandatory = $false)]
    [string] $Environment
)

$ErrorActionPreference = 'Stop'

Write-Host "-> Login to Azure account"
az login --tenant $TenantId

if ($LASTEXITCODE -ne 0) {
    throw "Failed to login to Azure account"
}

Write-Host "-> Switching to subscription: $SubscriptionId"
az account set --subscription $SubscriptionId

if ($LASTEXITCODE -ne 0) {
    throw "Failed to switch subscription to '$SubscriptionId'"
}

$ResourceGroupName = "rg-cabavsideaforge"
if (-not [string]::IsNullOrWhiteSpace($Environment)) {
    $ResourceGroupName = "$ResourceGroupName-$Environment"
}

$AppName = "gh-actions-terraform"
if (-not [string]::IsNullOrWhiteSpace($Environment)) {
    $AppName = "$AppName-$Environment"
}

Write-Host "-> Acquiring App Client ID: $AppName"
$AppClientId = az ad app list --display-name $AppName --query "[0].appId" --output tsv

if ($LASTEXITCODE -ne 0) {
    throw "Failed to acquire App Client ID for '$AppName'"
}

Write-Host "-> Assigning role 'AcrPush': $AppClientId"
$AcrName = az acr list --resource-group $ResourceGroupName --query "[0].name" --output tsv

az role assignment create `
    --assignee $AppClientId `
    --role "AcrPush" `
    --scope "/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.ContainerRegistry/registries/$AcrName" | Out-Null

if ($LASTEXITCODE -ne 0) {
    throw "Failed to assign the role 'AcrPush'"
}

Write-Host "-> Assigning role 'Storage Blob Data Contributor': $AppClientId"
$StorageAccountName = az storage account list --resource-group $ResourceGroupName --query "[?contains(name, 'func')].name | [0]" --output tsv

az role assignment create `
    --assignee $AppClientId `
    --role "Storage Blob Data Contributor" `
    --scope "/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$StorageAccountName" | Out-Null

if ($LASTEXITCODE -ne 0) {
    throw "Failed to assign the role 'Storage Blob Data Contributor'"
}