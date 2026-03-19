# Build stage
# Context: Services/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .

# Shared projects
COPY Shared/ServiceDefaults/ServiceDefaults.csproj Shared/ServiceDefaults/

# Booking project
COPY Booking/Booking.API/Booking.API.csproj Booking/Booking.API/
RUN dotnet restore Booking/Booking.API/Booking.API.csproj

# Copy all source files
COPY Shared/ Shared/
COPY Booking/ Booking/

RUN dotnet publish Booking/Booking.API/Booking.API.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Booking.API.dll"]
