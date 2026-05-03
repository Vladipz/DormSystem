# Build stage
# Context: Services/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Packages.props .

COPY Shared/ServiceDefaults/ServiceDefaults.csproj Shared/ServiceDefaults/
COPY MotivationFake/MotivationFake.API/MotivationFake.API.csproj MotivationFake/MotivationFake.API/

RUN dotnet restore MotivationFake/MotivationFake.API/MotivationFake.API.csproj

COPY Shared/ Shared/
COPY MotivationFake/ MotivationFake/

RUN dotnet publish MotivationFake/MotivationFake.API/MotivationFake.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "MotivationFake.API.dll"]
