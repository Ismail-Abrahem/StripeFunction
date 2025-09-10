# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and .env
COPY *.csproj ./
RUN dotnet restore

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Use port 80 instead of 8080 for simpler mapping
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "WalletApi.dll"]