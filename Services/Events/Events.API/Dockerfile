# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Services/DormSystem.sln", "Services/"]
COPY ["Services/Events/Events.API/Events.API.csproj", "Services/Events/Events.API/"]
COPY ["Services/Shared/Shared.Data/Shared.Data.csproj", "Services/Shared/Shared.Data/"]
COPY ["Services/Shared/Shared.TokenService/Shared.TokenService.csproj", "Services/Shared/Shared.TokenService/"]
COPY ["Services/Shared/Shared.UserServiceClient/Shared.UserServiceClient.csproj", "Services/Shared/Shared.UserServiceClient/"]
COPY ["Services/Shared/Shared.FileServiceClient/Shared.FileServiceClient.csproj", "Services/Shared/Shared.FileServiceClient/"]

# Restore dependencies
RUN dotnet restore "Services/Events/Events.API/Events.API.csproj"

# Copy all source files
COPY Services/ Services/

# Build the application
WORKDIR /src/Services/Events/Events.API
RUN dotnet build "Events.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Events.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Events.API.dll"]
