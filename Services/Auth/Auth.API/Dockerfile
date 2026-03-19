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
COPY Shared/Shared.FileServiceClient/Shared.FileServiceClient.csproj Shared/Shared.FileServiceClient/

# Auth projects
COPY Auth/Auth.DAL/Auth.DAL.csproj Auth/Auth.DAL/
COPY Auth/Auth.BLL/Auth.BLL.csproj Auth/Auth.BLL/
COPY Auth/Auth.API/Auth.API.csproj Auth/Auth.API/
RUN dotnet restore Auth/Auth.API/Auth.API.csproj

# Copy all source files
COPY Shared/ Shared/
COPY Auth/ Auth/

RUN dotnet publish Auth/Auth.API/Auth.API.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Auth.API.dll"]
