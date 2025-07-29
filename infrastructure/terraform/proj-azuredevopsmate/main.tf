# Container App for Azure DevOps Mate (API)
resource "azurerm_container_app" "aca_azuredevopsmateapi" {
  name                         = "aca-azuredevopsmateapi"
  container_app_environment_id = var.container_app_environment_id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.uami_azuredevopsmate.id]
  }

  ingress {
    allow_insecure_connections = false
    external_enabled           = true
    target_port                = 8080
    transport                  = "auto"

    traffic_weight {
      percentage      = 100
      label           = "primary"
      latest_revision = true
    }

    ip_security_restriction {
      name             = "block-all-ips"
      description      = "Block all public IPs from accessing the container app"
      ip_address_range = "0.0.0.0/0"
      action           = "Deny"
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].env,
      template[0].container[0].image
    ]
  }

  registry {
    server   = var.acr_login_server
    identity = azurerm_user_assigned_identity.uami_azuredevopsmate.id
  }

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = "azuredevopsmateapi"
      image  = "mcr.microsoft.com/dotnet/samples:aspnetapp"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.application_insights_connection_string
      }
    }
  }
}

# User-Assigned Managed Identity
resource "azurerm_user_assigned_identity" "uami_azuredevopsmate" {
  name                = "uami-azuredevopsmate"
  location            = var.location
  resource_group_name = var.resource_group_name
}

# Role assignments
resource "azurerm_role_assignment" "acr_pull_for_azuredevopsmate" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}

resource "azurerm_role_assignment" "sa_blob_reader_for_azuredevopsmate" {
  scope                = var.blob_container_scope
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}

resource "azurerm_role_assignment" "sa_table_contributor_for_azuredevopsmate" {
  scope                = var.st_id
  role_definition_name = "Storage Table Data Contributor"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}

# Function apps
resource "azurerm_linux_function_app" "func_azuredevopsmate" {
  name                        = "func-azuredevopsmate"
  location                    = var.location
  resource_group_name         = var.resource_group_name
  service_plan_id             = var.asp_id
  storage_account_name        = var.st_name
  storage_account_access_key  = var.st_primary_key
  functions_extension_version = "~4"
  https_only                  = true

  site_config {}

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.uami_azuredevopsmate.id]
  }

  lifecycle {
    ignore_changes = [
      app_settings,
      site_config
    ]
  }

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = var.application_insights_connection_string
    "FUNCTIONS_WORKER_RUNTIME"              = "dotnet-isolated"
  }
}