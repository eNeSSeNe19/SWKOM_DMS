# Use the .NET 8.0 runtime as the base image for running the application
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use the .NET SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file(s) and restore dependencies
COPY ["OCRWorker.csproj", "./"]
RUN dotnet restore "./OCRWorker.csproj"

# Copy the entire source code and build the application
COPY . .
WORKDIR "/src"
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final image for running the application
FROM base AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libleptonica-dev \
        libtesseract-dev \
        ghostscript \
    && rm -rf /var/lib/apt/lists/*
RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so
WORKDIR /app/x64
RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so
WORKDIR /app
EXPOSE 80


# Add tessdata files to the runtime image
COPY ./tessdata /app/tessdata

# Set the TESSDATA_PREFIX environment variable
ENV TESSDATA_PREFIX=/app/tessdata

# Copy the published files to the runtime image
COPY --from=publish /app/publish .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "OCRWorker.dll"]



