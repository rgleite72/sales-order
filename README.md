# SalesOrder API

API for sales order management built with:

* .NET 8
* ASP.NET Core
* PostgreSQL
* Entity Framework Core
* Docker
* Azure DevOps Pipelines (CI - restore/build)


## Overview




## Architecture

The solution follows a layered architecture:

### Domain

* Entities
* Business rules

### Application

* DTOs
* Service contracts
* Application services

### Infrastructure

* EF Core persistence
* Repository implementations
* Database configuration

### API

* Controllers
* Dependency Injection
* HTTP endpoints


## Project Structure

src/

 ├── SalesOrder.Api

 ├── SalesOrder.Application

 ├── SalesOrder.Domain

 ├── SalesOrder.Infrastructure


## Main Features


## Main Endpoints






## Running Locally

### 1. Clone repository




### 2. Start infrastructure

docker compose up -d


### 3. Apply migrations

dotnet ef database update --project src/SalesOrder.Infrastructure --startup-project src/SalesOrder.Api


### 4. Run application

dotnet run --project src/SalesOrder.Api



## Docker

The project uses Docker Compose to provision PostgreSQL locally.

Main service:

* PostgreSQL database


## Continuous Integration

This project uses Azure DevOps Pipelines for Continuous Integration.

Pipeline executes:

* Restore dependencies
* Build solution


## Technology Stack

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* Docker
* Azure DevOps


## Status

Current version:

V1 - Sales Order core + Docker + Initial CI

## teste pipeline
