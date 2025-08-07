FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "CabaVS.AzureDevOpsMate.Jobs.RemainingWorkTracker.dll"]
