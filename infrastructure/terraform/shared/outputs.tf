output "application_insights_connection_string" {
  value = azurerm_application_insights.app_insights.connection_string
}

output "acr_id" {
  value = azurerm_container_registry.acr.id
}

output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "ace_id" {
  value = azurerm_container_app_environment.ace.id
}

output "asp_flex_id" {
  value = azurerm_service_plan.flex_plan.id
}

output "storage_account_functions_backplane_id" {
  value = azurerm_storage_account.function_backplane.id
}

output "storage_account_functions_backplane_name" {
  value = azurerm_storage_account.function_backplane.name
}

output "storage_account_functions_backplane_primary_connection_string" {
  value = azurerm_storage_account.function_backplane.primary_connection_string
}