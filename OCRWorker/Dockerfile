# Basis-Image mit .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build


# Arbeitsverzeichnis erstellen
WORKDIR /app

# Kopiere die Projektdateien in den Container
COPY . ./

# Wiederherstellen der Abhängigkeiten und Build des Projekts
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Basis-Image für die Laufzeit
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Installiere zusätzliche Abhängigkeiten (Tesseract, Ghostscript)
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    tesseract-ocr \
    ghostscript && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Arbeitsverzeichnis festlegen
WORKDIR /app

# Dateien aus dem Build-Container kopieren
COPY --from=build /app/publish .

# Standard-Kommando zum Starten der Anwendung
ENTRYPOINT ["dotnet", "OCRWorker.dll"]
