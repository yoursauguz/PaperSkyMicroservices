<img width="1024" height="640" alt="image" src="https://github.com/user-attachments/assets/1343db53-f074-4114-bed5-71684123829f" />

# Domain Analysis

## Overview

The **Catalog Service** is a core microservice responsible for managing the bookstore's inventory. It acts as the single source of truth for all book-related metadata, allowing users and other microservices to browse, search, and retrieve detailed product information.

## Core Responsibilities

- Maintain the master list of available books, genres, and pricing.
- Expose RESTful endpoints for CRUD operations (Create, Read, Update, Delete) on book items.
- Support efficient querying by ID, genre, or author to enhance the user shopping experience.

## Data Model: `Book` Entity

- **`Id`** (`Guid` / `string`): Unique identifier for the book.
- **`Name`** (`string`): The title of the book.
- **`Genres`** (`List<string>`): Listof categories/genres (e.g., `["Fiction", "Sci-Fi"]`).
- **`Description`** (`string`): Detailed synopsis of the book.
- **`Image`** (`string`): Cover image URL or asset path.
- **`Price`** (`decimal`): Monetary price of the book.
- **`NumberOfPages`** (`int`): Total page count.
- **`Authors`** (`List<string>`): List of writers or creators.
- **`AverageRating`** (`double`): Aggregate customer review score.

## Application Use Cases of Catalog Microservice

- Listing books
- Get Book by id
- Get Books by genre
- Get Books by author
- Create new book
- Update book
- Delete book

## Rest API Endpoints of Catalog Microservice

| **Operation**     | **HTTP Method** | **Route Pattern**                 | **Description**                                                     |
| ----------------- | --------------- | --------------------------------- | ------------------------------------------------------------------- |
| **Get All**       | `GET`           | `/api/v1/catalog`                 | Retrieves a paginated list of all books in the catalog.             |
| **Get By ID**     | `GET`           | `/api/v1/catalog/{id}`            | Retrieves details for a specific book by its unique ID.             |
| **Get By Genre**  | `GET`           | `/api/v1/catalog/genre/{genre}`   | Filters books matching a specific genre inside the `Genres` list.   |
| **Get By Author** | `GET`           | `/api/v1/catalog/author/{author}` | Filters books matching a specific author inside the `Authors` list. |
| **Add**           | `POST`          | `/api/v1/catalog`                 | Creates and saves a new book item to the inventory.                 |
| **Edit**          | `PUT`           | `/api/v1/catalog/{id}`            | Updates details of an existing book item.                           |
| **Delete**        | `DELETE`        | `/api/v1/catalog/{id}`            | Removes a book record from the catalog by its ID.                   |

## Data Storage & Persistence Strategy

The **Catalog Service** uses **PostgreSQL** as its underlying database engine, powered by the **Marten** .NET library. Instead of mapping relational tables with a traditional ORM, Marten treats PostgreSQL as a full-fledged document database by storing .NET `Book` entities directly as JSON documents using PostgreSQL's `JSONB` capabilities.

### Why Marten + PostgreSQL?

- **Developer Productivity**: Eliminates complex object-relational impedance matching; the C# `Book` model maps directly to the document store.
- **ACID Compliance**: Inherits PostgreSQL's rock-solid transactional guarantees and strong consistency.
- **Powerful Querying**: Supports full LINQ queries over JSON data structures, as well as native JavaScript/SQL querying when needed.

### Sample Marten Setup for Catalog Service

In the `Program.cs`, initializing Marten looks like this:

C#

```
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database")!);

    // Configure Book document identity and indexes
    options.Schema.For<Book>()
        .Identity(x => x.Id)
        .Index(x => x.Genres)
        .Index(x => x.Authors);
})
.UseLightweightSessions();
```

### Querying with Marten (e.g., Get By Genre)

Because `Genres` is stored as a string array (`string[]`) inside the JSON document, Marten makes array-contains queries seamless using standard LINQ:

C#

```
public async Task<IEnumerable<Book>> GetBooksByGenreAsync(IDocumentSession session, string genre)
{
    return await session.Query<Book>()
        .Where(b => b.Genres.Contains(genre))
        .ToListAsync();
}
```
<img width="276" height="93" alt="image" src="https://github.com/user-attachments/assets/2fca9930-710a-41b5-a26b-24fbd94e5ccc" />

# Technical Analysis

## Application Architecture Style

To maximize maintainability, scalability, and code clarity within each microservice, the application strictly follows **Vertical Slice Architecture**.

<img width="271" height="297" alt="image" src="https://github.com/user-attachments/assets/76a03d0e-ae7c-41f6-97d6-b30e1477225e" />

### Core Architecture Concepts

- **Feature-Centric Organization**: Each feature (e.g., creating a book, getting books by author, updating book details) is packaged into its own completely isolated slice containing its request model, handler or endpoint logic, and validation rules.
- **Minimized Coupling**: Because features are self-contained, changes or refactoring made to one specific feature (like altering the input payload for adding a book) will not accidentally impact or break unrelated features.
- **TDD-Friendly Structure**: Vertical slices align exceptionally well with Test-Driven Development (TDD). You can build, test, and verify a single feature slice entirely in isolation before moving on to the next endpoint.

### Example Folder Structure (Catalog Service)

Plaintext

```markdown
Catalog.API/
│
├── Features/
│ ├── CreateBook/
│ │ ├── CreateBookEndpoint.cs (FastEndpoints or Minimal API)
│ │ ├── CreateBookHandler.cs (Business logic / command processing)
│ │ └── CreateBookCommand.cs (Request contract & validation)
│ │
│ ├── GetBookById/
│ │ ├── GetBookByIdEndpoint.cs
│ │ └── GetBookByIdHandler.cs
│ │
│ ├── GetBooksByGenre/
│ │ ├── GetBooksByGenreEndpoint.cs
│ │ └── GetBooksByGenreHandler.cs
│ │
│ ├── GetBooksByAuthor/
│ │ ├── GetBooksByAuthorEndpoint.cs
│ │ └── GetBooksByAuthorHandler.cs
│ │
│ ├── UpdateBook/
│ │ ├── UpdateBookEndpoint.cs
│ │ └── UpdateBookHandler.cs
│ │
│ └── DeleteBook/
│ ├── DeleteBookEndpoint.cs
│ └── DeleteBookHandler.cs
│
└── Program.cs
```

## Patterns and Principles of Catalog Microservices

Here is how these core patterns and principles fit together to power the cloud-based bookstore microservices, specifically tailored to the **Vertical Slice Architecture** and **Marten + PostgreSQL** stack:

### 1. CQRS Pattern (Command Query Responsibility Segregation)

Separates ther application operations into two distinct types: **Commands** (which mutate state, like adding or updating a book) and **Queries** (which only read state, like getting books by genre or author).

### 2. Mediator Pattern

Promotes loose coupling by introducing a mediator object (commonly implemented using the `MediatR` library in .NET) that encapsulates how objects interact.

Within each vertical slice, the Minimal API endpoint sends a command or query (e.g., `CreateBookCommand`) to the mediator, which routes it directly to the corresponding handler. This decouples the HTTP transport layer completely from the business logic.

### 3. Dependency Injection (DI) in ASP.NET Core

ASP.NET Core’s built-in Inversion of Control (IoC) container manages the lifetime and creation of the application dependencies.

Uses DI to inject infrastructure components—such as Marten's `IDocumentSession` or feature handlers—directly into the Minimal API endpoints or command handlers, ensuring code is modular, testable, and loosely coupled.

### 4. Minimal APIs & Routing in ASP.NET Core

A lightweight, high-performance alternative to traditional MVC controllers for building fast HTTP endpoints with minimal boilerplate.

Minimal APIs fit **Vertical Slice Architecture** perfectly. Instead of grouping all routing logic into a massive `CatalogController`, each feature slice defines its own route extension (e.g., `MapCreateBookEndpoint`), keeping endpoints close to their handlers.

### 5. ORM / Document Store Pattern (Marten)

A pattern for mapping domain models to database structures. Rather than a traditional relational ORM (like EF Core mapping tables), Marten acts as a document database abstraction over PostgreSQL.

It serializes the C# `Book` entities directly into PostgreSQL `JSONB` columns while providing powerful LINQ querying capabilities (making array checks on `Genres` and `Authors` seamless).

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

<img width="421" height="100" alt="image" src="https://github.com/user-attachments/assets/05f27027-9837-4077-99fd-34bd951cfcf7" />

## Deployment and Containerization

The Catalog microservice and its persistence layer are containerized using Docker and orchestrated via Docker Compose to ensure environment consistency across development and deployment pipelines.

### Container Architecture

- **Catalog API Container**: Built using a multi-stage `Dockerfile` that separates the compilation phase from the runtime environment, minimizing image footprint and enhancing security.
- **PostgreSQL Container**: Runs an isolated instance backed by a persistent Docker volume to guarantee that stored document data survives container restarts.

<img width="171" height="102" alt="image" src="https://github.com/user-attachments/assets/03249b26-12da-4ef1-bb76-e799364620c5" />


