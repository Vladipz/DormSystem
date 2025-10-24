# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Services/DormSystem.sln", "Services/"]
COPY ["Services/FileStorage/FileStorage.API/FileStorage.API.csproj", "Services/FileStorage/FileStorage.API/"]
COPY ["Services/Shared/Shared.Data/Shared.Data.csproj", "Services/Shared/Shared.Data/"]
COPY ["Services/Shared/Shared.TokenService/Shared.TokenService.csproj", "Services/Shared/Shared.TokenService/"]

# Restore dependencies
RUN dotnet restore "Services/FileStorage/FileStorage.API/FileStorage.API.csproj"

# Copy all source files
COPY Services/ Services/

# Build the application
WORKDIR /src/Services/FileStorage/FileStorage.API
RUN dotnet build "FileStorage.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FileStorage.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create directories for file storage
RUN mkdir -p /app/data /app/uploads

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileStorage.API.dll"]
