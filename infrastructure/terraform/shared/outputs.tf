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