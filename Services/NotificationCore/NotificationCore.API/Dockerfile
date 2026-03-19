# Build stage
# Context: Services/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .

# Shared projects
COPY Shared/ServiceDefaults/ServiceDefaults.csproj Shared/ServiceDefaults/
COPY Shared/Shared.TokenService/Shared.TokenService.csproj Shared/Shared.TokenService/
COPY Shared/Shared.UserServiceClient/Shared.UserServiceClient.csproj Shared/Shared.UserServiceClient/
COPY Shared/Shared.Data/Shared.Data.csproj Shared/Shared.Data/
COPY Shared/Shared.RoomServiceClient/Shared.RoomServiceClient.csproj Shared/Shared.RoomServiceClient/

# NotificationCore project
COPY NotificationCore/NotificationCore.API/NotificationCore.API.csproj NotificationCore/NotificationCore.API/
RUN dotnet restore NotificationCore/NotificationCore.API/NotificationCore.API.csproj

# Copy all source files
COPY Shared/ Shared/
COPY NotificationCore/ NotificationCore/

RUN dotnet publish NotificationCore/NotificationCore.API/NotificationCore.API.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "NotificationCore.API.dll"]
