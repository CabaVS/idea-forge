# Infrastructure

<div align="center">

![Infrastructure Logo](https://img.shields.io/badge/Infrastructure-as%20Code-blue?style=for-the-badge&logo=terraform)

</div>

## Overview

This directory contains all infrastructure-related code and configuration for deploying and managing the IdeaForge platform. It follows Infrastructure as Code (IaC) principles to ensure consistent, reproducible, and version-controlled infrastructure deployments.

## Structure

```
📁 infrastructure/
├── 📁 scripts/                  # Deployment and utility scripts
│   ├── pre-deployment.ps1
│   ├── pre-deployment-entraid.ps1
│   ├── post-deployment-entraid.ps1
│   └── tool-tfvarstobase64.ps1
└── 📁 terraform/                # Terraform configuration
    ├── 📁 shared/               # Shared Terraform modules
    ├── 📁 proj-azuredevopsmate/ # Project-specific infrastructure
    └── main.tf                  # Main Terraform configuration
```

## Key Components

### Scripts

- **Pre-Deployment Scripts**: Prepare the environment before deployment
- **Post-Deployment Scripts**: Finalize configurations after deployment
- **Utility Scripts**: Support tooling for infrastructure operations

### Terraform

The infrastructure is primarily defined using Terraform, with separate configurations for:

- **Shared Resources**: Common infrastructure components used across projects
- **Project-Specific Resources**: Infrastructure tailored to individual projects

## Getting Started

### Prerequisites

- [Terraform](https://www.terraform.io/downloads.html) (v1.0+)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) (for running deployment scripts)

### Deployment Workflow

1. **Initialize Terraform:**
   ```bash
   cd infrastructure/terraform
   terraform init
   ```

2. **Run Pre-Deployment Scripts:**
   ```powershell
   ../scripts/pre-deployment.ps1
   ../scripts/pre-deployment-entraid.ps1
   ```

3. **Plan Deployment:**
   ```bash
   terraform plan -out=tfplan
   ```

4. **Apply Changes:**
   ```bash
   terraform apply tfplan
   ```

5. **Run Post-Deployment Scripts:**
   ```powershell
   ../scripts/post-deployment-entraid.ps1
   ```

## Best Practices

- Always use version control for infrastructure changes
- Run `terraform plan` before applying changes
- Use workspace isolation for different environments
- Store sensitive values in secure vaults, not in code

## Related Resources

- [Azure Documentation](https://docs.microsoft.com/en-us/azure/)
- [Terraform Documentation](https://www.terraform.io/docs/)
