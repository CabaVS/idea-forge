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

Write-Host "-> Acquiring SP object ID: $AppName"
$SpObjectId = az ad sp list --display-name $AppName --query "[0].id" --output tsv

if ($LASTEXITCODE -ne 0) {
    throw "Failed to acquire SP object ID for '$AppName'"
}

Write-Host "-> Assigning role 'AcrPush' to SP: $SpObjectId"
$AcrName = "acrcabavsideaforge"
if (-not [string]::IsNullOrWhiteSpace($Environment)) {
    $AcrName = "$AcrName$Environment"
}

az role assignment create `
    --assignee-object-id $SpObjectId `
    --role "AcrPush" `
    --scope "/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.ContainerRegistry/registries/$AcrName" | Out-Null

if ($LASTEXITCODE -ne 0) {
    throw "Failed to assign the role 'AcrPush' to the SP"
}
