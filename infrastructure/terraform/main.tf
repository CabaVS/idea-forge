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

# Modules: Shared
module "shared" {
  source = "./shared"

  resource_group_name = data.azurerm_resource_group.existing.name
  location            = data.azurerm_resource_group.existing.location
}