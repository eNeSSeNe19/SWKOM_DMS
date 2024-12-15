# SWKOM_DMS
Project Paperless: Document Management System (DMS)
The goal of this project is a Document Management System (DMS) for archiving documents in a FileStore, with automatic OCR (queue for OC-recognition), tagging and full text search (ElasticSearch). Users can upload documents, (fuzzy) search documents and manage the documents metadata. DMS performs OCR automatically and creates an index in ElasticSearch.

Sprints
Sprint 1: Project-Setup, REST API
Sprint 2: WebUI
Sprint 3: Data Access Layer (DAL), PostgreSQL
Sprint 4: RabbitMQ Integartion
Sprint 5: OCR Service
Sprint 6: ElasticSearch Integration
Sprint 7: Finalization

Project Setup
Services
For this project, you need to have the following services running:

PostgreSQL
RabbitMQ
Elasticsearch
nginx

Run
Open terminal and go to project folder containing docker compose file.

Build & Run project with: docker compose up --build

Integration Tests
is in the test project and can be executed as a normal such
