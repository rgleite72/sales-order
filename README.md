SalesOrder API
API for sales order management built with modern backend practices.
---
🚀 Tech Stack
.NET 8
ASP.NET Core Web API
PostgreSQL
Entity Framework Core
Docker
Azure DevOps (CI)
---
📌 Overview
The SalesOrder API is responsible for managing sales orders, including:
Order creation
Order listing
Order details retrieval
Order status management (confirm/cancel)
This project simulates a real-world backend service using clean
architecture and production-ready practices.
---
🧱 Architecture
The solution follows a layered architecture:
Domain
Core business entities
Business rules
Application
DTOs
Service interfaces
Business use cases
Infrastructure
EF Core configuration
Repository implementations
Database access
API
Controllers
Dependency Injection
HTTP endpoints
---
📂 Project Structure
    src/
    ├── SalesOrder.Api
    ├── SalesOrder.Application
    ├── SalesOrder.Domain
    ├── SalesOrder.Infrastructure

    tests/
    ├── SalesOrder.IntegrationTests
    ├── SalesOrder.UnitTests

---
⚙️ Main Features
Create sales orders
List orders with pagination
Get order details
Confirm order
Cancel order
Integration-ready architecture (ProductCatalog)
---
🔌 Main Endpoints
Method   Endpoint                   Description
---
POST     /api/orders                Create order
GET      /api/orders/{id}           Get order by ID
GET      /api/orders                List orders
POST     /api/orders/{id}/confirm   Confirm order
POST     /api/orders/{id}/cancel    Cancel order
---
📦 Running Locally
``` bash
git clone https://github.com/rgleite72/sales-order.git
cd sales-order

docker compose up -d

dotnet ef database update --project src/SalesOrder.Infrastructure --startup-project src/SalesOrder.Api

dotnet run --project src/SalesOrder.Api

# Swagger:
http://localhost:5000/swagger
```
---
🐳 Docker
Docker Compose is used to provision:
PostgreSQL database
---
🔄 Continuous Integration
This project uses Azure DevOps Pipelines.
Pipeline executes:
Restore dependencies
Build solution
Run tests
---
🧪 Tests
The project includes:
Unit tests
Integration tests
Executed automatically via pipeline.
---
📊 Project Status
Version: V1
Core SalesOrder API
Docker environment
CI pipeline with Azure DevOps
PR validation with build checks
---
