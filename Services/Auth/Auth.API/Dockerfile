# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Services/DormSystem.sln", "Services/"]
COPY ["Services/Auth/Auth.API/Auth.API.csproj", "Services/Auth/Auth.API/"]
COPY ["Services/Auth/Auth.BLL/Auth.BLL.csproj", "Services/Auth/Auth.BLL/"]
COPY ["Services/Auth/Auth.DAL/Auth.DAL.csproj", "Services/Auth/Auth.DAL/"]
COPY ["Services/Shared/Shared.Data/Shared.Data.csproj", "Services/Shared/Shared.Data/"]
COPY ["Services/Shared/Shared.TokenService/Shared.TokenService.csproj", "Services/Shared/Shared.TokenService/"]

# Restore dependencies
RUN dotnet restore "Services/Auth/Auth.API/Auth.API.csproj"

# Copy all source files
COPY Services/ Services/

# Build the application
WORKDIR /src/Services/Auth/Auth.API
RUN dotnet build "Auth.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]
