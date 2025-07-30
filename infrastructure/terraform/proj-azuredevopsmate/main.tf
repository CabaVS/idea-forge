# Container App for Azure DevOps Mate (API)
resource "azurerm_container_app" "aca_azuredevopsmateapi" {
  name                         = "aca-azuredevopsmateapi"
  container_app_environment_id = var.container_app_environment_id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.uami_azuredevopsmateapi.id]
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

    # TODO: Replace with a proper VNET or other sort of protection
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
    identity = azurerm_user_assigned_identity.uami_azuredevopsmateapi.id
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
resource "azurerm_user_assigned_identity" "uami_azuredevopsmateapi" {
  name                = "uami-azuredevopsmateapi"
  location            = var.location
  resource_group_name = var.resource_group_name
}

resource "azurerm_user_assigned_identity" "uami_azuredevopsmate" {
  name                = "uami-azuredevopsmate"
  location            = var.location
  resource_group_name = var.resource_group_name
}

# Role assignments
resource "azurerm_role_assignment" "acr_pull_for_azuredevopsmateapi" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmateapi.principal_id
}

resource "azurerm_role_assignment" "sa_blob_reader_for_azuredevopsmateapi" {
  scope                = "${var.storage_account_id}/blobServices/default/containers/${var.container_name_for_app_configs}"
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmateapi.principal_id
}

resource "azurerm_role_assignment" "sa_blob_contributor_for_azuredevopsmate" {
  scope                = var.functions_backplane_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}

resource "azurerm_role_assignment" "sa_table_contributor_for_azuredevopsmate" {
  scope                = var.functions_backplane_id
  role_definition_name = "Storage Table Data Contributor"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}

resource "azurerm_role_assignment" "sa_queue_contributor_for_azuredevopsmate" {
  scope                = var.functions_backplane_id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}

resource "azurerm_role_assignment" "sa_blob_reader_for_azuredevopsmate" {
  scope                = "${var.storage_account_id}/blobServices/default/containers/${var.container_name_for_app_configs}"
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_user_assigned_identity.uami_azuredevopsmate.principal_id
}

# Storage Account Containers
resource "azurerm_storage_container" "function_release_container" {
  name                  = "function-app-package-proj-azuredevopsmate"
  storage_account_id    = var.functions_backplane_id
  container_access_type = "private"
}

# Function App
resource "azurerm_function_app_flex_consumption" "function_app" {
  name                = "func-azuredevopsmate"
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = var.asp_flex_id

  storage_container_type            = "blobContainer"
  storage_authentication_type       = "UserAssignedIdentity"
  storage_user_assigned_identity_id = azurerm_user_assigned_identity.uami_azuredevopsmate.id
  storage_container_endpoint        = "https://${var.functions_backplane_name}.blob.core.windows.net/${azurerm_storage_container.function_release_container.name}"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.uami_azuredevopsmate.id]
  }

  site_config {}

  lifecycle {
    ignore_changes = [
      app_settings["APPLICATIONINSIGHTS_CONNECTION_STRING"],
      app_settings["AzureWebJobsStorage"],
      site_config[0].application_insights_connection_string,
      storage_access_key
    ]
  }

  runtime_name    = "dotnet-isolated"
  runtime_version = "9.0"

  instance_memory_in_mb = 512

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = var.application_insights_connection_string

    # It's injected automatically with invalid connection string, even though UAMI authorization is used (which causes functions app to crash)
    "AzureWebJobsStorage" = var.functions_backplane_primary_connection_string
  }

  https_only = true
}