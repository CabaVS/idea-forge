[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SubscriptionId,

    [Parameter(Mandatory = $true)]
    [string] $TenantId,

    [Parameter(Mandatory = $false)]
    [string] $Location = "westeurope",

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

Write-Host "-> Obtaining storage account key for $StorageAccountName"
$StorageAccountKey = az storage account keys list `
    --resource-group $ResourceGroupName `
    --account-name $StorageAccountName `
    --query "[0].value" -o tsv

if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($StorageAccountKey)) {
    throw "Failed to obtain storage account key for '$StorageAccountName'"
}

$ContainerNameForTfState = "tfstate"

Write-Host "-> Creating blob container: $ContainerNameForTfState"
az storage container create `
    --name $ContainerNameForTfState `
    --account-name $StorageAccountName `
    --account-key $StorageAccountKey `
    --public-access off `
    --only-show-errors `
    --output none

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create blob container '$ContainerNameForTfState'"
}

$ContainerNameForAppConfigs = "app-configs"

Write-Host "-> Creating blob container: $ContainerNameForAppConfigs"
az storage container create `
    --name $ContainerNameForAppConfigs `
    --account-name $StorageAccountName `
    --account-key $StorageAccountKey `
    --public-access off `
    --only-show-errors `
    --output none

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create blob container '$ContainerNameForAppConfigs'"
}