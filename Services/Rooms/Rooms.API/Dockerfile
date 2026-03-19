# Build stage
# Context: Services/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .

# Shared projects
COPY Shared/ServiceDefaults/ServiceDefaults.csproj Shared/ServiceDefaults/
COPY Shared/Shared.TokenService/Shared.TokenService.csproj Shared/Shared.TokenService/
COPY Shared/Shared.PagedList/Shared.PagedList.csproj Shared/Shared.PagedList/
COPY Shared/Shared.UserServiceClient/Shared.UserServiceClient.csproj Shared/Shared.UserServiceClient/
COPY Shared/Shared.Data/Shared.Data.csproj Shared/Shared.Data/
COPY Shared/Shared.FileServiceClient/Shared.FileServiceClient.csproj Shared/Shared.FileServiceClient/

# Rooms project
COPY Rooms/Rooms.API/Rooms.API.csproj Rooms/Rooms.API/
RUN dotnet restore Rooms/Rooms.API/Rooms.API.csproj

# Copy all source files
COPY Shared/ Shared/
COPY Rooms/ Rooms/

RUN dotnet publish Rooms/Rooms.API/Rooms.API.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Rooms.API.dll"]
