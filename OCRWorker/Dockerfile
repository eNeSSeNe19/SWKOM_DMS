# Use the .NET 8.0 runtime as the base image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use the .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj file and restore dependencies
COPY ["OCRWorker.csproj", "./"]
COPY log4net.config ./ 
RUN dotnet restore "./OCRWorker.csproj"

# Copy everything else and build
COPY . . 
WORKDIR "/src"
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Add runtime dependencies for Tesseract and Leptonica
FROM base AS final
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    tesseract-ocr \
    ghostscript && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "OCRWorker.dll"]
