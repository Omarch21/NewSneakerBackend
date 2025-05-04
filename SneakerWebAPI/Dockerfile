FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

# Stage for building the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SneakerWebAPI/SneakerWebAPI.csproj", "SneakerWebAPI/"]
RUN dotnet restore "./SneakerWebAPI/SneakerWebAPI.csproj"
COPY . . 
WORKDIR "/src/SneakerWebAPI"
RUN dotnet build "./SneakerWebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage for publishing the service project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SneakerWebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image with PowerShell and Playwright
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install PowerShell
RUN apt-get update && apt-get install -y wget apt-transport-https software-properties-common gnupg \
    && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y powershell

# Copy app
COPY --from=publish /app/publish .

# Install Playwright and Chromium
RUN pwsh playwright.ps1 install --with-deps chromium

# Entry point
ENTRYPOINT ["dotnet", "SneakerWebAPI.dll"]