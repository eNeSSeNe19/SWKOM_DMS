# Use the .NET 8.0 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the .NET 8.0 SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SWKOM_DMS.csproj", "./"]
RUN dotnet restore "./SWKOM_DMS.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SWKOM_DMS.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "SWKOM_DMS.csproj" -c Release -o /app/publish

# Final stage, run the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SWKOM_DMS.dll"]
