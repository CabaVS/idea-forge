variable "resource_group_name" {
  type        = string
  description = "Name of the existing resource group"
}

variable "location" {
  type        = string
  description = "Azure region for the resources"
}

variable "acr_id" {
  type        = string
  description = "Resource ID of the ACR (used for role assignment)"
}

variable "acr_login_server" {
  type        = string
  description = "Login server URL for the Azure Container Registry"
}

variable "container_app_environment_id" {
  type        = string
  description = "ID of the Container App Environment"
}

variable "application_insights_connection_string" {
  type        = string
  description = "Connection string for the shared Application Insights instance"
}

variable "blob_container_app_configs" {
  type        = string
  description = "Container name with Application Configurations"
}

variable "blob_container_function_apps" {
  type        = string
  description = "Container name with Function Apps files"
}

variable "asp_id" {
  type        = string
  description = "Resource ID of the ASP for Function Apps"
}

variable "st_id" {
  type        = string
  description = "ID of the Storage Account"
}

variable "st_name" {
  type        = string
  description = "Name of the Storage Account"
}