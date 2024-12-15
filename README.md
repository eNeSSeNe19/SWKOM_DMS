# SWKOM_DMS

## Project Paperless: Document Management System (DMS)

The goal of this project is to develop a Document Management System (DMS) for archiving documents in a FileStore. The system will feature automatic OCR (Optical Character Recognition) through a queue for OCR recognition, tagging, and full-text search powered by ElasticSearch. Users can upload documents, perform fuzzy searches, and manage document metadata. The DMS will automatically perform OCR and create an index in ElasticSearch.

## Sprints

- **Sprint 1**: Project Setup, REST API
- **Sprint 2**: Web UI
- **Sprint 3**: Data Access Layer (DAL), PostgreSQL
- **Sprint 4**: RabbitMQ Integration
- **Sprint 5**: OCR Service
- **Sprint 6**: ElasticSearch Integration
- **Sprint 7**: Finalization

## Project Setup

### Services

To run this project, the following services must be running:

- **PostgreSQL**
- **RabbitMQ**
- **Elasticsearch**
- **nginx**

### Run

1. Open a terminal and navigate to the project folder that contains the `docker-compose.yml` file.
2. Build and run the project with the following command:

   ```bash
   docker compose up --build
# Integration Tests
is in the test project and can be executed as a normal such
