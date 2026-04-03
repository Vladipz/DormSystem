# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy Directory.Build.props first — defines TargetFramework for all projects
COPY Directory.Build.props .
COPY Directory.Packages.props .

# Copy shared project files for layer-cached restore
# we use for ./Services as a context
COPY Shared/Shared.Data/Shared.Data.csproj Shared/Shared.Data/
COPY Shared/Shared.PagedList/Shared.PagedList.csproj Shared/Shared.PagedList/
COPY Shared/Shared.TokenService/Shared.TokenService.csproj Shared/Shared.TokenService/
COPY Shared/Shared.UserServiceClient/Shared.UserServiceClient.csproj Shared/Shared.UserServiceClient/
COPY Shared/ServiceDefaults/ServiceDefaults.csproj Shared/ServiceDefaults/

# Copy Events project file and restore
COPY Events/Events.API/Events.API.csproj Events/Events.API/
RUN dotnet restore Events/Events.API/Events.API.csproj

# Copy all source files
COPY Shared/ Shared/
COPY Events/ Events/

# Build and publish
# If .csproj files unchanged, 'RUN dotnet restore' layer above is cached.
# Packages in /root/.nuget/packages/ persist, so restore here is near-instant.
RUN dotnet publish Events/Events.API/Events.API.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Events.API.dll"]
