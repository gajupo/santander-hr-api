# ---------------------------------------------------------
# STAGE 1: Build & Restore using the .NET 8 SDK
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY santander-hr-api.sln ./
COPY ./api/*.csproj ./api/
COPY ./tests/santander-hr-api.tests/*.csproj ./tests/santander-hr-api.tests/

# Restore all dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build solution
RUN dotnet build -c Release

# Run tests
RUN dotnet test -c Release --no-build

# Publish API only
RUN dotnet publish ./api/*.csproj -c Release -o /app/publish

# ---------------------------------------------------------
# STAGE 2: Runtime Image
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published output from the previous stage
COPY --from=build /app/publish .

# Expose ports (optional, but helps documentation)
EXPOSE 80
EXPOSE 443

# Run the application
ENTRYPOINT ["dotnet", "santander-hr-api.dll"]