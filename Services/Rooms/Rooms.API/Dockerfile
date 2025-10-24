# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Services/DormSystem.sln", "Services/"]
COPY ["Services/Rooms/Rooms.API/Rooms.API.csproj", "Services/Rooms/Rooms.API/"]
COPY ["Services/Shared/Shared.Data/Shared.Data.csproj", "Services/Shared/Shared.Data/"]
COPY ["Services/Shared/Shared.TokenService/Shared.TokenService.csproj", "Services/Shared/Shared.TokenService/"]
COPY ["Services/Shared/Shared.PagedList/Shared.PagedList.csproj", "Services/Shared/Shared.PagedList/"]

# Restore dependencies
RUN dotnet restore "Services/Rooms/Rooms.API/Rooms.API.csproj"

# Copy all source files
COPY Services/ Services/

# Build the application
WORKDIR /src/Services/Rooms/Rooms.API
RUN dotnet build "Rooms.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Rooms.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create directory for SQLite database
RUN mkdir -p /app/data

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rooms.API.dll"]
