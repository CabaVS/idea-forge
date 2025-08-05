# User-assigned managed identity for the Container App
# Provides secure access to Container Registry and Storage
resource "azurerm_user_assigned_identity" "identity_azuredevopsmate" {
  name                = "uami-azuredevopsmate"
  location            = var.location
  resource_group_name = var.resource_group_name
}

# Role assignment for Container Registry access
resource "azurerm_role_assignment" "role_acr_pull" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.identity_azuredevopsmate.principal_id
}

# Role assignment for Storage Blob access
resource "azurerm_role_assignment" "role_blob_reader" {
  scope                = "${var.storage_account_id}/blobServices/default/containers/${var.storage_account_container_app_configs_name}"
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_user_assigned_identity.identity_azuredevopsmate.principal_id
}

# Azure Container App - Azure DevOps Mate API
resource "azurerm_container_app" "app_azuredevopsmate" {
  name                         = "aca-azuredevopsmate"
  container_app_environment_id = var.ace_id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.identity_azuredevopsmate.id]
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
  }

  registry {
    server   = var.acr_login_server
    identity = azurerm_user_assigned_identity.identity_azuredevopsmate.id
  }

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = "api"
      image  = "mcr.microsoft.com/dotnet/samples:aspnetapp"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.application_insights_connection_string
      }
    }
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].env,
      template[0].container[0].image
    ]
  }
}