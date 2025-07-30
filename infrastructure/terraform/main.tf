terraform {
  required_version = "1.12.2"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.37.0"
    }
  }

  backend "azurerm" {}
}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}

# Variables
variable "resource_group_name" {}
variable "storage_account_name" {}
variable "subscription_id" {}

# Existing Resource Group
data "azurerm_resource_group" "existing" {
  name = var.resource_group_name
}

# Existing Storage Account
data "azurerm_storage_account" "existing" {
  name                = var.storage_account_name
  resource_group_name = var.resource_group_name
}

# Existing Storage Account Containers
resource "azurerm_storage_container" "app_configs" {
  name                  = "app-configs"
  storage_account_id    = data.azurerm_storage_account.existing.id
  container_access_type = "private"
}

# Modules: Shared
module "shared" {
  source = "./shared"

  resource_group_name = data.azurerm_resource_group.existing.name
  location            = data.azurerm_resource_group.existing.location
}

# Modules: Project for Azure DevOps Mate
module "project_azuredevopsmate" {
  source = "./proj-azuredevopsmate"

  resource_group_name                           = var.resource_group_name
  location                                      = data.azurerm_resource_group.existing.location
  acr_id                                        = module.shared.acr_id
  acr_login_server                              = module.shared.acr_login_server
  container_app_environment_id                  = module.shared.ace_id
  application_insights_connection_string        = module.shared.application_insights_connection_string
  storage_account_id                            = data.azurerm_storage_account.existing.id
  container_name_for_app_configs                = azurerm_storage_container.app_configs.name
  asp_flex_id                                   = module.shared.asp_flex_id
  functions_backplane_id                        = module.shared.storage_account_functions_backplane_id
  functions_backplane_name                      = module.shared.storage_account_functions_backplane_name
  functions_backplane_primary_connection_string = module.shared.storage_account_functions_backplane_primary_connection_string
}