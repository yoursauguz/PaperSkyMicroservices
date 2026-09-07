<img width="1019" height="631" alt="image" src="https://github.com/user-attachments/assets/2370510b-a21e-45c7-b89b-5ae990e82f14" />

# Domain Analysis

## Overview

The **Basket Service** is a core microservice responsible for managing the shopping cart operations. It acts as the single source of truth for all shopping cart-related metadata, allowing users and other microservices to browse, search, add, and update the products inside the shopping cart.

## Core Responsibilities

- Maintain the master list of products that are inside the shopping cart.
- Expose RESTful endpoints for CRUD operations (Create, Read, Update, Delete) on shopping cart items.
- Support efficient querying by username to enhance the user shopping experience.

## Data Model:

Primary domain models are **ShoppingCart**, **ShoppingCartItem** and **BasketCheckout**

`ShoppingCart` Entity

- **`UserName`** (`string`): The name of the user.
- **`Items`** (`List<ShoppingCartItem>`): List of shopping cart items
- **`TotalPrice`** (`decimal`): Total price of items inside the shopping cart.

`ShoppingCartItem` Entity

- **`Quantity`** (`int`): Number of items
- **`Name`** (`string`): Name of the item
- **`Price`** (`decimal`): Price of each item.
- **`Id`** (`guid`): Unique Id of the item.

## Application Use Cases of Basket Microservice

- Get shopping cart with items
- Store (Upsert-create and update) shopping cart with items
- Delete shopping cart with items

**Grpc Basket operations**

Get discount from a grpc service to apply discount for the products added into the shopping cart, the total price will be calculated after applying the discount

**Async Basket operations**

Checkout basket and publish event to RabbitMQ message broker

## Rest API Endpoints of Basket Microservice

| **Operation** | **HTTP Method** | **Route Pattern** | **Description** |
| --- | --- | --- | --- |
| **Get Basket By Username** | `GET` | `/api/v1/basket/{username}` | Get the basket information including the shopping cart items by username. |
| **Store Basket (insert and update)** | `POST` | `/api/v1/basket/{username}` | Insert or update the basket information by username. |
| **Delete Basket By Username** | `DELETE` | `/api/v1/basket/{username}` | Delete the basket and its content by the username |
| Checkout | `POST` | `/api/v1/basket/checkout` | Checkout the basket with shopping cart items |

## Data Storage & Persistence Strategy

The **Basket Service** uses **PostgreSQL** as its underlying database engine, powered by the **Marten** .NET library. Instead of mapping relational tables with a traditional ORM, Marten treats PostgreSQL as a full-fledged document database by storing .NET entities directly as JSON documents using PostgreSQL's `JSONB` capabilities. 

This service additionally uses **Redis distributed Cache** with Cache-aside (Lazy Loading) strategy to support retrieval of information from the cache, if not found then it fetches from database to minimize database load.

### Why Marten + PostgreSQL?

- **Developer Productivity**: Eliminates complex object-relational impedance matching; the C# model maps directly to the document store.
- **ACID Compliance**: Inherits PostgreSQL's rock-solid transactional guarantees and strong consistency.
- **Powerful Querying**: Supports full LINQ queries over JSON data structures, as well as native JavaScript/SQL querying when needed.

### Why Redis?

• **Sub-millisecond Speed:** It stores data in **RAM** rather than on a slow hard disk, allowing it to read and write data in microseconds.
• **Shared Memory (Distributed):** If you scale your app across multiple servers, they can all share the same centralized Redis cache.
• **Database Relief:** It intercepts repetitive queries, preventing your main database from getting overwhelmed and saving operational costs.
• **Built-in Automation:** It handles data expiration (TTL) and memory cleanup automatically, so you don't have to code it yourself.

### Sample Marten Setup for Basket Service

In the `Program.cs`, initializing Marten looks like this:

C#

```
builder.Services.AddMarten(config =>
{
    config.Connection(builder.Configuration.GetConnectionString("Database")!);
    config.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();
```

### Querying with Marten

C#

```
public class BasketRepository(IDocumentSession session) : IBasketRepository
{
    public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        session.Delete<ShoppingCart>(userName);
        await session.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        var basket = await session.LoadAsync<ShoppingCart>(userName, cancellationToken);

        return basket is null ? throw new BasketNotFoundException(userName) : basket;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        session.Store(cart);
        await session.SaveChangesAsync(cancellationToken);
        return cart;
    }
}
```

### Querying with Redis

C#

```
public class CachedBasketRepository(IBasketRepository repository, IDistributedCache cache) : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        var cachedBasket = await cache.GetStringAsync(userName, cancellationToken);

        if (!string.IsNullOrEmpty(cachedBasket))
            return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket)!;

        var basket =  await repository.GetBasket(userName, cancellationToken);
        await cache.SetStringAsync(userName, JsonSerializer.Serialize(basket), cancellationToken);
        return basket;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        await repository.StoreBasket(basket, cancellationToken);

        await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), cancellationToken);

        return basket;
    }

    public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        await repository.DeleteBasket(userName, cancellationToken);

        await cache.RemoveAsync(userName, cancellationToken);

        return true;
    }
}
```
<img width="363" height="88" alt="image" src="https://github.com/user-attachments/assets/27ef83fe-6160-4876-8594-65538d69860b" />

# Technical Analysis

## Application Architecture Style

To maximize maintainability, scalability, and code clarity within each microservice, the application strictly follows **Vertical Slice Architecture**.

<img width="268" height="309" alt="image" src="https://github.com/user-attachments/assets/9be29e70-bcb9-4c63-aafc-d93b0c7ee37c" />

### Core Architecture Concepts

- **Feature-Centric Organization**: Each feature (e.g., adding a shopping cart item, getting the shopping cart items by username) is packaged into its own completely isolated slice containing its request model, handler or endpoint logic, and validation rules.
- **Minimized Coupling**: Because features are self-contained, changes or refactoring made to one specific feature (like altering the input payload for adding a book) will not accidentally impact or break unrelated features.
- **TDD-Friendly Structure**: Vertical slices align exceptionally well with Test-Driven Development (TDD). You can build, test, and verify a single feature slice entirely in isolation before moving on to the next endpoint.

### Example Folder Structure (BasketService)

Plaintext

```markdown
Basket.API/
│
├── Features/
│   ├── DeleteBasket/
│   │   ├── DeleteBasketEndpoint.cs     (FastEndpoints or Minimal API)
│   │   └── DeleteBasketHandler.cs      (Business logic / command processing)
│   │
│   ├── GetBasket/
│   │   ├── GetBasketEndpoint.cs
│   │   └── GetBasketHandler.cs
│   │
│   └── StoreBasket/
│       ├── StoreBasketEndpoint.cs
│       └── StoreBasketHandler.cs
│
└── Program.cs
```

## Patterns and Principles of Basket Microservices

Here is how these core patterns and principles fit together to power the cloud-based bookstore microservices, specifically tailored to the **Vertical Slice Architecture** and **Marten + PostgreSQL** stack:

### 1. CQRS Pattern (Command Query Responsibility Segregation)

Separates the application operations into two distinct types: **Commands** (which mutate state, like adding or updating a book) and **Queries** (which only read state, like getting books by genre or author).

### 2. Mediator Pattern

Promotes loose coupling by introducing a mediator object (commonly implemented using the `MediatR` library in .NET) that encapsulates how objects interact.

Within each vertical slice, the Minimal API endpoint sends a command or query (e.g., `DeleteBasketCommand`) to the mediator, which routes it directly to the corresponding handler. This decouples the HTTP transport layer completely from the business logic.

### 3. Dependency Injection (DI) in ASP.NET Core

ASP.NET Core’s built-in Inversion of Control (IoC) container manages the lifetime and creation of the application dependencies.

Uses DI to inject infrastructure components—such as Marten's `IDocumentSession` or feature handlers—directly into the Minimal API endpoints or command handlers, ensuring code is modular, testable, and loosely coupled.

### 4. Minimal APIs & Routing in ASP.NET Core

 A lightweight, high-performance alternative to traditional MVC controllers for building fast HTTP endpoints with minimal boilerplate.

Minimal APIs fit **Vertical Slice Architecture** perfectly. Instead of grouping all routing logic into a massive `BasketController`, each feature slice defines its own route extension (e.g., `MapCreateBasketEndpoint`), keeping endpoints close to their handlers.

### 5. ORM / Document Store Pattern (Marten)

A pattern for mapping domain models to database structures. Rather than a traditional relational ORM (like EF Core mapping tables), Marten acts as a document database abstraction over PostgreSQL.

It serializes the C# entities directly into PostgreSQL `JSONB` columns while providing powerful LINQ querying capabilities.

## Essential NuGet Packages for Vertical Slice Architecture

### 1. MediatR

- **Mediator Pattern Implementation:** Decouples message senders from message receivers by routing commands and queries to their respective handlers.
- **Slice Isolation:** Encapsulates business logic within individual vertical slice handlers, keeping application endpoints clean and focused solely on transport concerns.

### 2. Carter

- **Minimal API Enhancement:** Built on top of ASP.NET Core Minimal APIs to organize route definitions into modular, feature-based classes.
- **Boilerplate Reduction:** Eliminates repetitive route mapping configuration by automatically discovering and registering route endpoints across feature folders.

### 3. Marten

- **PostgreSQL Document Database:** Leverages PostgreSQL native `JSONB` capabilities to store .NET entities directly as documents without complex relational ORM mapping.
- **Advanced Querying:** Provides robust LINQ support over document structures, making array-based queries (such as filtering by genres or authors) efficient and seamless.

### 4. Mapster

- **High-Performance Object Mapping:** Copies data between different layers, such as mapping incoming request models to commands or converting domain entities into response DTOs.
- **Speed and Simplicity:** Offers fast execution and convention-based mapping configurations with minimal setup overhead.

### 5. FluentValidation

- **Expressive Validation Rules:** Enables the creation of strongly-typed, chainable validation logic for incoming commands and requests.
- **Pipeline Integration:** Validates incoming payloads before execution reaches the core handlers, ensuring invalid data is rejected early with clear error responses.

<img width="890" height="142" alt="image" src="https://github.com/user-attachments/assets/736a6f13-45ad-452f-a33e-cc94f52a1245" />

## Deployment and Containerization

The Basket microservice and its persistence layer are containerized using Docker and orchestrated via Docker Compose to ensure environment consistency across development and deployment pipelines.

### Container Architecture

- **Basket API Container**: Built using a multi-stage `Dockerfile` that separates the compilation phase from the runtime environment, minimizing image footprint and enhancing security.
- **PostgreSQL Container**: Runs an isolated instance backed by a persistent Docker volume to guarantee that stored document data survives container restarts.
- **Redis Distributed Cache Container**: Runs an isolated instance backed by a persistent Docker volume to guarantee that redis cache container restarts.

<img width="237" height="134" alt="image" src="https://github.com/user-attachments/assets/6cbf02a8-119d-418e-880b-d34bdf9b9f6d" />
