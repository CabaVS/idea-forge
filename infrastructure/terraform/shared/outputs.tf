# Monitoring Outputs
output "application_insights_connection_string" {
  description = "Connection string for Application Insights integration"
  value       = azurerm_application_insights.app_insights.connection_string
  sensitive   = true
}

# Container Registry Outputs
output "acr_id" {
  description = "Resource ID of the Azure Container Registry"
  value       = azurerm_container_registry.acr.id
}

output "acr_login_server" {
  description = "Login server URL for the Azure Container Registry"
  value       = azurerm_container_registry.acr.login_server
}

# Container App Environment Outputs
output "ace_id" {
  description = "Resource ID of the Azure Container App Environment"
  value       = azurerm_container_app_environment.ace.id
}

# Storage Outputs
output "storage_account_container_app_configs_name" {
  description = "Name of the storage container used for application configurations"
  value       = azurerm_storage_container.app_configs.name
}