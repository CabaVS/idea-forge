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

# App Service Plan for Flex Consumption
resource "azurerm_service_plan" "flex_plan" {
  name                = "asp-cabavsideaforge-flex"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = "FC1"
}

# Storage Account for Function Apps
resource "azurerm_storage_account" "function_backplane" {
  name                = "stcabavsideaforgefunc"
  resource_group_name = var.resource_group_name
  location            = var.location

  account_tier               = "Standard"
  account_replication_type   = "LRS"
  account_kind               = "StorageV2"
  https_traffic_only_enabled = true

  min_tls_version = "TLS1_2"
}