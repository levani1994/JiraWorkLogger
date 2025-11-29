# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["JiraWorkLogger.csproj", "./"]
RUN dotnet restore "JiraWorkLogger.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "JiraWorkLogger.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "JiraWorkLogger.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published app
COPY --from=publish /app/publish .

# Set environment variable for port
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "JiraWorkLogger.dll"]

