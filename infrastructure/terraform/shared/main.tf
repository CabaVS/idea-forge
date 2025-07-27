# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "law" {
  name                = "log-cabavsideaforge"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

# Application Insights
resource "azurerm_application_insights" "app_insights" {
  name                 = "appi-cabavsideaforge"
  location             = var.location
  resource_group_name  = var.resource_group_name
  application_type     = "web"
  workspace_id         = azurerm_log_analytics_workspace.law.id
  sampling_percentage  = 100
  daily_data_cap_in_gb = 1
}

# Azure Container Registry
resource "azurerm_container_registry" "acr" {
  name                = "acrcabavsideaforge"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "Basic"
  admin_enabled       = false
}

# Container App Environment
resource "azurerm_container_app_environment" "ace" {
  name                       = "ace-cabavsideaforge"
  location                   = var.location
  resource_group_name        = var.resource_group_name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.law.id
}