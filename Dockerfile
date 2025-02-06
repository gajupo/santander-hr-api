# Build and restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY santander-hr-api.sln ./
COPY ./api/*.csproj ./api/
COPY ./tests/santander-hr-api.tests/*.csproj ./tests/santander-hr-api.tests/

RUN dotnet restore

COPY . .

RUN dotnet build -c Release

RUN dotnet test -c Release --no-build

# Publish API only
RUN dotnet publish ./api/*.csproj -c Release -o /app/publish

#Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

# Expose ports
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "santander-hr-api.dll"]