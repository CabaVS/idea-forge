[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SubscriptionId,

    [Parameter(Mandatory = $true)]
    [string] $TenantId,

    [string] $Location = "westeurope",
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
    throw "Failed to swithch subscription to '$SubscriptionId'"
}

$ResourceGroupName = "rg-cabavsideaforge"
if (-not [string]::IsNullOrWhiteSpace($Environment)) {
    $ResourceGroupName = "$ResourceGroupName-$Environment"
}

Write-Host "-> Creating resource group: $ResourceGroupName in $Location"
az group create `
  --name $ResourceGroupName `
  --location $Location `
  --output none

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create resource group '$ResourceGroupName'"
}

$StorageAccountName = "stcabavsideaforge"
if (-not [string]::IsNullOrWhiteSpace($Environment)) {
    $StorageAccountName = "$StorageAccountName$Environment"
}

Write-Host "-> Creating storage account: $StorageAccountName"
az storage account create `
  --name $StorageAccountName `
  --resource-group $ResourceGroupName `
  --location $Location `
  --sku Standard_LRS `
  --kind StorageV2 `
  --allow-blob-public-access false `
  --min-tls-version TLS1_2 `
  --https-only true `
  --only-show-errors `
  --output none

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create storage account '$StorageAccountName'"
}
