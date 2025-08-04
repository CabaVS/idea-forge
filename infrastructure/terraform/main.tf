# Configure Terraform and required providers
terraform {
  required_version = "1.12.2"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.37.0"
    }
  }

  # Azure backend configuration for storing Terraform state
  backend "azurerm" {}
}

# Configure Azure provider with minimal features
provider "azurerm" {
  features {}
  subscription_id = var.azure_subscription_id
}

# Input Variables
variable "azure_resource_group_name" {
  description = "Name of the existing Azure Resource Group"
  type        = string
}

variable "azure_storage_account_name" {
  description = "Name of the existing Azure Storage Account"
  type        = string
}

variable "azure_subscription_id" {
  description = "Azure Subscription ID where resources will be deployed"
  type        = string
}

# Data Sources - Existing Azure Resources
data "azurerm_resource_group" "target_rg" {
  name = var.azure_resource_group_name
}

data "azurerm_storage_account" "target_storage" {
  name                = var.azure_storage_account_name
  resource_group_name = var.azure_resource_group_name
}

# Shared Infrastructure Module
# Contains common resources like Container Registry and App Environment
module "shared_infrastructure" {
  source = "./shared"

  resource_group_name = data.azurerm_resource_group.target_rg.name
  location            = data.azurerm_resource_group.target_rg.location
  storage_account_id  = data.azurerm_storage_account.target_storage.id
}

# Project: Azure DevOps Mate
# Contains project-specific resources and configurations
module "project_azure_devops_mate" {
  source = "./proj-azuredevopsmate"

  resource_group_name                        = var.azure_resource_group_name
  location                                   = data.azurerm_resource_group.target_rg.location
  storage_account_id                         = data.azurerm_storage_account.target_storage.id
  acr_id                                     = module.shared_infrastructure.acr_id
  acr_login_server                           = module.shared_infrastructure.acr_login_server
  ace_id                                     = module.shared_infrastructure.ace_id
  application_insights_connection_string     = module.shared_infrastructure.application_insights_connection_string
  storage_account_container_app_configs_name = module.shared_infrastructure.storage_account_container_app_configs_name
}