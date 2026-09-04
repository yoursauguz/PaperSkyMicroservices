<img width="1024" height="640" alt="image" src="https://github.com/user-attachments/assets/72b3397d-15ff-459c-9655-c72a0c4df3b9" />

# Domain Analysis

## Overview

The Discount service is a grpc based microservice responsible for managing the discount coupons. It acts as the single source of truth for all the coupon-related metadata, allowing users and other microservices to add, remove, and retrieve coupon information for a product.

## Core Responsibilities

- Maintain the master list of discount coupons for a product.
- Expose grpc endpoints for CRUD operations (Create, Read, Update, Delete) on discount coupons.

## Data Model: `Coupon`Entity

- **`Id`** (`int`): Unique identifier for the coupon.
- **`ProductName`** (`string`): The title of the product.
- **`Description`** (`string`): Detailed information about the coupon.
- **`Amount`** (`int`): Applicable discount amount.

## Application Use Cases of Discount Microservice

- Get discount coupon by product name
- Create a discount coupon
- Update a discount coupon
- Delete a discount coupon

## Grpc Endpoints of Discount Microservice

| **Operation** | **RPC** | **Description** |
| --- | --- | --- |
| **Get**  | `/GetDiscount` | Retrieves a discount coupon matching the product name. |
| **Add** | `/CreateDiscount` | Creates and saves a new discount coupon |
| **Edit** | `/UpdateDiscount` | Updates details of an existing discount coupon. |
| **Delete** | `/DeleteDiscount` | Removes a discount coupon by the product name. |

## Data Storage & Persistence Strategy

The **Discount Service** uses **SQLite** as its underlying database engine with a traditional ORM. It is chosen for its simplicity and efficiency for small-scale data like discount coupons. it is embedded within the application, reducing the need for additional infrastructure.

<img width="256" height="92" alt="image" src="https://github.com/user-attachments/assets/be27eb20-67f7-471e-af23-31d7867f248f" />

# Technical Analysis

## Application Architecture Style

Discount microservice follows the Traditional **N-Layer** **Architecture**. this is a classic structural pattern designed to separate concerns across horizontal layers. Each layer has a distinct responsibility and strictly communicates with adjacent layers.

**Core Concepts**

<img width="257" height="209" alt="image" src="https://github.com/user-attachments/assets/87eb2545-52f1-4530-a25b-dfdfd1748046" />

### Core Architecture Concepts

- **Separation of Concerns:** Each layer handles a specific task-UI, business logic, or data acces-preventing logic from tangling together.
- **Top-Down Dependency:** Dependencies flow downward in one direction
- **Abstraction & Loose Coupling:** Interfaces defined in lower layers allow upper layers to interact with logic without needing to know concrete implementation details.
- **Reusability & Maintainability:** Replacing a database or UI framework only requires changes to the target layer, leaving the core domain and business logic untouched.

### Example Folder Structure

MySolution/
│
├── src/
│   ├── MySolution.Web/                     # Presentation Layer (API / MVC)
│   │   ├── Controllers/
│   │   │   └── ProductsController.cs
│   │   ├── Models/                         # Request / Response DTOs
│   │   │   └── ProductRequestDto.cs
│   │   ├── Program.cs                      # Dependency Injection & Pipeline Setup
│   │   └── appsettings.json
│   │
│   ├── MySolution.Business/                # Business Logic Layer (BLL)
│   │   ├── Services/
│   │   │   ├── ProductService.cs
│   │   │   └── Interfaces/
│   │   │       └── IProductService.cs
│   │   └── Validators/
│   │       └── ProductValidator.cs
│   │
│   ├── MySolution.DataAccess/              # Data Access Layer (DAL)
│   │   ├── Context/
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Entities/                       # Database Tables Models
│   │   │   └── ProductEntity.cs
│   │   ├── Repositories/
│   │   │   ├── ProductRepository.cs
│   │   │   └── Interfaces/
│   │   │       └── IProductRepository.cs
│   │   └── Migrations/
│   │
│   └── MySolution.Core/                    # Common / Cross-Cutting Concerns
│       ├── Exceptions/
│       │   └── NotFoundException.cs
│       ├── Helpers/
│       └── Constants/
│
└── tests/
├── MySolution.Business.Tests/
└── MySolution.DataAccess.Tests/

## Patterns and Principles of Discount Microservice

### 1. gRPC ProtoBuf filed Endpoints

In gRPC, API endpoints are defined inside **Protocol Buffer (`.proto`) schema files** by declaring a `service` block containing `rpc` (Remote Procedure Call) definitions.

Unlike REST APIs that map endpoints to HTTP methods and paths (e.g., `GET /api/products`), gRPC routes requests directly to specific method names generated from the `.proto` file.

### 2. Entity Framework Core

**Entity Framework Core (EF Core)** is an open-source, lightweight, cross-platform Object-Relational Mapper (ORM) for .NET. It allows developers to work with databases using strongly typed .NET objects, eliminating the need to write most raw SQL data-access code.

### 3. SQLite Database

**SQLite** is an in-process, serverless, zero-configuration SQL database engine. Unlike PostgreSQL or SQL Server, SQLite does not run as a separate background daemon; instead, the entire database engine is compiled directly into your application as a lightweight library, storing all data (tables, indexes, and schema) inside a single cross-platform file on disk.

## Essential NuGet Packages

### 1. Microsoft.EntityFrameworkCore.Sqlite

- **Purpose:** The official EF Core database provider for SQLite.
- **Role:** Translates LINQ queries into SQLite-compatible SQL, configures relational table mapping, handles database connections via `Microsoft.Data.Sqlite`, and enables `UseSqlite()` in `Program.cs`.

### 2. Microsoft.EntityFrameworkCore.Tools

- **Purpose:** Design-time tooling for Entity Framework Core.
- **Role:** Enables CLI and PowerShell commands (e.g., `Add-Migration`, `Update-Database`, or `dotnet ef migrations add`) to auto-generate schema migration files and apply database updates directly from Visual Studio or the command line.

### 3. Aspnetcore.Grpc

- **`Grpc.AspNetCore`** is Microsoft’s official framework package for hosting high-performance **gRPC services inside ASP.NET Core** applications.
- It integrates gRPC into the ASP.NET Core middleware ecosystem, giving you built-in routing, dependency injection, logging, authentication, and authorization for gRPC endpoints over HTTP/2.

### 4. Mapster

- **High-Performance Object Mapping:** Copies data between different layers, such as mapping incoming request models to commands or converting domain entities into response DTOs.
- **Speed and Simplicity:** Offers fast execution and convention-based mapping configurations with minimal setup overhead.

<img width="595" height="101" alt="image" src="https://github.com/user-attachments/assets/ecb00007-7684-434b-a2bf-0c43dde37c52" />

## Deployment and Containerization

The Discount microservice and its persistence layer are containerized using Docker and orchestrated via Docker Compose to ensure environment consistency across development and deployment pipelines.

### Container Architecture

- **Discount Grpc Container**: Built using a multi-stage `Dockerfile` that separates the compilation phase from the runtime environment, minimizing image footprint and enhancing security.

<img width="214" height="137" alt="image" src="https://github.com/user-attachments/assets/8e9d264c-e6dc-4c4c-99d3-162811c23444" />
