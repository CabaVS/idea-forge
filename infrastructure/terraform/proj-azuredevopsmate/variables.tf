variable "resource_group_name" {
  type        = string
  description = "Name of the existing Azure Resource Group for deployment"
}

variable "location" {
  type        = string
  description = "Azure region where resources will be deployed (e.g., westeurope, eastus)"
}

variable "storage_account_id" {
  type        = string
  description = "Resource ID of the Azure Storage Account used for application data"
}

variable "storage_account_container_app_configs_name" {
  type        = string
  description = "Name of the storage container that holds application configuration files"
}

variable "acr_id" {
  type        = string
  description = "Resource ID of the Azure Container Registry for container image storage"
}

variable "acr_login_server" {
  type        = string
  description = "FQDN of the Azure Container Registry (e.g., myregistry.azurecr.io)"
}

variable "ace_id" {
  type        = string
  description = "Resource ID of the Azure Container App Environment where apps will be deployed"
}

variable "application_insights_connection_string" {
  type        = string
  description = "Connection string for Application Insights telemetry collection"
  sensitive   = true
}