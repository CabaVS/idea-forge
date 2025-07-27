# CabaVS Idea Forge

<div align="center">

![IdeaForge Logo](https://img.shields.io/badge/IdeaForge-Mono--Repo-orange?style=for-the-badge&logo=csharp)

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?style=flat-square&logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Latest-512BD4?style=flat-square&logo=dotnet)](https://docs.microsoft.com/en-us/aspnet/core/)

</div>

## Overview

Idea Forge is a mono-repository containing multiple projects and shared components that may or may not be related to each other. Each project represents a different idea or application, but they're all managed within a single repository to facilitate code sharing, consistent practices, and simplified deployment.

## Repository Structure

```
📁 IdeaForge/
├── 📁 aspire/               # .NET Aspire orchestration
├── 📁 infrastructure/       # Infrastructure as Code (IaC) resources
├── 📁 shared/               # Shared components and libraries
├── 📁 proj-azuredevopsmate/ # Azure DevOps Mate project
└── 📁 .github/              # GitHub workflows and templates
```

## Infrastructure

Infrastructure as Code (IaC) resources for deploying and managing all projects:

[View Infrastructure Documentation](./infrastructure/README.md)

## Projects

### Azure DevOps Mate

A tool to enhance productivity when working with Azure DevOps.

[View Project README](./proj-azuredevopsmate/README.md)

## Shared Components

The repository contains shared components that are reused across multiple projects:

[View Shared Components Documentation](./shared/README.md)

## Development Setup

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for infrastructure operations)

## Adding a New Project

To add a new project to the mono-repo:

1. Create a new directory with the prefix `proj-` (e.g., `proj-newproject`)
2. Follow the standard .NET project structure
3. Reference shared components as needed
4. Update the central Directory.Build.props and Directory.Packages.props as needed
5. Add project-specific infrastructure in the infrastructure directory
6. Create a README.md for your project

## Technology Stack

- **Backend:** ASP.NET Core, C# 13.0
- **Infrastructure:** Azure, Terraform
- **CI/CD:** GitHub Actions
- **Containerization:** Docker
