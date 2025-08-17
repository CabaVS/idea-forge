# Monitoring Infrastructure
# Log Analytics Workspace for centralized logging
resource "azurerm_log_analytics_workspace" "law" {
  name                = "log-cabavsideaforge"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

# Application Insights for application monitoring and telemetry
resource "azurerm_application_insights" "app_insights" {
  name                 = "appi-cabavsideaforge"
  location             = var.location
  resource_group_name  = var.resource_group_name
  application_type     = "web"
  workspace_id         = azurerm_log_analytics_workspace.law.id
  sampling_percentage  = 100
  daily_data_cap_in_gb = 1
}

# Container Infrastructure
# Azure Container Registry for storing application container images
resource "azurerm_container_registry" "acr" {
  name                = "acrcabavsideaforge"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "Basic"
  admin_enabled       = false
}

# Azure Container Registry task that runs nightly to purge old images, keeping the 10 most recent per repository
resource "azurerm_container_registry_task" "purge_keep10" {
  name                  = "purge-keep10"
  container_registry_id = azurerm_container_registry.acr.id

  platform { os = "Linux" }

  encoded_step {
    task_content = file("${path.module}/acr_purge.yml")
  }

  timer_trigger {
    name     = "nightly"
    schedule = "0 0 * * *"
    enabled  = true
  }
}

# Container App Environment for running containerized applications
resource "azurerm_container_app_environment" "ace" {
  name                       = "ace-cabavsideaforge"
  location                   = var.location
  resource_group_name        = var.resource_group_name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.law.id
}

# Storage Infrastructure
# Container for storing application configuration files
resource "azurerm_storage_container" "app_configs" {
  name                  = "app-configs"
  storage_account_id    = var.storage_account_id
  container_access_type = "private"
}