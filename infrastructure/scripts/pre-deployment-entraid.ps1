[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SubscriptionId,

    [Parameter(Mandatory = $true)]
    [string] $TenantId,

    [Parameter(Mandatory = $false)]
    [string] $Environment,

    # Format: owner/repo (e.g., myorg/myinfra)
    [Parameter(Mandatory = $true)]
    [string] $GitHubRepo,

    # GitHub environment to allow OIDC from (e.g., production)
    [Parameter(Mandatory = $true)]
    [string] $GitHubEnv,

    [Parameter(Mandatory = $true)]
    [string] $StorageAccountName
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

Write-Host "-> Creating app registration: $AppName"
$App = az ad app create --display-name $AppName | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create app registration '$AppName'"
}

$ClientId = $App.appId
$AppObjectId = $App.id

Write-Host "-> Creating service principal for $AppName"
$Sp = az ad sp create --id $ClientId | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    throw "Failed to create service principal for '$AppName'"
}

$SpObjectId = $Sp.id

$Subject = "repo:$($GitHubRepo):environment:$($GitHubEnv)"

Write-Host "-> Adding federated identity credential for $Subject"

$TempFile = New-TemporaryFile

@"
{
  "name": "github-actions-$($GitHubRepo.Replace('/','-'))-$($GitHubEnv)",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "$Subject",
  "description": "GitHub Actions OIDC for $GitHubRepo on environment $GitHubEnv",
  "audiences": ["api://AzureADTokenExchange"]
}
"@ | Out-File -Encoding utf8 -FilePath $TempFile.FullName

az ad app federated-credential create `
    --id $AppObjectId `
    --parameters "@$($TempFile.FullName)" | Out-Null

Remove-Item $TempFile.FullName -Force

if ($LASTEXITCODE -ne 0) {
    throw "Failed to add federated identity credential for '$Subject'"
}

$Scope = "/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName"

Write-Host "-> Assigning required roles to SP at scope: $Scope"

az role assignment create `
    --assignee-object-id $SpObjectId `
    --role "Contributor" `
    --scope $Scope | Out-Null

if ($LASTEXITCODE -ne 0) {
    throw "Failed to assign a role 'Contributor' for SP"
}

az role assignment create `
    --assignee-object-id $SpObjectId `
    --role "User Access Administrator" `
    --scope $Scope | Out-Null

if ($LASTEXITCODE -ne 0) {
    throw "Failed to assign a role 'User Access Administrator' for SP"
}

$StorageAccountName = "stcabavsideaforge"
if (-not [string]::IsNullOrWhiteSpace($Environment)) {
    $StorageAccountName = "$StorageAccountName$Environment"
}

$TerraformStateContainerName = "tfstate"

az role assignment create `
    --assignee-object-id $SpObjectId `
    --role "Storage Blob Data Contributor" `
    --scope "$Scope/providers/Microsoft.Storage/storageAccounts/$StorageAccountName/blobServices/default/containers/$TerraformStateContainerName" | Out-Null

if ($LASTEXITCODE -ne 0) {
    throw "Failed to assign a role 'Storage Blob Data Contributor' for SP"
}

# Output required values for GitHub secrets
Write-Host ""
Write-Host "Done. Add the following to your GitHub repo secrets:"
Write-Host "AZURE_CLIENT_ID       = $ClientId"
Write-Host "AZURE_TENANT_ID       = $TenantId"
Write-Host "AZURE_SUBSCRIPTION_ID = $SubscriptionId"