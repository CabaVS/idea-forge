# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:9.0.302 AS build
WORKDIR /app

# Copy sln and supporting files
COPY .editorconfig ./
COPY Directory.* ./
COPY global.json ./
COPY *.slnx ./

# Copy shared resources
COPY shared/CabaVS.Shared.Infrastructure/ ./shared/CabaVS.Shared.Infrastructure/

# Copy source
COPY proj-azuredevopsmate/ ./proj-azuredevopsmate/

# Publish (build + restore in one go)
WORKDIR /app/proj-azuredevopsmate/CabaVS.AzureDevOpsMate
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# -------- Runtime stage --------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CabaVS.AzureDevOpsMate.dll"]
