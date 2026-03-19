# Build stage
# Context: Services/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .

# Shared projects
COPY Shared/ServiceDefaults/ServiceDefaults.csproj Shared/ServiceDefaults/

# FileStorage project
COPY FileStorage/FileStorage.API/FileStorage.API.csproj FileStorage/FileStorage.API/
RUN dotnet restore FileStorage/FileStorage.API/FileStorage.API.csproj

# Copy all source files
COPY Shared/ Shared/
COPY FileStorage/ FileStorage/

RUN dotnet publish FileStorage/FileStorage.API/FileStorage.API.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FileStorage.API.dll"]
