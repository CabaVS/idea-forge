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
resource "azurerm_role_assignment" "acr_pull_for_aca_azuredevopsmate" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}

resource "azurerm_role_assignment" "sa_blob_reader_for_aca_azuredevopsmate" {
  scope                = var.blob_container_scope
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}