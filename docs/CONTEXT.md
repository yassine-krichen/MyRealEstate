# EstateFlow — Real Estate Management System

> **Technical Report & Architecture Documentation**
> Version 1.0 | February 2026

---

## Table of Contents

1. [Executive Summary](#-executive-summary)
2. [Architecture & Design Patterns](#-architecture--design-patterns)
3. [Domain Layer — Business Logic](#-domain-layer--business-logic)
4. [Application Layer — Use Cases & CQRS](#-application-layer--use-cases--cqrs)
5. [Infrastructure Layer — Data Access & Services](#-infrastructure-layer--data-access--services)
6. [Presentation Layer — MVC & Views](#-presentation-layer--mvc--views)
7. [Feature Breakdown](#-feature-breakdown)
8. [Security & Authorization](#-security--authorization)
9. [Data Model & Persistence](#-data-model--persistence)
10. [Cross-Cutting Concerns](#-cross-cutting-concerns)
11. [Technology Stack](#-technology-stack)
12. [Project Metrics](#-project-metrics)
13. [Running the Application](#-running-the-application)
14. [Branding & Design System](#-branding--design-system)

---

## 1. Executive Summary

**EstateFlow** is a production-grade ASP.NET Core 8.0 MVC application for end-to-end real estate management. The system covers the full property lifecycle — from listing creation and visitor browsing, through inquiry management and agent-visitor communication, to deal closure with commission tracking and analytics.

### What Sets This Project Apart

- **Clean Architecture** with strict layer separation — the Domain layer has zero external dependencies
- **CQRS via MediatR** with a full pipeline: validation, logging, and handler execution
- **Rich Domain Model** — entities encapsulate business rules through domain methods (e.g., `Property.Publish()`, `Inquiry.AssignToAgent()`, `Deal.Complete()`) rather than exposing raw state mutation
- **Value Objects** (`Money`, `Address`) enforce invariants at the type level — you cannot create a negative price or an address without a city
- **Result Pattern** for explicit error handling without exceptions in the command flow
- **Dual-audience system** — an authenticated admin/agent panel and a public-facing property browser with a token-based inquiry tracking system that requires no visitor registration
- **9 controllers** (5 admin, 4 public) with **33 views** across admin and public areas
- **8 entity types**, **4 enums**, **2 value objects**, **22 command/query handlers**, **10 validators**, **8 EF Core entity configurations**
- **938-line database seeder** generating realistic interconnected sample data across all entities

---

## 2. Architecture & Design Patterns

### 2.1 Clean Architecture (Onion Architecture)

```
┌─────────────────────────────────────────────────────────────────────┐
│                     MyRealEstate.Web                                │
│              ASP.NET Core MVC · Controllers · Views                 │
│                  Razor · Bootstrap 5 · Middleware                   │
├────────────────────────┬────────────────────────────────────────────┤
│ MyRealEstate.          │         MyRealEstate.                      │
│ Infrastructure         │         Application                        │
│ EF Core · SQLite       │         MediatR · CQRS                     │
│ Identity · Services    │         FluentValidation · AutoMapper      │
│ Repositories           │         Commands · Queries · DTOs           │
├────────────────────────┴────────────────────────────────────────────┤
│                     MyRealEstate.Domain                              │
│          Entities · Value Objects · Enums · Interfaces               │
│                    ZERO external dependencies                        │
└─────────────────────────────────────────────────────────────────────┘
```

**Dependency flow** — outer layers depend on inner layers, never the reverse:

| Layer          | Project                       | Depends On                                  |
| -------------- | ----------------------------- | ------------------------------------------- |
| Domain         | `MyRealEstate.Domain`         | Nothing (pure C#)                           |
| Application    | `MyRealEstate.Application`    | Domain                                      |
| Infrastructure | `MyRealEstate.Infrastructure` | Application, Domain                         |
| Presentation   | `MyRealEstate.Web`            | Application (never Infrastructure directly) |

The **Dependency Inversion Principle** is strictly enforced: the Application layer defines interfaces (`IApplicationDbContext`, `IFileStorage`, `IEmailSender`, `IPropertyViewRepository`, `ICurrentUserService`), and Infrastructure provides concrete implementations. The Web layer never references Infrastructure types — it resolves everything through DI.

### 2.2 Design Patterns Employed

| Pattern                  | Implementation                                                       | Where                                                              |
| ------------------------ | -------------------------------------------------------------------- | ------------------------------------------------------------------ |
| **CQRS**                 | Commands (writes) and Queries (reads) as separate request types      | `Application/Commands/`, `Application/Queries/`                    |
| **Mediator**             | MediatR dispatches requests to handlers                              | All controllers send `IRequest<T>` via `IMediator`                 |
| **Pipeline Behaviors**   | Cross-cutting concerns injected into the MediatR pipeline            | `ValidationBehavior<,>`, `LoggingBehavior<,>`                      |
| **Repository**           | Abstracted data access for analytics                                 | `IPropertyViewRepository` → `PropertyViewRepository`               |
| **Unit of Work**         | EF Core `DbContext` tracks changes, single `SaveChangesAsync` commit | `ApplicationDbContext`                                             |
| **Value Object**         | Immutable types with equality by value                               | `Money`, `Address`                                                 |
| **Result Pattern**       | Explicit success/failure returns without exceptions                  | `Result`, `Result<T>` for property commands                        |
| **Soft Delete**          | Logical deletion with global query filters                           | `ISoftDelete` interface, EF global filters                         |
| **Domain Methods**       | Business rules enforced inside entities                              | `Property.Publish()`, `Deal.Complete()`, `Inquiry.AssignToAgent()` |
| **Specification/Filter** | Query-level filtering with composable predicates                     | Query handlers with multi-parameter filters                        |
| **Factory Method**       | Static creation via `PaginatedList<T>.Create(...)`                   | Pagination helper                                                  |
| **Strategy**             | Swappable service implementations                                    | `IFileStorage` → `LocalFileStorage` (could swap to cloud)          |

### 2.3 CQRS Pipeline Architecture

Every request flows through this pipeline before reaching the handler:

```
Controller
  │
  ▼
IMediator.Send(command/query)
  │
  ▼
┌──────────────────────────┐
│  ValidationBehavior<,>   │  ← Runs ALL registered FluentValidation validators
│  (IPipelineBehavior)     │     Throws ValidationException if any rule fails
└──────────┬───────────────┘
           │
           ▼
┌──────────────────────────┐
│   LoggingBehavior<,>     │  ← Logs "Handling {RequestName}" / "Handled successfully"
│   (IPipelineBehavior)    │     Logs errors at Error level, then rethrows
└──────────┬───────────────┘
           │
           ▼
┌──────────────────────────┐
│    Command/Query         │  ← Actual business logic
│    Handler               │     Returns result to controller
└──────────────────────────┘
```

This means **every** command and query automatically gets:

1. Input validation (with detailed error messages per field)
2. Structured logging with request names
3. Error logging with exception details

No handler needs to implement validation or logging — it's handled once, globally.

---

## 3. Domain Layer — Business Logic

> `MyRealEstate.Domain` — 0 NuGet dependencies (only references are `Microsoft.AspNetCore.Identity.EntityFrameworkCore` for `IdentityUser<Guid>`)

### 3.1 Entity Hierarchy

All entities inherit from `BaseEntity`, which implements `IAuditable`:

```
BaseEntity (abstract)
  ├── Id : Guid
  ├── CreatedAt : DateTime          ← Auto-set by DbContext on insert
  └── UpdatedAt : DateTime?         ← Auto-set by DbContext on update
```

Entities implementing `ISoftDelete` gain:

```
ISoftDelete
  ├── IsDeleted : bool
  └── DeletedAt : DateTime?
```

### 3.2 Core Entities

#### Property (20 properties, 4 domain methods)

The central entity. Represents a real estate listing with rich domain behavior.

| Property                | Type                         | Purpose                                               |
| ----------------------- | ---------------------------- | ----------------------------------------------------- |
| `Id`                    | `Guid`                       | Primary key (from BaseEntity)                         |
| `Title`                 | `string`                     | Listing headline                                      |
| `Slug`                  | `string?`                    | URL-friendly identifier                               |
| `Description`           | `string`                     | Full HTML description                                 |
| `Price`                 | `Money`                      | Value object (amount + currency)                      |
| `PropertyType`          | `string`                     | House, Apartment, Villa, Studio, Duplex, Land         |
| `Status`                | `PropertyStatus`             | Draft → Published → UnderOffer → Sold/Rented/Archived |
| `Address`               | `Address`                    | Value object (line1, city, country, coords)           |
| `Bedrooms`              | `int`                        | Number of bedrooms                                    |
| `Bathrooms`             | `int`                        | Number of bathrooms                                   |
| `AreaSqM`               | `decimal`                    | Total area in square meters                           |
| `AgentId`               | `Guid?`                      | FK to assigned agent                                  |
| `ClosedDealId`          | `Guid?`                      | FK to the deal that closed this property              |
| `ViewsCount`            | `int`                        | Cached view counter                                   |
| `IsDeleted / DeletedAt` | `bool / DateTime?`           | Soft delete fields                                    |
| `Images`                | `ICollection<PropertyImage>` | Navigation — 1:N                                      |
| `Inquiries`             | `ICollection<Inquiry>`       | Navigation — 1:N                                      |
| `Views`                 | `ICollection<PropertyView>`  | Navigation — 1:N (analytics)                          |

**Domain Methods:**

- `CanBePublished()` — validates Title, Description, Price > 0, Address exists, not deleted
- `Publish()` — transitions status to Published (throws `InvalidOperationException` if validation fails)
- `MarkAsSold(Guid dealId)` — transitions to Sold, records closing deal
- `SoftDelete()` — marks as logically deleted with timestamp

#### Inquiry (14 properties, 6 domain methods)

Models a potential buyer's inquiry about a property, with a full state machine:

```
New ──→ Assigned ──→ InProgress ──→ Answered ──→ Closed
                                                    │
                                                    ▼
                                                 Reopen()
                                                    │
                                                    ▼
                                               InProgress
```

**Key fields:** `VisitorName`, `VisitorEmail`, `VisitorPhone`, `InitialMessage`, `AccessToken` (32-char cryptographic token for visitor tracking), `AssignedAgentId`, `RelatedDealId`, `ClosedAt`

**Domain Methods:**

- `AssignToAgent(Guid agentId)` — validates status, handles New→Assigned transition
- `StartProgress()` — transitions from New/Assigned to InProgress
- `MarkAsAnswered()` — transitions from InProgress/Assigned to Answered
- `Close(Guid? dealId)` — closes inquiry, optionally links to a deal
- `Reopen()` — reverses a closed inquiry back to InProgress, clears deal link
- `SoftDelete()` — logical deletion

The `Reopen()` method is critical for the CancelDeal workflow — when a deal falls through, the linked inquiry is automatically reopened so the agent can continue the conversation.

#### Deal (15 properties, 3 domain methods)

Tracks a property sale from negotiation through completion:

| Field                   | Type         | Purpose                                               |
| ----------------------- | ------------ | ----------------------------------------------------- |
| `PropertyId`            | `Guid`       | Which property is being sold                          |
| `InquiryId`             | `Guid?`      | Optional link to the originating inquiry              |
| `AgentId`               | `Guid`       | The closing agent                                     |
| `BuyerName/Email/Phone` | `string?`    | Buyer contact information                             |
| `SalePrice`             | `decimal`    | Final sale price                                      |
| `CommissionPercent`     | `decimal?`   | Agent's commission rate                               |
| `CommissionAmount`      | `decimal?`   | Calculated commission value                           |
| `Status`                | `DealStatus` | Pending → Completed / Cancelled                       |
| `Notes`                 | `string?`    | Free-text notes (appended on completion/cancellation) |
| `ClosedAt`              | `DateTime?`  | When the deal was finalized                           |

**Domain Methods:**

- `CalculateCommission()` — `CommissionAmount = SalePrice × (CommissionPercent / 100)`
- `Complete()` — validates Pending status, sets Completed + ClosedAt
- `Cancel()` — validates not already Completed, sets Cancelled

#### Supporting Entities

| Entity                | Fields                                                                                                              | Purpose                                                            |
| --------------------- | ------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `PropertyImage`       | `PropertyId`, `FilePath`, `FileName`, `IsMain`, `Width`, `Height`, `FileSize` + `SetAsMain()`/`UnsetMain()` methods | Multiple images per property with main image designation           |
| `PropertyView`        | `PropertyId`, `SessionId`, `IpAddress`, `UserAgent`, `ViewedAt`                                                     | Analytics — tracks each page view for the "Most Visited" dashboard |
| `ConversationMessage` | `InquiryId`, `SenderType`, `SenderUserId`, `Body`, `IsInternalNote`                                                 | Agent↔visitor threaded conversation with internal agent-only notes |
| `ContentEntry`        | `Key` (unique), `HtmlValue`, `UpdatedByUserId`                                                                      | CMS key-value store for dynamic site content                       |
| `User`                | Extends `IdentityUser<Guid>` with `FullName`, `IsActive`, `LastLoginAt` + soft delete                               | Authentication subject with audit trail                            |

### 3.3 Value Objects

#### Money

```csharp
Money(decimal amount, string currency = "TND")
```

- **Invariants:** Amount ≥ 0, Currency not empty (auto-uppercased)
- **Equality:** By value (Amount + Currency)
- **Mapped as:** EF Owned Type → columns `Price` (precision 18,2) + `Currency` (max 3 chars)

#### Address

```csharp
Address(string line1, string city, string country = "Tunisia",
        string? line2, string? state, string? postalCode,
        decimal? latitude, decimal? longitude)
```

- **Invariants:** Line1 and City are required (throws `ArgumentNullException`)
- **Equality:** By value (Line1 + Line2 + City + State + PostalCode + Country)
- **Helper:** `GetFullAddress()` returns formatted multi-line string
- **Mapped as:** EF Owned Type → 8 columns including GPS coordinates (precision 10,7)

### 3.4 Enums

| Enum             | Values                                                                             |
| ---------------- | ---------------------------------------------------------------------------------- |
| `PropertyStatus` | `Draft(0)`, `Published(1)`, `UnderOffer(2)`, `Sold(3)`, `Rented(4)`, `Archived(5)` |
| `InquiryStatus`  | `New(0)`, `Assigned(1)`, `InProgress(2)`, `Answered(3)`, `Closed(4)`               |
| `DealStatus`     | `Pending(0)`, `Completed(1)`, `Cancelled(2)`                                       |
| `SenderType`     | `Visitor(0)`, `Agent(1)`, `Admin(2)`, `System(3)`                                  |

---

## 4. Application Layer — Use Cases & CQRS

> `MyRealEstate.Application` — depends only on Domain + MediatR, FluentValidation, AutoMapper

### 4.1 Commands (Write Operations) — 16 Total

| Command                      | Returns                 | Description                                                                     |
| ---------------------------- | ----------------------- | ------------------------------------------------------------------------------- |
| **Property Management**      |                         |                                                                                 |
| `CreatePropertyCommand`      | `Result<Guid>`          | Creates draft property with Money + Address value objects                       |
| `UpdatePropertyCommand`      | `Result`                | Updates all property fields, replaces value objects                             |
| `DeletePropertyCommand`      | `Result`                | Soft-deletes property via domain method                                         |
| `PublishPropertyCommand`     | `Result`                | Validates and transitions Draft → Published                                     |
| `UploadPropertyImageCommand` | `Result<Guid>`          | Saves file via `IFileStorage`, creates image record, auto-main logic            |
| `DeletePropertyImageCommand` | `Result`                | Deletes file + record, promotes next image to main                              |
| `SetMainImageCommand`        | `Result`                | Swaps main image designation                                                    |
| **Inquiry Management**       |                         |                                                                                 |
| `CreateInquiryCommand`       | `CreateInquiryResponse` | Creates inquiry with cryptographic access token                                 |
| `AssignInquiryCommand`       | `Unit`                  | Delegates to `inquiry.AssignToAgent()` domain method                            |
| `AddMessageCommand`          | `Guid`                  | Adds conversation message with auto-status progression                          |
| `UpdateInquiryStatusCommand` | `Unit`                  | Manual status transitions with domain method delegation                         |
| **Deal Management**          |                         |                                                                                 |
| `CreateDealCommand`          | `Guid`                  | Creates deal, calculates commission, marks property Sold, closes linked inquiry |
| `UpdateDealCommand`          | `Unit`                  | Updates buyer info + sale price, recalculates commission (Pending only)         |
| `CompleteDealCommand`        | `Unit`                  | Finalizes deal, ensures property Sold status                                    |
| `CancelDealCommand`          | `Unit`                  | Cancels deal, reverts property to Published, reopens linked inquiry             |
| **Analytics**                |                         |                                                                                 |
| `RecordPropertyViewCommand`  | `Unit`                  | Session-deduplicated view tracking (30-minute window)                           |

#### Noteworthy Handler Logic

**`AddMessageCommandHandler`** implements intelligent auto-status progression:

- When an agent/admin replies to a `New` inquiry → auto-assigns them and transitions to `Assigned`
- When an agent/admin replies to an `Assigned` inquiry → transitions to `InProgress`
- This eliminates manual status clicking — the conversation flow naturally drives the state machine

**`CancelDealCommandHandler`** performs a coordinated multi-entity rollback:

1. Cancels the deal
2. Reverts the property from Sold → Published
3. Clears the property's `ClosedDealId`
4. Reopens the linked inquiry (if any) via `inquiry.Reopen()`

**`RecordPropertyViewCommandHandler`** uses a safe fire-and-forget pattern:

- Checks `IPropertyViewRepository.HasRecentViewAsync()` to avoid duplicate counting within 30 minutes
- Catches and logs all exceptions silently — analytics should never break the user experience

**`CreateInquiryCommandHandler`** generates cryptographically secure access tokens:

- 24 random bytes via `RandomNumberGenerator.Create()`
- Base64-encoded with URL-safe character replacements (`+→-`, `/→_`, `=` stripped)
- This gives visitors a shareable link to track their inquiry without creating an account

### 4.2 Queries (Read Operations) — 12 Total

| Query                          | Returns                                  | Key Features                                                                                                                                                                                          |
| ------------------------------ | ---------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetPropertiesQuery`           | `Result<PaginatedList<PropertyListDto>>` | 8 filter parameters: status, agent, type, price range, bedrooms, city. Pagination. Orders by CreatedAt DESC. Post-processes image URLs.                                                               |
| `SearchPropertiesQuery`        | `Result<PaginatedList<PropertyListDto>>` | Public search — only Published properties. Text search on title + description (case-insensitive). Same filter set.                                                                                    |
| `GetPropertyByIdQuery`         | `Result<PropertyDetailDto>`              | Includes images, agent, addresses. Optional `TrackView` flag to record analytics.                                                                                                                     |
| `GetInquiriesQuery`            | `InquiryListResult`                      | Role-aware: agents see only their own + unassigned new inquiries. Text search on visitor name/email/message/property title. Message count aggregation.                                                |
| `GetInquiryByIdQuery`          | `InquiryDetailDto?`                      | Full inquiry with ordered conversation history, sender names resolved.                                                                                                                                |
| `GetInquiryByTokenQuery`       | `InquiryDetailDto?`                      | Token-based access for visitors (validates 32-char format).                                                                                                                                           |
| `GetAllDealsQuery`             | `DealListResult`                         | Filterable by status, agent, date range, search term (buyer name/property title). Paginated.                                                                                                          |
| `GetDealByIdQuery`             | `DealDetailDto?`                         | Full deal with property images, agent, linked inquiry.                                                                                                                                                |
| `GetDealStatisticsQuery`       | `DealStatisticsDto`                      | Aggregates: total/completed/pending/cancelled deals, revenue, commission, averages. **SQLite-aware** — projects financial columns to memory before aggregation (SQLite cannot SUM/AVG decimal types). |
| `GetMostViewedPropertiesQuery` | `List<PropertyViewStats>`                | Groups views by property, counts, orders by view count DESC. Date-filterable.                                                                                                                         |
| `GetAllContentEntriesQuery`    | `List<ContentEntryDto>`                  | All CMS entries with last-updated-by user name.                                                                                                                                                       |
| `GetContentByKeyQuery`         | `string?`                                | Single content value lookup by key.                                                                                                                                                                   |

### 4.3 Validators — 10 Total

Every command with user input has a corresponding `AbstractValidator<T>`:

| Validator                             | Key Rules                                                                                                                               |
| ------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| `CreatePropertyCommandValidator`      | Title ≤ 200, Description ≤ 5000, Price ≥ 0, Currency exactly 3 chars (ISO 4217), Area > 0, GPS coordinates (-90..90 lat, -180..180 lng) |
| `UpdatePropertyCommandValidator`      | Same as Create + Id not empty                                                                                                           |
| `UploadPropertyImageCommandValidator` | Extensions whitelist (.jpg, .jpeg, .png, .gif, .webp), file size ≤ 10 MB                                                                |
| `CreateInquiryCommandValidator`       | Name ≤ 100, Email validated, Phone ≤ 20, Message 10–2000 chars                                                                          |
| `AssignInquiryCommandValidator`       | Both InquiryId and AgentId required                                                                                                     |
| `AddMessageCommandValidator`          | Message 1–2000 chars, SenderType must be valid enum                                                                                     |
| `CreateDealCommandValidator`          | PropertyId + AgentId required, BuyerName ≤ 200, BuyerEmail validated, SalePrice > 0, CommissionRate 0–100%, Notes ≤ 2000                |
| `UpdateDealCommandValidator`          | Same as Create + Id required                                                                                                            |
| `CreateContentEntryCommand`           | Key: required, ≤ 100 chars, regex `^[a-zA-Z0-9_]+$`                                                                                     |
| `UpdateContentEntryCommand`           | Same key validation + Id required                                                                                                       |

All validators are **auto-discovered** via `AddValidatorsFromAssembly()` and executed automatically by `ValidationBehavior<,>` before any handler runs.

### 4.4 DTOs — 16 Data Transfer Objects

**Property DTOs:**

- `PropertyListDto` — compact grid view (12 fields: id, title, price, currency, type, beds/baths/area, city, status, main image, date)
- `PropertyDetailDto` — full detail with nested `AddressDto`, `AgentDto`, `List<PropertyImageDto>`, view count
- `AddressDto`, `AgentDto`, `PropertyImageDto` — nested sub-DTOs

**Inquiry DTOs:**

- `InquiryDto` — list view with message count and response timestamp
- `InquiryDetailDto` — extends InquiryDto with `List<MessageDto>`
- `MessageDto` — sender type, name, message, timestamp
- `CreateInquiryDto`, `CreateInquiryResponse` (InquiryId + AccessToken)

**Deal DTOs:**

- `DealListDto` — grid view with property/agent/buyer summary and financials
- `DealDetailDto` — full detail including property image URL, agent contact, linked inquiry
- `DealStatisticsDto` — 8 aggregated financial metrics
- `DealListResult` — paginated wrapper with Items, TotalCount, Page, PageSize, computed TotalPages

**Content DTOs:**

- `ContentEntryDto` — key, HTML value, last updated by, timestamps

### 4.5 Cross-Cutting Application Services

**Interfaces defined** (5 — all implemented in Infrastructure):

| Interface                 | Methods                                                                      | Purpose                      |
| ------------------------- | ---------------------------------------------------------------------------- | ---------------------------- |
| `IApplicationDbContext`   | 7 `DbSet<T>` properties + `SaveChangesAsync` + `SaveChanges`                 | Core data access abstraction |
| `ICurrentUserService`     | `UserId`, `UserName`, `Email`, `IsAuthenticated`, `IsInRole()`               | Claims-based current user    |
| `IFileStorage`            | `SaveFileAsync`, `DeleteFileAsync`, `GetFileAsync`, `GetFileUrl`             | File storage abstraction     |
| `IEmailSender`            | `SendEmailAsync` (2 overloads)                                               | Email delivery abstraction   |
| `IPropertyViewRepository` | `AddPropertyViewAsync`, `HasRecentViewAsync`, `GetMostViewedPropertiesAsync` | Analytics query abstraction  |

**Shared Models:**

- `PaginatedList<T>` — generic pagination wrapper with `HasPreviousPage`/`HasNextPage` computed properties
- `Result` / `Result<T>` — explicit success/failure monad with static factory methods

**AutoMapper Profile** (`PropertyMappingProfile`):

- `Property → PropertyListDto` — maps value object fields (`Price.Amount`, `Price.Currency`, `Address.City`), ignores `MainImageUrl` (set manually after query for URL resolution)
- `Property → PropertyDetailDto` — same approach, ignores `ViewCount` (computed separately)
- `Address → AddressDto` — auto-mapped
- `User → AgentDto` — auto-mapped
- `PropertyImage → PropertyImageDto` — ignores `Url` (resolved via `IFileStorage.GetFileUrl`)

---

## 5. Infrastructure Layer — Data Access & Services

> `MyRealEstate.Infrastructure` — implements all interfaces defined in the Application layer

### 5.1 ApplicationDbContext

Extends `IdentityDbContext<User, IdentityRole<Guid>, Guid>` and implements `IApplicationDbContext`.

**7 DbSets:** `Properties`, `PropertyImages`, `Inquiries`, `ConversationMessages`, `Deals`, `PropertyViews`, `ContentEntries`

**OnModelCreating:**

1. Calls `base.OnModelCreating()` (Identity schema)
2. Auto-applies all `IEntityTypeConfiguration<T>` from the assembly
3. Registers **3 global query filters** for soft delete:
    - `Property` → `WHERE NOT IsDeleted`
    - `Inquiry` → `WHERE NOT IsDeleted`
    - `User` → `WHERE NOT IsDeleted`

**SaveChangesAsync override:**

- Intercepts `ChangeTracker.Entries<IAuditable>()` on every save
- `Added` → sets `CreatedAt = DateTime.UtcNow`
- `Modified` → sets `UpdatedAt = DateTime.UtcNow`
- This means audit fields are **set exactly once, in one place** — individual handlers never need to manage timestamps

### 5.2 Entity Configurations — 8 Files

Each entity has a dedicated `IEntityTypeConfiguration<T>` with explicit:

- Property constraints (max lengths, precision, required fields)
- Relationship configurations (cascade/restrict/set-null delete behaviors)
- Database indexes (single-column and composite)
- Value object mappings (owned types for `Money` and `Address`)

#### Index Strategy

| Entity            | Indexes                                                                                    | Purpose                                                                   |
| ----------------- | ------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------- |
| **Property**      | `Status`, `AgentId`, `{Status, IsDeleted}` composite, `Slug` (unique)                      | Filter by status, agent lookup, compound filter optimization, URL routing |
| **PropertyImage** | `PropertyId`, `{PropertyId, IsMain}` composite                                             | Image retrieval, main image lookup                                        |
| **PropertyView**  | `PropertyId`, `ViewedAt`, `{PropertyId, ViewedAt}` composite                               | Analytics queries, date-range filtering                                   |
| **Inquiry**       | `Status`, `AssignedAgentId`, `CreatedAt`, `{AssignedAgentId, Status, CreatedAt}` composite | Dashboard filtering, agent inbox, recent inquiry sorting                  |
| **Deal**          | `AgentId`, `ClosedAt`, `Status`, `{AgentId, ClosedAt}` composite                           | Agent performance, date-range reporting                                   |
| **ContentEntry**  | `Key` (unique)                                                                             | CMS key lookups                                                           |
| **User**          | `Email`, `IsActive`                                                                        | Login lookups, active user filtering                                      |

**Total indexes: 20** across 8 configurations.

#### Relationship Delete Behaviors

| Relationship                 | Behavior   | Reasoning                            |
| ---------------------------- | ---------- | ------------------------------------ |
| Property → Images            | `Cascade`  | Images are owned by property         |
| Property → Views             | `Cascade`  | Analytics tied to property lifecycle |
| Property → Inquiries         | `SetNull`  | Preserve inquiry history             |
| Property → Agent             | `SetNull`  | Agent can be reassigned              |
| Inquiry → Messages           | `Cascade`  | Messages are part of inquiry         |
| Inquiry → AssignedAgent      | `SetNull`  | Agent can be removed                 |
| Deal → Property              | `Restrict` | Cannot delete property with deals    |
| Deal → Agent                 | `Restrict` | Cannot delete agent with deals       |
| Deal → Inquiry               | `SetNull`  | Deal can exist without inquiry       |
| ConversationMessage → Sender | `SetNull`  | Preserve message after user deletion |
| ContentEntry → UpdatedByUser | `SetNull`  | Preserve content after user deletion |

### 5.3 Service Implementations

| Service                   | Implementation           | Key Details                                                                                                                                                       |
| ------------------------- | ------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ICurrentUserService`     | `CurrentUserService`     | Reads claims from `IHttpContextAccessor`, parses `NameIdentifier` to `Guid?`                                                                                      |
| `IEmailSender`            | `FakeEmailSender`        | Logs email details to `ILogger<FakeEmailSender>` — swappable to SMTP/SendGrid                                                                                     |
| `IFileStorage`            | `LocalFileStorage`       | Saves to `wwwroot/uploads/` with `{Guid}_{filename}` naming. Content-type detection for jpg/png/gif/webp/pdf. URL generation with scheme+host awareness.          |
| `IPropertyViewRepository` | `PropertyViewRepository` | Implements session-based deduplication via `HasRecentViewAsync`. `GetMostViewedPropertiesAsync` groups by `{PropertyId, Title, City}`, counts views, orders DESC. |

### 5.4 Database Seeder — Realistic Sample Data

The seeder (`DatabaseSeeder.cs`, 938 lines) creates a fully interconnected dataset:

| Data                      | Count | Details                                                                                              |
| ------------------------- | ----- | ---------------------------------------------------------------------------------------------------- |
| **Roles**                 | 2     | Admin, Agent                                                                                         |
| **Users**                 | 3     | 1 admin + 2 agents (Ahmed Ben Ali, Fatma Mansour)                                                    |
| **Content Entries**       | 4     | HomeHero, AboutHtml, FooterText, ContactInfo                                                         |
| **Properties**            | 8     | Range: 180K–1.2M TND, types: Villa/Apartment/House/Studio/Duplex                                     |
| **Property Images**       | 14    | 2 per property (most), with main image designation                                                   |
| **Inquiries**             | 6     | All 5 statuses covered (New, Assigned, InProgress, Answered, Closed) + general inquiry (no property) |
| **Conversation Messages** | ~8    | Agent replies, visitor follow-ups, internal notes                                                    |
| **Property Views**        | ~171  | Distributed over 28 days, 5 user agents, varied IPs                                                  |
| **Deals**                 | 5     | 2 Completed, 2 Pending, 1 Cancelled — with commission calculations                                   |

**Seed interconnections:** Deals reference properties and inquiries. `Inquiry.Close(dealId)` is called. `Property.MarkAsSold(dealId)` is called. The cancelled deal's property reverts to Draft. PropertyViews generate a realistic "Most Visited" ranking with the Penthouse Apartment at 45 views.

### 5.5 Dependency Injection Registration

All services registered in `Infrastructure.DependencyInjection.AddInfrastructure()`:

```
AddDbContext<ApplicationDbContext>          → SQLite ("DefaultConnection")
IApplicationDbContext                       → ApplicationDbContext (Scoped)
AddIdentity<User, IdentityRole<Guid>>       → EF stores, token providers
IFileStorage                                → LocalFileStorage (Scoped)
IEmailSender                                → FakeEmailSender (Scoped)
ICurrentUserService                         → CurrentUserService (Scoped)
IPropertyViewRepository                     → PropertyViewRepository (Scoped)
IHttpContextAccessor                        → (Singleton, framework)
```

**Identity configuration:**

- Passwords: 8+ characters, requires uppercase, lowercase, digit, special character
- Lockout: 5 failed attempts → 5-minute lockout
- Unique email enforced
- Cookie auth: 24-hour expiry, sliding expiration, paths for login/logout/access-denied

### 5.6 Migrations

| Migration                 | Date         | Description                                                                                                             |
| ------------------------- | ------------ | ----------------------------------------------------------------------------------------------------------------------- |
| `InitialCreate`           | Jan 16, 2026 | Full schema: Properties, Images, Users, Inquiries, ConversationMessages, PropertyViews, ContentEntries, Identity tables |
| `AddAccessTokenToInquiry` | Jan 19, 2026 | Adds `AccessToken` column to Inquiries for visitor tracking                                                             |
| `AddDealExtendedFields`   | Feb 7, 2026  | Adds Deal entity with InquiryId, BuyerEmail, BuyerPhone, Notes, ClosedAt columns                                        |

---

## 6. Presentation Layer — MVC & Views

> `MyRealEstate.Web` — ASP.NET Core 8.0 MVC with Areas pattern, Razor views, Bootstrap 5

### 6.1 Application Startup (Program.cs)

**Serilog** configured with console + rolling file sinks (`logs/log-{date}.txt`).

**Service registration order:**

1. `AddApplication()` — MediatR + FluentValidation + AutoMapper
2. `AddInfrastructure(config)` — EF Core + Identity + all service implementations
3. `AddControllersWithViews()` + `AddRazorPages()`
4. `AddDistributedMemoryCache()` + `AddSession()` (2h idle timeout, HttpOnly)

**Middleware pipeline (order matters):**

1. `DatabaseSeeder.SeedAsync()` — runs before any HTTP handling
2. `ExceptionHandlingMiddleware` — global error catching
3. `UseHsts()` (production only)
4. `UseHttpsRedirection()`
5. `UseStaticFiles()`
6. `UseSerilogRequestLogging()` — structured HTTP request logs
7. `UseRouting()`
8. `UseSession()`
9. `UseAuthentication()` → `UseAuthorization()`
10. Area route: `{area:exists}/{controller=Dashboard}/{action=Index}/{id?}`
11. Default route: `{controller=Home}/{action=Index}/{id?}`

### 6.2 Controllers — 9 Total

#### Admin Area Controllers (5)

| Controller             | Auth          | Actions    | Key Responsibility                                                                                                                              |
| ---------------------- | ------------- | ---------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `DashboardController`  | Admin, Agent  | 1 action   | Aggregates 15+ metrics, computes deal financials client-side (SQLite workaround), shows top 5 most viewed properties, recent items              |
| `PropertiesController` | Authenticated | 11 actions | Full CRUD + image management (upload/delete/set-main) + publish workflow                                                                        |
| `InquiriesController`  | Admin, Agent  | 6 actions  | Inquiry lifecycle: list (role-filtered), detail (authorization-checked), assign (Admin only), reply, close, status update                       |
| `DealsController`      | Admin, Agent  | 9 actions  | Deal lifecycle: list, detail, create (with inquiry pre-fill), edit (Pending only), complete, cancel + AJAX endpoint for property inquiry lookup |
| `ContentController`    | Admin only    | 6 actions  | CMS CRUD with TinyMCE rich text editor + API key diagnostic endpoint                                                                            |

**Total admin actions: 33**

#### Public Controllers (4)

| Controller             | Actions   | Key Responsibility                                                                                         |
| ---------------------- | --------- | ---------------------------------------------------------------------------------------------------------- |
| `HomeController`       | 4 actions | Landing page, privacy, error handling (reads from `HttpContext.Items`), 404 page                           |
| `AccountController`    | 4 actions | Login (with lockout, LastLoginAt tracking), logout, access denied                                          |
| `PropertiesController` | 2 actions | Public property browse (Published only, 12/page, multi-filter) + detail with fire-and-forget view tracking |
| `InquiriesController`  | 5 actions | Submit inquiry, track by token, add visitor message, mark answered, close                                  |

**Total public actions: 15**

### 6.3 ViewModels — 30+ View Models

Organized across 5 files with complete Data Annotations validation:

| File                    | View Models                                                                                                                                                                                       | Purpose                   |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------- |
| `PropertyViewModels.cs` | `PropertyListViewModel`, `PropertyDetailViewModel`, `PropertyCreateViewModel`, `PropertyEditViewModel`, `PropertyImageViewModel`, `AddressViewModel`, `AgentViewModel`, `PropertySearchViewModel` | Admin property management |
| `InquiryViewModels.cs`  | `InquiryListViewModel`, `InquiryDetailViewModel`, `MessageViewModel`, `InquiryCreateViewModel`, `ReplyToInquiryViewModel`, `AssignInquiryViewModel`, `InquirySearchViewModel`                     | Admin inquiry management  |
| `DealViewModels.cs`     | `DealListViewModel`, `DealDetailViewModel`, `DealCreateViewModel`, `DealEditViewModel`, `DealSearchViewModel`                                                                                     | Admin deal management     |
| `PublicViewModels.cs`   | `PublicPropertyListViewModel`, `PropertySearchFilters`, `PublicPropertyDetailViewModel`, `CreateInquiryViewModel`, `InquiryTrackingViewModel`, `AddMessageViewModel`, `InquiryCreatedViewModel`   | Public visitor experience |
| `ErrorViewModel.cs`     | `ErrorViewModel`                                                                                                                                                                                  | Error page                |

### 6.4 Views — 33 Total

| Area                  | Views                                                            | Files |
| --------------------- | ---------------------------------------------------------------- | ----- |
| **Admin Dashboard**   | Dashboard overview                                               | 1     |
| **Admin Properties**  | Index, Create, Edit, Details, Delete                             | 5     |
| **Admin Inquiries**   | Index, Details                                                   | 2     |
| **Admin Deals**       | Index, Create, Edit, Details                                     | 4     |
| **Admin Content**     | Index, Create, Edit                                              | 3     |
| **Public Home**       | Index, NotFound, Privacy                                         | 3     |
| **Public Account**    | Login, AccessDenied                                              | 2     |
| **Public Properties** | Index (browse), Details                                          | 2     |
| **Public Inquiries**  | Created (success), InvalidToken, Track                           | 3     |
| **Shared**            | \_Layout, \_Layout.cshtml.css, Error, \_ValidationScriptsPartial | 4     |
| **Config**            | \_ViewImports (×2), \_ViewStart (×2)                             | 4     |

### 6.5 Middleware

**`ExceptionHandlingMiddleware`** — global exception handler with intelligent response formatting:

| Exception Type        | HTTP Status               | Behavior                                |
| --------------------- | ------------------------- | --------------------------------------- |
| `ValidationException` | 400 Bad Request           | Returns validation errors dictionary    |
| `NotFoundException`   | 404 Not Found             | Redirects to custom 404 page            |
| Any other             | 500 Internal Server Error | Dev: shows message; Prod: generic error |

**Response format detection:**

- **AJAX requests** (detected via `X-Requested-With: XMLHttpRequest` or `Accept: application/json`) → returns JSON: `{ success, message, errors, traceId }`
- **Browser requests** → stores error details in `HttpContext.Items`, redirects to error page

This ensures that both traditional page navigation and JavaScript AJAX calls get properly formatted error responses.

### 6.6 Layout & Navigation

The shared `_Layout.cshtml` provides:

**Public navigation:** Home, Browse Properties (with Bootstrap Icon)

**Authenticated navigation (role-based):**

- **Admin:** Dashboard, Manage Properties, Manage Inquiries, Manage Deals, Content
- **Agent:** Manage Deals (limited visibility)

**User dropdown:** Displays username with logout form (anti-forgery token on logout POST)

**Frontend stack:** Bootstrap 5 CSS + JS bundle, Bootstrap Icons (CDN v1.11.3), jQuery, jQuery Validation + Unobtrusive, custom `site.css` with brand variables

---

## 7. Feature Breakdown

### 7.1 Property Management

**Full CRUD lifecycle** with a status-driven workflow:

```
Draft ──[Publish]──→ Published ──[CreateDeal]──→ Sold
                         │                         │
                         │                    [CancelDeal]
                         │                         │
                         ◄─────────────────────────┘
                    (reverts to Published)
```

**Image management:**

- Multiple images per property (uploaded via `IFileStorage`)
- Main image designation (exactly one per property)
- Auto-promotion: if the main image is deleted, the next image automatically becomes main
- File validation: whitelist extensions (.jpg, .jpeg, .png, .gif, .webp), max 10 MB
- Image URLs resolved through `IFileStorage.GetFileUrl()` for portability

**Admin features:** Paginated listing with status filter, inline publish action, deletion confirmation page, image gallery management in edit view

**Public features:** Browse Published properties only with 6 filter parameters (city, type, price range, bedrooms). 12 items per page with pagination. Detail page with fire-and-forget view tracking.

### 7.2 Inquiry Management

**Token-based visitor tracking** — visitors receive a unique 32-character cryptographic token when submitting an inquiry. This token allows them to:

- Track inquiry status without creating an account
- View the conversation history
- Reply to agent messages
- Mark the inquiry as answered or closed

**Agent workflow:**

1. New inquiries appear in the dashboard and inbox
2. Admin assigns an inquiry to an agent (or the agent auto-assigns by replying)
3. Agent replies — conversation is threaded with timestamps and sender identification
4. Agent can add **internal notes** (visible only to agents/admin, not visitors)
5. Inquiry progresses through statuses automatically based on actions

**Role-based visibility:**

- **Admin** sees all inquiries
- **Agent** sees only their assigned inquiries + unassigned new inquiries (potential leads)

### 7.3 Deal Management

**Complete sales tracking** from negotiation to closure:

**Create deal** → auto-fills buyer info from linked inquiry if available. Calculates commission. Marks property as Sold. Closes linked inquiry.

**Edit deal** → only Pending deals can be modified (buyer info, sale price, commission rate). Commission auto-recalculated.

**Complete deal** → finalizes with optional notes. Sets `ClosedAt` timestamp. Ensures property stays Sold.

**Cancel deal** → multi-entity rollback: cancels deal, reverts property to Published, reopens linked inquiry. Appends cancellation reason to notes for audit trail.

**Dashboard statistics:**

- Total/Completed/Pending/Cancelled deal counts
- Total revenue and commission across completed deals
- Average sale price and commission
- Recent deals (top 5)
- Deals closed this month

### 7.4 Content Management System (CMS)

**Key-value HTML content store** — Admin users can create and edit dynamic content blocks:

| Key Pattern   | Usage                     |
| ------------- | ------------------------- |
| `HomeHero`    | Landing page hero section |
| `AboutHtml`   | About page content        |
| `FooterText`  | Footer content            |
| `ContactInfo` | Contact details           |

**TinyMCE integration** — rich text editor with API key stored in User Secrets (not committed to source control). Includes a diagnostic endpoint (`/Admin/Content/TestApiKey`) to verify API key configuration.

**Audit trail** — every content entry records who last updated it and when.

### 7.5 Analytics — Property View Tracking

**Session-based deduplication:**

- When a visitor views a property, the system checks if the same session has viewed it in the last 30 minutes
- Only unique views within the deduplication window are counted
- Tracking data: SessionId, IP address, User Agent, timestamp

**Dashboard widget — "Most Visited Properties (Last 30 Days)":**

- Groups views by property
- Joins property title and city
- Orders by view count descending
- Displays top 5 with view counts and last viewed timestamp

**Indexed for performance:** Composite index on `{PropertyId, ViewedAt}` for efficient date-range queries.

### 7.6 Admin Dashboard

The dashboard aggregates data from **all feature areas** into a single overview:

| Section             | Metrics                                       |
| ------------------- | --------------------------------------------- |
| **Properties**      | Total, Published, Draft, Sold                 |
| **Inquiries**       | Total, New (unread), Open (active)            |
| **Deals**           | Total, Pending, Completed, Closed This Month  |
| **Financial**       | Total Revenue, Total Commission               |
| **Most Viewed**     | Top 5 properties by view count (last 30 days) |
| **Recent Activity** | Recent 5 deals, inquiries, properties         |

**SQLite compatibility:** Financial aggregation uses a client-side projection workaround because SQLite cannot SUM/AVG decimal types. Only the financial columns (SalePrice, CommissionAmount) are loaded to memory — the filtering and counting still happen at the database level.

---

## 8. Security & Authorization

### 8.1 Authentication

- **ASP.NET Core Identity** with `User` extending `IdentityUser<Guid>`
- Cookie authentication with 24-hour sliding expiration
- Password policy: 8+ characters, requires uppercase, lowercase, digit, special character
- Account lockout: 5 failed attempts → 5-minute lockout
- `LastLoginAt` updated on every successful login

### 8.2 Authorization Strategy

| Area               | Authorization   | Details                                                                                                  |
| ------------------ | --------------- | -------------------------------------------------------------------------------------------------------- |
| Public pages       | None            | Anyone can browse properties, submit inquiries                                                           |
| Login/Logout       | Mixed           | Login is public; Logout requires authentication                                                          |
| Dashboard          | `Admin, Agent`  | Both roles can access                                                                                    |
| Properties (Admin) | `Authenticated` | Any logged-in user                                                                                       |
| Inquiries (Admin)  | `Admin, Agent`  | Role-based data filtering at query level                                                                 |
| Inquiry Assignment | `Admin` only    | Enforced in controller                                                                                   |
| Inquiry Detail     | `Admin, Agent`  | Agents can only view their own or unassigned new inquiries (403 Forbid returned for unauthorized access) |
| Deals (Admin)      | `Admin, Agent`  | Both roles                                                                                               |
| Content (CMS)      | `Admin` only    | Strictly admin-gated                                                                                     |

### 8.3 CSRF Protection

Every state-changing action uses `[ValidateAntiForgeryToken]`:

- All POST form submissions
- Logout form (even though it's a simple action)
- No state-changing GET endpoints

### 8.4 Sensitive Data Management

- TinyMCE API key stored in **User Secrets** (development) — never in `appsettings.json`
- Connection strings isolated in configuration
- No credentials in source code (seed passwords are for development only)

### 8.5 Visitor Security

- Access tokens generated via `RandomNumberGenerator` (CSPRNG)
- Token format: 32 characters, URL-safe Base64
- Token length validated before database lookup (prevents SQL injection via length check)
- No account creation required — privacy-preserving inquiry tracking

---

## 9. Data Model & Persistence

### 9.1 Database: SQLite (Development)

Connection string: `Data Source=MyRealEstate.db`

**Production-ready design:** The application uses `IApplicationDbContext` abstraction. Switching to SQL Server or PostgreSQL requires only changing the connection string and the `UseSqlite()` call in `DependencyInjection.cs`.

**Known SQLite limitation handled:** The `GetDealStatisticsQueryHandler` projects only financial columns to memory before aggregation, because SQLite's decimal support doesn't allow SUM/AVG at the database level.

### 9.2 Entity Relationship Diagram

```
┌─────────────┐     1:N     ┌──────────────┐    1:N    ┌──────────────────┐
│    User      │◄───────────│   Property    │─────────►│  PropertyImage   │
│  (Agent)     │  AgentId   │              │          │                  │
└──────┬───────┘            └──────┬───────┘          └──────────────────┘
       │                          │
       │ AssignedAgentId          │ PropertyId         PropertyId
       │                          │                        │
       ▼                          ▼                        ▼
┌──────────────┐    1:N    ┌──────────────┐         ┌──────────────────┐
│   Inquiry     │◄─────────│  Property    │         │  PropertyView    │
│              │           └──────────────┘         │  (Analytics)     │
└──────┬───────┘                                    └──────────────────┘
       │ InquiryId
       │                   ┌──────────────┐
       │     1:N           │    Deal       │
       ├──────────────────►│              │
       │                   │  PropertyId  │
       │ InquiryId         │  AgentId     │
       ▼                   │  InquiryId   │
┌──────────────────┐       │  SalePrice   │
│ ConversationMsg  │       │  Commission  │
│  SenderType      │       └──────────────┘
│  Body            │
│  IsInternalNote  │       ┌──────────────┐
└──────────────────┘       │ ContentEntry │
                           │  Key (unique)│
                           │  HtmlValue   │
                           └──────────────┘
```

### 9.3 Global Query Filters

Three entities have soft-delete query filters applied globally:

```csharp
builder.Entity<Property>().HasQueryFilter(p => !p.IsDeleted);
builder.Entity<Inquiry>().HasQueryFilter(i => !i.IsDeleted);
builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
```

This means **every query** against these entities automatically excludes deleted records — even joins and navigation properties. The application code never needs `WHERE IsDeleted = false` — it's handled by EF Core's infrastructure.

### 9.4 Value Object Persistence (Owned Types)

**Money** → stored as 2 columns:

- `Price` (decimal, precision 18,2)
- `Currency` (string, max 3 chars)

**Address** → stored as 8 columns:

- `AddressLine1` (200, required), `AddressLine2` (200)
- `City` (100, required), `State` (100)
- `PostalCode` (20), `Country` (100, required)
- `Latitude` (precision 10,7), `Longitude` (precision 10,7)

Both include private parameterless constructors for EF Core materialization while maintaining immutability for application code.

---

## 10. Cross-Cutting Concerns

### 10.1 Logging

**Serilog** with structured logging throughout:

| Level         | Usage                                                  |
| ------------- | ------------------------------------------------------ |
| `Information` | Request handling, successful operations, DB commands   |
| `Warning`     | EF Core query filter warnings, non-critical issues     |
| `Error`       | Command handler failures, middleware-caught exceptions |
| `Fatal`       | Application startup failures                           |

**Sinks:** Console (development) + Rolling file (`logs/log-{date}.txt`)

**Request logging:** `UseSerilogRequestLogging()` captures HTTP method, path, status code, and response time for every request.

**MediatR logging:** `LoggingBehavior<,>` automatically logs every command/query name at entry and exit, with error-level logging on exceptions.

### 10.2 Error Handling Strategy

Three layers of error handling work together:

1. **Domain level:** Entities throw `InvalidOperationException` for invalid state transitions (e.g., publishing a property without a price, completing an already-completed deal)

2. **Application level:** Handlers throw `NotFoundException` for missing entities, `ValidationException` for input errors (via pipeline behavior)

3. **Presentation level:** `ExceptionHandlingMiddleware` catches everything:
    - Maps exception types to HTTP status codes
    - Returns JSON for AJAX, redirects for browser requests
    - Includes `TraceId` for debugging
    - Hides internal details in production

Controllers additionally use try/catch with `TempData["Error"]` for user-friendly feedback.

### 10.3 Audit Trail

**Automatic timestamps:** Every entity implementing `IAuditable` gets `CreatedAt` and `UpdatedAt` automatically set by the `SaveChangesAsync` override — no handler code needed.

**Content tracking:** ContentEntry records `UpdatedByUserId` with a navigation property to resolve the editor's name.

**Login tracking:** `LastLoginAt` updated on every successful sign-in.

**Deal history:** Notes are appended (not replaced) on completion and cancellation, creating a chronological audit trail.

---

## 11. Technology Stack

### Backend

| Technology            | Version | Purpose                             |
| --------------------- | ------- | ----------------------------------- |
| ASP.NET Core          | 8.0     | Web framework                       |
| Entity Framework Core | 8.0     | ORM (Code First)                    |
| ASP.NET Core Identity | 8.0     | Authentication & authorization      |
| MediatR               | Latest  | CQRS mediator                       |
| FluentValidation      | Latest  | Input validation pipeline           |
| AutoMapper            | Latest  | Object mapping                      |
| Serilog               | Latest  | Structured logging (console + file) |
| SQLite                | —       | Development database                |

### Frontend

| Technology                    | Version | Purpose                    |
| ----------------------------- | ------- | -------------------------- |
| Bootstrap                     | 5.x     | CSS framework              |
| Bootstrap Icons               | 1.11.3  | Icon library (CDN)         |
| jQuery                        | 3.x     | DOM manipulation, AJAX     |
| jQuery Validation             | —       | Client-side validation     |
| jQuery Validation Unobtrusive | —       | MVC validation integration |
| TinyMCE                       | 6/8     | Rich text editor (CMS)     |
| Razor                         | —       | Server-side view engine    |

### Development Tools

| Tool               | Purpose                            |
| ------------------ | ---------------------------------- |
| .NET CLI           | Build, run, migrations             |
| User Secrets       | Sensitive config (TinyMCE API key) |
| EF Core Migrations | Schema versioning (3 migrations)   |

---

## 12. Project Metrics

### Codebase Size

| Metric                     | Count                                                                                              |
| -------------------------- | -------------------------------------------------------------------------------------------------- |
| **Solution projects**      | 4                                                                                                  |
| **Domain entities**        | 8 (Property, PropertyImage, PropertyView, Inquiry, ConversationMessage, Deal, ContentEntry, User)  |
| **Value objects**          | 2 (Money, Address)                                                                                 |
| **Enums**                  | 4 (PropertyStatus, InquiryStatus, DealStatus, SenderType)                                          |
| **Domain methods**         | 16 across entities                                                                                 |
| **Application interfaces** | 5                                                                                                  |
| **Commands**               | 16                                                                                                 |
| **Queries**                | 12                                                                                                 |
| **Command/Query handlers** | 22+                                                                                                |
| **Validators**             | 10                                                                                                 |
| **DTOs**                   | 16+                                                                                                |
| **Pipeline behaviors**     | 2 (Validation, Logging)                                                                            |
| **Custom exceptions**      | 2 (NotFoundException, ValidationException)                                                         |
| **EF configurations**      | 8                                                                                                  |
| **Database indexes**       | 20                                                                                                 |
| **Controllers**            | 9 (5 admin, 4 public)                                                                              |
| **Controller actions**     | 48 (33 admin, 15 public)                                                                           |
| **View models**            | 30+                                                                                                |
| **Razor views**            | 33                                                                                                 |
| **Middleware**             | 1 (ExceptionHandling)                                                                              |
| **Services**               | 4 (CurrentUser, FileStorage, EmailSender, PropertyViewRepository)                                  |
| **Database migrations**    | 3                                                                                                  |
| **Seed data volume**       | 8 properties, 14 images, 6 inquiries, ~8 messages, ~171 views, 5 deals, 4 content entries, 3 users |

### Architecture Quality Indicators

| Indicator                                      | Status                                                   |
| ---------------------------------------------- | -------------------------------------------------------- |
| Domain layer external dependencies             | **0** (pure C#, no framework references beyond Identity) |
| Controllers referencing Infrastructure types   | **0** (all go through Application interfaces)            |
| Raw SQL in application code                    | **0** (all LINQ via EF Core)                             |
| Business logic in controllers                  | **0** (delegated to MediatR handlers)                    |
| Handlers without validation                    | **0** (all covered by ValidationBehavior pipeline)       |
| Entities with public setters for status fields | **0** (state transitions via domain methods)             |
| Forms without anti-forgery tokens              | **0**                                                    |
| State-changing GET endpoints                   | **0**                                                    |
| Hard-coded connection strings                  | **0** (all in configuration)                             |
| Secrets in source control                      | **0** (User Secrets for sensitive config)                |

---

## 13. Running the Application

### Prerequisites

- .NET 8.0 SDK
- TinyMCE API Key (free from [tiny.cloud](https://tiny.cloud))

### Quick Start

```bash
# 1. Clone and restore
dotnet restore

# 2. Set TinyMCE API Key (optional — CMS feature only)
cd src/MyRealEstate.Web
dotnet user-secrets set "TinyMCE:ApiKey" "your-api-key-here"

# 3. Run (database auto-created and seeded on first run)
dotnet run --project src/MyRealEstate.Web
```

The application will:

1. Apply all 3 EF Core migrations automatically
2. Seed roles, users, properties, images, inquiries, conversations, views, and deals
3. Start on `http://localhost:5088`

### Default Credentials

| Role  | Email                 | Password     |
| ----- | --------------------- | ------------ |
| Admin | admin@estateflow.com  | Admin@123456 |
| Agent | agent1@estateflow.com | Agent@123456 |
| Agent | agent2@estateflow.com | Agent@123456 |

### Key URLs

| URL                 | Description                     |
| ------------------- | ------------------------------- |
| `/`                 | Public landing page             |
| `/Properties`       | Public property browser         |
| `/Account/Login`    | Login page                      |
| `/Admin`            | Admin dashboard (requires auth) |
| `/Admin/Properties` | Property management             |
| `/Admin/Inquiries`  | Inquiry management              |
| `/Admin/Deals`      | Deal management                 |
| `/Admin/Content`    | CMS management (Admin only)     |

---

## 14. Branding & Design System

**Name:** EstateFlow
**Tagline:** Streamlined Real Estate Management
**Logo:** `wwwroot/logo/Logo.png` (40px height in navbar)

### Color Palette

| Token                | Hex       | Usage                                 |
| -------------------- | --------- | ------------------------------------- |
| `--estate-primary`   | `#2563EB` | Primary buttons, links, active states |
| `--estate-secondary` | `#10B981` | Success states, positive metrics      |
| `--estate-accent`    | `#F59E0B` | Warnings, attention items             |
| `--estate-dark`      | `#1E293B` | Dark backgrounds, text                |
| `--estate-light`     | `#F8FAFC` | Light backgrounds                     |

### UI Framework

- **Bootstrap 5** — grid system, components, utilities
- **Bootstrap Icons** — consistent iconography via CDN
- **Custom CSS overrides** — `.btn-primary`, `.bg-primary`, `.badge.bg-primary`, `.border-primary` all mapped to EstateFlow brand colors via CSS custom properties
- **Focus ring styling** — custom focus indicators using brand primary color

### View Conventions

- Breadcrumb navigation on all admin pages
- Confirmation modals for destructive actions (delete)
- TempData flash messages: `TempData["Success"]` (green), `TempData["Error"]` (red)
- Bootstrap badge styling for statuses (color-coded per enum value)
- Responsive tables with pagination controls
- Form validation: server-side (FluentValidation) + client-side (jQuery Validation Unobtrusive)

---

## Appendix: Project File Structure

```
MyRealEstate/
├── MyRealEstate.sln
├── CONTEXT.md                          ← This file
│
├── src/
│   ├── MyRealEstate.Domain/           ← 0 dependencies
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── Property.cs
│   │   │   ├── PropertyImage.cs
│   │   │   ├── PropertyView.cs
│   │   │   ├── Inquiry.cs
│   │   │   ├── ConversationMessage.cs
│   │   │   ├── Deal.cs
│   │   │   ├── ContentEntry.cs
│   │   │   └── User.cs
│   │   ├── Enums/
│   │   │   ├── PropertyStatus.cs
│   │   │   ├── InquiryStatus.cs
│   │   │   ├── DealStatus.cs
│   │   │   └── SenderType.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuditable.cs
│   │   │   └── ISoftDelete.cs
│   │   └── ValueObjects/
│   │       ├── Address.cs
│   │       └── Money.cs
│   │
│   ├── MyRealEstate.Application/       ← Depends on Domain
│   │   ├── DependencyInjection.cs
│   │   ├── Commands/
│   │   │   ├── Analytics/
│   │   │   │   └── RecordPropertyViewCommand.cs
│   │   │   ├── Content/
│   │   │   │   ├── CreateContentEntryCommand.cs
│   │   │   │   ├── UpdateContentEntryCommand.cs
│   │   │   │   └── DeleteContentEntryCommand.cs
│   │   │   ├── Deals/
│   │   │   │   ├── CreateDealCommand.cs
│   │   │   │   ├── UpdateDealCommand.cs
│   │   │   │   ├── CompleteDealCommand.cs
│   │   │   │   └── CancelDealCommand.cs
│   │   │   ├── Inquiries/
│   │   │   │   ├── CreateInquiryCommand.cs
│   │   │   │   ├── AssignInquiryCommand.cs
│   │   │   │   ├── AddMessageCommand.cs
│   │   │   │   └── UpdateInquiryStatusCommand.cs
│   │   │   └── Properties/
│   │   │       ├── CreatePropertyCommand.cs
│   │   │       ├── UpdatePropertyCommand.cs
│   │   │       ├── DeletePropertyCommand.cs
│   │   │       ├── PublishPropertyCommand.cs
│   │   │       ├── UploadPropertyImageCommand.cs
│   │   │       ├── DeletePropertyImageCommand.cs
│   │   │       └── SetMainImageCommand.cs
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── LoggingBehavior.cs
│   │   │   ├── Exceptions/
│   │   │   │   ├── NotFoundException.cs
│   │   │   │   └── ValidationException.cs
│   │   │   └── Models/
│   │   │       ├── PaginatedList.cs
│   │   │       └── Result.cs
│   │   ├── DTOs/
│   │   │   ├── PropertyDtos.cs
│   │   │   ├── DealDtos.cs
│   │   │   └── InquiryDto.cs
│   │   ├── Interfaces/
│   │   │   ├── IApplicationDbContext.cs
│   │   │   ├── ICurrentUserService.cs
│   │   │   ├── IEmailSender.cs
│   │   │   ├── IFileStorage.cs
│   │   │   └── IPropertyViewRepository.cs
│   │   ├── Mappings/
│   │   │   └── PropertyMappingProfile.cs
│   │   ├── Queries/
│   │   │   ├── Analytics/
│   │   │   ├── Content/
│   │   │   ├── Deals/
│   │   │   ├── Inquiries/
│   │   │   └── Properties/
│   │   └── Validators/
│   │       ├── InquiryValidators.cs
│   │       └── DealValidators.cs
│   │
│   ├── MyRealEstate.Infrastructure/    ← Depends on Application + Domain
│   │   ├── DependencyInjection.cs
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/         ← 8 entity config files
│   │   │   ├── Migrations/             ← 3 migrations + snapshot
│   │   │   └── Seed/
│   │   │       └── DatabaseSeeder.cs   ← 938 lines
│   │   ├── Repositories/
│   │   │   └── PropertyViewRepository.cs
│   │   └── Services/
│   │       ├── CurrentUserService.cs
│   │       ├── FakeEmailSender.cs
│   │       └── LocalFileStorage.cs
│   │
│   └── MyRealEstate.Web/              ← Depends on Application
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Areas/
│       │   └── Admin/
│       │       ├── Controllers/        ← 5 controllers
│       │       └── Views/              ← 17 views
│       ├── Controllers/                ← 4 controllers
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Models/                     ← 5 view model files (30+ classes)
│       ├── Views/                      ← 16 views
│       └── wwwroot/
│           ├── css/site.css
│           ├── js/site.js
│           ├── logo/Logo.png
│           ├── images/properties/      ← 14 seed images
│           └── lib/                    ← Bootstrap, jQuery
```

---

> **Last Updated:** February 9, 2026
> **Status:** Active Development
> **Build:** 0 Errors, 1 Warning (pre-existing CS8605 in HomeController)
