# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Services/DormSystem.sln", "Services/"]
COPY ["Infrastructure-services/ApiGateways/ApiGateway.YARP/ApiGateway.YARP.csproj", "Infrastructure-services/ApiGateways/ApiGateway.YARP/"]

# Restore dependencies
RUN dotnet restore "Infrastructure-services/ApiGateways/ApiGateway.YARP/ApiGateway.YARP.csproj"

# Copy all source files
COPY Infrastructure-services/ Infrastructure-services/

# Build the application
WORKDIR /src/Infrastructure-services/ApiGateways/ApiGateway.YARP
RUN dotnet build "ApiGateway.YARP.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ApiGateway.YARP.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.YARP.dll"]
