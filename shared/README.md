# Shared Components

<div align="center">

![Shared Components](https://img.shields.io/badge/Shared-Components-green?style=for-the-badge&logo=dotnet)

</div>

## Overview

This directory contains reusable building blocks shared across multiple applications within the Idea Forge platform. These components are designed to prevent code duplication and ensure consistency across different projects.

## Structure

```
📁 shared/
└── 📁 CabaVS.Shared.Infrastructure/  # Infrastructure-related shared components
    ├── CabaVS.Shared.Infrastructure.csproj
    └── AzureBlobJsonConfigurationProvider.cs
```

## Component Libraries

### CabaVS.Shared.Infrastructure

A library containing shared infrastructure components that can be used across different projects.

**Key Features:**

- **AzureBlobJsonConfigurationProvider**: Provides configuration from JSON files stored in Azure Blob Storage
- Additional infrastructure utilities and helpers

#### Usage Example

```csharp
using CabaVS.Shared.Infrastructure;

// In Program.cs or Startup.cs
builder.Configuration.AddJsonStreamFromBlob(
    isDevelopment: false);
```

## Adding New Shared Components

When adding new shared components:

1. Determine the appropriate category for your component
2. Create a new library if needed, following the naming convention `CabaVS.Shared.[Category]`
3. Ensure comprehensive documentation and unit tests
4. Reference from consuming projects using project references or NuGet packages

## Guidelines

### When to Create Shared Components

- The code is used by two or more projects
- The functionality is generic and not specific to a single business domain
- The component has a clear, focused responsibility

### Design Principles

- **Single Responsibility**: Each component should do one thing well
- **Interface Segregation**: Design small, focused interfaces
- **Dependency Inversion**: Depend on abstractions, not concrete implementations
- **Minimal Dependencies**: Limit external dependencies to reduce complexity
