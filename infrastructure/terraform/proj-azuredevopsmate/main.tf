# User-assigned managed identity for the Container App and Container App Jobs
resource "azurerm_user_assigned_identity" "identity_azuredevopsmate" {
  name                = "uami-azuredevopsmate"
  location            = var.location
  resource_group_name = var.resource_group_name
}

resource "azurerm_user_assigned_identity" "identity_azuredevopsmate_jobs_rwt" {
  name                = "uami-azuredevopsmate-jobs-rwt"
  location            = var.location
  resource_group_name = var.resource_group_name
}

# Role assignment for Container Registry access
resource "azurerm_role_assignment" "role_acr_pull" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.identity_azuredevopsmate.principal_id
}

resource "azurerm_role_assignment" "role_acr_pull_jobs_rwt" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.identity_azuredevopsmate_jobs_rwt.principal_id
}

# Role assignment for Storage Blob access
resource "azurerm_role_assignment" "role_blob_reader" {
  scope                = "${var.storage_account_id}/blobServices/default/containers/${var.storage_account_container_app_configs_name}"
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_user_assigned_identity.identity_azuredevopsmate.principal_id
}

resource "azurerm_role_assignment" "role_blob_reader_jobs_rwt" {
  scope                = "${var.storage_account_id}/blobServices/default/containers/${var.storage_account_container_app_configs_name}"
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_user_assigned_identity.identity_azuredevopsmate_jobs_rwt.principal_id
}

resource "azurerm_role_assignment" "role_blob_contributor_jobs_rwt" {
  scope                = "${var.storage_account_id}/blobServices/default/containers/${var.storage_account_container_azuredevopsmate_name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.identity_azuredevopsmate_jobs_rwt.principal_id

  depends_on = [azurerm_storage_container.container_azuredevopsmate]
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

# Azure Container App Job - Azure DevOps Mate - Remaining Work Tracker
resource "azurerm_container_app_job" "job_azuredevopsmate_rwt" {
  name                         = "job-azuredevopsmate-rwt"
  container_app_environment_id = var.ace_id
  resource_group_name          = var.resource_group_name
  location                     = var.location

  replica_timeout_in_seconds = 60
  replica_retry_limit        = 5

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.identity_azuredevopsmate_jobs_rwt.id]
  }

  schedule_trigger_config {
    cron_expression = "0 0 * * 2-6"
    parallelism     = 1
  }

  template {
    container {
      name   = "job"
      image  = "mcr.microsoft.com/dotnet/samples:dotnetapp"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.application_insights_connection_string
      }
    }
  }

  registry {
    server   = var.acr_login_server
    identity = azurerm_user_assigned_identity.identity_azuredevopsmate_jobs_rwt.id
  }

  lifecycle {
    ignore_changes = [
      template[0].container[0].env,
      template[0].container[0].image
    ]
  }
}

# Storage Container creation for AzureDevOpsMate
resource "azurerm_storage_container" "container_azuredevopsmate" {
  name                  = var.storage_account_container_azuredevopsmate_name
  storage_account_id    = var.storage_account_id
  container_access_type = "private"
}