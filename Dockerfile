# Stage 1 - Build the .NET 8 Azure Functions project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy the project file and restore NuGet packages
COPY CoffeeNchill.csproj ./
RUN dotnet restore CoffeeNchill.csproj

# Copy the rest of the project
COPY . .

# Publish the application
RUN dotnet publish CoffeeNchill.csproj --configuration Release --output /app/publish


# Stage 2 - Run the application using Azure Functions
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
ENV FUNCTIONS_WORKER_RUNTIME=dotnet-isolated

COPY --from=build /app/publish /home/site/wwwroot