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
COPY Shared/Shared.RoomServiceClient/Shared.RoomServiceClient.csproj Shared/Shared.RoomServiceClient/

# Inspections project
COPY Inspections/Inspections.API/Inspections.API.csproj Inspections/Inspections.API/
RUN dotnet restore Inspections/Inspections.API/Inspections.API.csproj

# Copy all source files
COPY Shared/ Shared/
COPY Inspections/ Inspections/

RUN dotnet publish Inspections/Inspections.API/Inspections.API.csproj \
    -c Release \
    -o /app/publish

# Runtime stage — needs native libs for SkiaSharp/QuestPDF
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libfreetype6 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Inspections.API.dll"]
