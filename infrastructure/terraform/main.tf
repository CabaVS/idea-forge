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

variable "container_name_for_app_configs" {
  type    = string
  default = "app-configs"
}

variable "container_name_for_function_apps" {
  type    = string
  default = "function-releases"
}

# Existing Resource Group
data "azurerm_resource_group" "existing" {
  name = var.resource_group_name
}

# Existing Storage Account
data "azurerm_storage_account" "existing" {
  name                = var.storage_account_name
  resource_group_name = var.resource_group_name
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

  resource_group_name                    = var.resource_group_name
  location                               = data.azurerm_resource_group.existing.location
  acr_id                                 = module.shared.acr_id
  acr_login_server                       = module.shared.acr_login_server
  container_app_environment_id           = module.shared.ace_id
  blob_container_app_configs             = var.container_name_for_app_configs
  blob_container_function_apps           = var.container_name_for_function_apps
  application_insights_connection_string = module.shared.application_insights_connection_string
  asp_id                                 = module.shared.asp_id
  st_id                                  = data.azurerm_storage_account.existing.id
  st_name                                = data.azurerm_storage_account.existing.name
}