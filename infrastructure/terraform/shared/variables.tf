variable "resource_group_name" {
  type        = string
  description = "Name of the existing Azure Resource Group where resources will be deployed"
}

variable "location" {
  type        = string
  description = "Azure region where all resources will be deployed (e.g., westeurope, eastus)"
}

variable "storage_account_id" {
  type        = string
  description = "Resource ID of the existing Azure Storage Account used for application data"
}