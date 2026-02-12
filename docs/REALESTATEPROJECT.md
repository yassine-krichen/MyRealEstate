# Project — MyRealEstate (Admin Portal)

A professional-grade, production-ready **Administration Portal for a Real Estate website** built **only with ASP.NET Core (Razor Views)**. Clean Architecture, strong separation of concerns, enterprise practices, CI/CD, test coverage, deployment, and the ability to be sold to real estate companies.

---

# 1 — High-level summary / elevator pitch

MyRealEstate is an all-in-one admin platform for real estate businesses to manage listings, agents, leads, content, and site analytics. Built with modern ASP.NET Core best practices (Clean Architecture, MediatR, EF Core, Razor), it’s secure, extensible, and ready for production deployment in Docker. Designed to be a showcase project and a sellable product.

Success metrics:

* Production-quality codebase with tests and CI.
* Clean Architecture and Razor-only UI that an interviewer/technical reviewer will praise.
* Demoable admin dashboard with charts, RBAC, file handling, search, background processing.
* Easily customizable and white-label-able for customers.

---

# 2 — Target users & personas

* **Admin**: full control — manage users, listings, site content and analytics.
* **Agent / Broker**: manage own listings, respond to leads, view performance.
* **Moderator**: moderate listings and messages, verify documents.
* **Visitor (public)**: not part of the admin portal, but the admin portal supports features used by public site (leads, messages).
* **Customer (Business)**: real estate companies evaluating the product for purchase.

---

# 3 — Goals & non-goals

**Goals**

* Showcase advanced ASP.NET skills and production-ready practices.
* Build a Razor-only UI that is clean and responsive.
* Provide secure, auditable workflows, and multi-role access.
* Demonstrate integration points: emails, file storage, search, background jobs, metrics.

**Non-goals**

* Building a separate SPA frontend — all UI is Razor.
* Creating complex CRM workflows beyond leads/messages and user management in MVP.

---

# 4 — Feature list (exhaustive)

## MVP (must-have)

1. Authentication & Authorization

   * ASP.NET Identity (cookie based), roles: Admin, Agent, Moderator.
   * Policies for actions (e.g., can-approve-listing).
2. Property management (CRUD)

   * Create/Edit/Delete/List properties with server-side validation.
   * Fields: Title, Description, Price, Currency, Type (Sale/Rent), Status (Draft/Published/Sold), Address (structured), City, PostalCode, Lat/Lng, Bedrooms, Bathrooms, Area (sq m), Features (list), AgentId, ListedAt, PublishedAt.
3. Media: image upload + gallery per property

   * File validation, image processing (resize), store via IFileStorage abstraction.
4. Dashboard

   * KPIs: total active listings, leads last 30 days, new users, top agents.
   * Charts: listings by status, leads by source.
5. Search & filtering

   * Filters: price range, city, type, bedrooms, area.
   * Paginated results & sorting.
6. Leads & messaging

   * Store inquiries, link to property and agent, agent notifications (email/dashboard).
7. Role-based UI (menu and pages appear based on roles).
8. Logging & error handling (Serilog).
9. Unit tests for domain and application logic + at least 1 integration test for create-listing.
10. README + run instructions + migration scripts.

## v1 (very strong show)

1. Background worker for image processing and scheduled reports (IHostedService / Hangfire optional).
2. File storage implementations: LocalDisk + Azure Blob (switchable).
3. ViewComponents for reusable UI (ListingCard, AgentBadge, FiltersComponent).
4. MediatR for commands/queries (CQRS-lite).
5. FluentValidation for request validation.
6. AutoMapper profiles (or manual mapping in Application layer).
7. Docker + Docker Compose + database (Postgres or SQL Server).
8. Swagger/OpenAPI for any APIs.
9. Integration tests with Testcontainers (DB in container).
10. CI (GitHub Actions) running tests and building images.

## Advanced (wow-factor)

1. Full-text search (Elasticsearch or PostgreSQL full text).
2. Geospatial search (radius search by lat/lng).
3. SignalR for real-time notifications (new leads).
4. Audit logs + activity feed.
5. Multi-tenant support (tenant per company).
6. Analytics export to NoSQL (for AI later).
7. Billing/subscription engine (Stripe integration) for SaaS sales.
8. OpenTelemetry tracing + metrics dashboard (Prometheus/Grafana).
9. Accessibility (WCAG) and internationalization (i18n).
10. White-labeling theme system + tenant-specific CSS.

---

# 5 — Non-functional requirements

* Security: OWASP-aware, anti-forgery tokens, input validation, XSS/CSRF protection.
* Performance: database indexes, query optimization, caching for expensive queries.
* Scalability: stateless web app (session via distributed cache if needed), background workers horizontally scalable.
* Observability: structured logs (Serilog), metrics (Prometheus/OTel), centralized error tracking.
* Maintainability: Clean Architecture, DI, SOLID, no EF entities leaked to UI.
* Testability: Unit + Integration + e2e smoke tests.
* Deployability: Docker images + docker-compose, optionally Kubernetes-ready manifests.

---

# 6 — Architecture (layers & patterns)

Adopt **Clean Architecture / Onion** style:

* **Domain** (MyRealEstate.Domain)

  * Entities, Value Objects, Domain Events, Domain Exceptions, Interfaces for domain services.
* **Application** (MyRealEstate.Application)

  * DTOs, Use Cases (MediatR commands/queries), Validation (FluentValidation), Mapping (AutoMapper), Interfaces (IRepository abstractions, IFileStorage).
* **Infrastructure** (MyRealEstate.Infrastructure)

  * EF Core DbContext, Migrations, Repositories, Implementation of IFileStorage, EmailSender, Search provider, Logging sinks.
* **Web** (MyRealEstate.Web)

  * Razor Views, Controllers, ViewModels, ViewComponents, Tag Helpers, DI registration (Startup/Program).
* **Tests** (MyRealEstate.Tests)

  * Unit & integration tests.

Patterns:

* CQRS via MediatR for separation of commands/queries.
* Repository + UnitOfWork (thin, limited to db transactions if needed) — but prefer EF DbContext directly behind an interface for testability.
* IFileStorage abstraction for media storage swapping.
* IEmailSender abstraction for emails.
* Options pattern for configuration.
* Background workers using IHostedService.

---

# 7 — Data model (entities & attributes)

Below are core domain entities and key fields. Use EF Core with expressive configuration (Fluent API). Don’t expose domain entities to Views; map to viewmodels/DTOs.

## Entities (concise)

**User**

* Id (GUID)
* Email
* UserName
* PasswordHash (managed by Identity)
* Roles (IdentityRole)
* FullName
* PhoneNumber
* CreatedAt, LastLoginAt
* IsActive, IsVerified
* AgentProfileId (nullable) — link to Agent

**Agent**

* Id (GUID)
* UserId (FK)
* AgencyName
* LicenseNumber
* Bio
* Rating (computed)
* CreatedAt

**Property**

* Id (GUID)
* Title
* Slug
* Description
* Price (decimal)
* Currency (string)
* Type (enum: Sale, Rent)
* Status (enum: Draft, Published, UnderContract, Sold, Archived)
* AddressLine1, AddressLine2, City, State, PostalCode, Country
* Latitude, Longitude (decimal)
* Bedrooms (int), Bathrooms (int), AreaSqM (decimal)
* Features (collection of strings or Feature entity)
* AgentId (FK)
* CreatedAt, PublishedAt, UpdatedAt
* ViewsCount (int)
* MainImageUrl (string)

**PropertyImage**

* Id, PropertyId, Url, FileName, IsPrimary, Width, Height, FileSize, CreatedAt

**Lead / Inquiry**

* Id
* PropertyId (nullable)
* AgentId (nullable)
* Name
* Email
* Phone
* Message
* Source (enum: ContactForm, Phone, ExternalAPI)
* Status (New, Read, Replied)
* CreatedAt

**Message**

* Id
* FromUserId
* ToUserId
* Subject
* Body
* CreatedAt
* ReadAt

**AuditLog**

* Id, UserId, Action, EntityType, EntityId, Changes (JSON), CreatedAt, IpAddress

**SiteContent**

* Id, Key, HtmlContent, LastUpdatedAt

**Statistic** (read-model for dashboard)

* precomputed or derived; can be stored in NoSQL or relational.

ER relationships: User 1:N Agent, Agent 1:N Property, Property 1:N PropertyImage, Property 1:N Lead.

---

# 8 — Textual ER diagram (quick)

```
User (1) --- (0..1) Agent
Agent (1) --- (0..*) Property
Property (1) --- (0..*) PropertyImage
Property (1) --- (0..*) Lead
User (1) --- (0..*) Message (to/from)
```

---

# 9 — API / Controllers / Routes (Web MVC endpoints)

Everything is Razor Views, but controllers also provide JSON endpoints for internal AJAX.

Important controllers:

* `AccountController` — login, logout, register (Admin-only registration).
* `Admin/DashboardController` — GET /admin/dashboard
* `Admin/PropertiesController` — CRUD for properties

  * GET /admin/properties
  * GET /admin/properties/create
  * POST /admin/properties/create
  * GET /admin/properties/edit/{id}
  * POST /admin/properties/edit/{id}
  * POST /admin/properties/delete/{id}
  * POST /admin/properties/publish/{id}
* `Admin/AgentsController` — manage agents
* `Admin/LeadsController` — list/inbox/view/reply leads
* `Admin/UsersController` — manage users & roles
* `Admin/MediaController` — upload/delete images (AJAX)
* `Admin/ReportsController` — download reports (CSV/PDF)
* `Api/PropertiesApiController` — read-only endpoints for public site (if needed)

  * GET /api/properties/search
  * GET /api/properties/{id}

Design considerations:

* Use `Authorize(Roles = "Admin,Agent,Moderator")` with specific policy attributes on actions.
* Return 403 for unauthorized.

---

# 10 — Razor UI structure & best practices

* Use **strongly-typed view models** only.
* Use **ViewComponents** for reusable UI pieces:

  * `ListingCardViewComponent`
  * `PropertyFiltersViewComponent`
  * `AgentProfileViewComponent`
* Use **Partial Views** only for simple markup reuse inside a view.
* Use **Tag Helpers** extensively: `asp-action`, `asp-controller`, `asp-for`.
* Use `IHtmlHelper` extension methods sparingly when necessary.
* Layouts:

  * `_Layout.cshtml` for admin shell with top nav/sidebar.
  * `_AdminNav.cshtml` partial controlling menu items based on roles/policies.
* Client-side enhancements:

  * Use unobtrusive AJAX (jquery-ajax or fetch) for file uploads and modal forms.
  * Use Chart.js (static JS include) for charts — data provided server-side via JSON endpoints.

Accessibility & UX:

* Semantic HTML, proper labels, aria attributes.
* Responsive layout with Tailwind CSS or Bootstrap (Tailwind recommended if you want modern look; otherwise use Bootstrap).
* Breadcrumbs and contextual help.

Naming & folder structure in Web project:

```
/Areas/Admin/Controllers
/Areas/Admin/Views/Properties
/Areas/Admin/Views/Shared/Components
/Views/Shared/_Layout.cshtml
/Views/Shared/_ValidationScriptsPartial.cshtml
/Services (app-level services used by controllers)
```

---

# 11 — Data access & persistence details

* Use **EF Core** with fluent configurations.
* Prefer **Postgres** or **SQL Server** (choose one; Postgres recommended for later geospatial and full text). For demo, SQLite is ok.
* Migrations in `Infrastructure` project.
* Seeding: create Roles (Admin/Agent/Moderator) + demo Admin user + sample agents and properties.
* For performance:

  * Use explicit projection for read listing (select only needed columns).
  * Indexes: Price, City, Status, AgentId, (Latitude, Longitude) for geospatial queries.
* Use `AsNoTracking()` for read queries.

---

# 12 — File storage (IFileStorage)

Interface:

```csharp
public interface IFileStorage {
  Task<FileUploadResult> SaveFileAsync(Stream stream, string fileName, CancellationToken ct = default);
  Task DeleteFileAsync(string filePath, CancellationToken ct = default);
  Task<Stream> GetFileAsync(string filePath, CancellationToken ct = default);
}
```

Implementations:

* `LocalFileStorage` (dev)
* `AzureBlobFileStorage` (prod)

Store image metadata in `PropertyImage`.

---

# 13 — Background processing

* Use `IHostedService` or `BackgroundService`.
* Use background job to:

  * Resize images (multiple sizes).
  * Generate thumbnails.
  * Send batched emails (digest).
  * Run nightly reports.
* Optionally integrate Hangfire if you want dashboarded recurring jobs.

---

# 14 — Search & filtering

* Start with EF LINQ filtered queries and indexes.
* For advanced: integrate Elasticsearch or Postgres full-text search.
* Provide server-side pagination with page size and total count.

---

# 15 — Notifications & email

* IEmailSender interface, with implementations:

  * `SmtpEmailSender` (SMTP)
  * `SendGridEmailSender` (optional)
* Notifications:

  * Email on new lead to assigned agent.
  * Admin dashboard notifications for system alerts.
  * Optional: SignalR hub for real-time browser notifications.

---

# 16 — Logging, monitoring, observability

* **Serilog** for structured logging; sinks: Console, File, Seq (optional).
* Centralized error handling middleware returning friendly error page.
* Integrate **OpenTelemetry** for traces & metrics; export to Prometheus/Grafana in prod.
* Exception handling pattern: log + correlation id + user friendly response.

---

# 17 — Security

* Use ASP.NET Identity with secure password options and account lockout.
* Use HTTPS-only cookie settings.
* Protect admin areas with Role/Policy attributes.
* Use anti-forgery tokens on forms (`@Html.AntiForgeryToken()`).
* Input validation with FluentValidation.
* Rate-limit public API endpoints (if any).
* Secure file upload: validate content-type and extension, scan for malicious files if possible.

---

# 18 — Testing strategy

* **Unit tests**: domain invariants, value objects, services, validation rules (xUnit + Moq).
* **Integration tests**: use in-memory DB or Testcontainers to run EF migrations and test flows (create listing → read listing).
* **UI tests / Smoke tests**: optional Playwright or Selenium to verify critical flows.
* Coverage target: 60–80% on domain & application logic.
* CI runs tests on PRs.

---

# 19 — CI/CD pipeline (GitHub Actions template)

Pipeline steps:

1. `on: push, pull_request`
2. Check out code.
3. Setup .NET SDK (use latest stable you want to target, e.g. .NET 8).
4. Restore, build, test.
5. Run static analysis (`dotnet format --verify-no-changes`, Roslyn analyzers).
6. Build Docker image and push to registry on `main`.
7. Deploy step (optional): push to staging server or container registry.

Example jobs:

* `test` job: run unit & integration tests.
* `build` job: build & produce docker image.
* `deploy` job: deploy to server via SSH or to container registry.

---

# 20 — Docker & docker-compose (sample)

Provide `Dockerfile` for web app and `docker-compose.yml` for local dev:

```yaml
version: '3.8'
services:
  db:
    image: postgres:15
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: example
      POSTGRES_DB: myrealestate
    volumes:
      - db_data:/var/lib/postgresql/data
  web:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=myrealestate;Username=postgres;Password=example
    depends_on:
      - db
volumes:
  db_data:
```

Add migrations and run `dotnet ef database update` in container startup or via entrypoint script for dev.

---

# 21 — Dev environment & run commands

* Prereqs: .NET SDK (8+), Docker, Node (if you use Tailwind).
* Clone repo:

  ```bash
  git clone git@github.com:you/MyRealEstate.git
  cd MyRealEstate
  docker-compose up -d
  # or run locally
  dotnet restore
  dotnet build
  dotnet ef database update --project src/MyRealEstate.Infrastructure
  cd src/MyRealEstate.Web
  dotnet run
  ```
* Seed user creation via `dotnet run` or migration seed.

---

# 22 — Coding standards & quality rules

* Use `async` all the way for I/O.
* CancellationToken on async APIs.
* Avoid `dynamic` or passing EF entities to views.
* Use small, well-named services; favor composition over inheritance.
* Use `IOptions<T>` for configuration.
* Use DI for everything; register in Program.cs with extension methods (e.g., `services.AddInfrastructure(configuration)`).
* Use `Swashbuckle` only for APIs (if any).
* Add Git hooks or GitHub actions to run `dotnet format`.

---

# 23 — Documentation & deliverables (what to include in repo)

* `README.md` (detailed run & architecture).
* `ARCHITECTURE.md` (justification of choices).
* `ERD.png` or ASCII ERD + `SEQUENCE_DIAGRAMS.md`.
* `DEPLOYMENT.md` (how to deploy to Docker, Azure Web App, or Kubernetes).
* `CHANGELOG.md`
* `SECURITY.md` (how to configure secrets, TLS, security checklist).
* `LICENSE` (MIT or choose one).
* `DEMO.md` and `DEMO_VIDEO.mp4` (optional).
* `docs/` directory for API, architecture decisions, and integration points.
* Seed SQL / JSON files.

---

# 24 — Demo script / elevator demo (for interviews & clients)

1. Landing admin login (show roles).
2. Admin dashboard: KPIs + charts (show live filters).
3. Create property — media upload — publish.
4. Show lead creation (simulate visitor form) → agent gets dashboard notification + email.
5. Show search & filter + paginated results.
6. Show logs & audit trail for property change.
7. Show deployment: Docker compose up (or deployed URL).
8. Show unit tests and CI pipeline run.

Include sample talk track emphasizing Clean Architecture, separation of concerns, testing, and production-readiness.

---

# 25 — Monetization / go-to-market & packaging for sale

* **Product tiers**:

  * Free (limited features, watermark).
  * Standard (single company, core features).
  * Premium (multi-tenant, analytics, SLA, priority support).
* **Licensing**: SaaS subscription (monthly), or perpetual license with support contract.
* **Upsells**: Custom theming, data migration, premium integrations (CRMs, MLS).
* **Pitch assets**: sales one-pager, demo video, pricing page, sample contract.

---

# 26 — Legal / privacy / compliance

* Provide privacy policy and terms templates.
* Data retention policy for leads/messages.
* GDPR considerations: consent capture on forms, export/delete user data endpoints.
* Secure storage of sensitive data (no plaintext storage of PII).

---

# 27 — Accessibility & internationalization

* Use localization for labels and currencies (IStringLocalizer).
* Currency formatting using CultureInfo.
* Right-to-left support optional.
* WCAG level AA compliance for admin portal (forms, contrast, keyboard nav).

---

# 28 — Deliverable Checklist (what you must produce to be sellable & CV-worthy)

* [ ] Clean Architecture repo with 4 projects (Web, Application, Domain, Infrastructure).
* [ ] Razor Views only (no SPA); ViewComponents and Tag Helpers used.
* [ ] EF Core migrations + seed data.
* [ ] Authentication & role management (Identity).
* [ ] Dashboard with charts.
* [ ] Image upload with storage abstraction and background processing for thumbnails.
* [ ] Search & filters with pagination.
* [ ] Lead/message flow (store + agent notification).
* [ ] Serilog logs and global error handling.
* [ ] Unit + Integration tests (CI run).
* [ ] Docker + docker-compose + README for running.
* [ ] CI pipeline (GitHub Actions) building, testing, and publishing.
* [ ] Documentation: README, ARCHITECTURE, DEPLOYMENT, DEMO.
* [ ] Demo script + screenshots/video.
* [ ] License and basic legal docs (privacy/terms).

---

# 29 — Timeline & milestones (suggested, aggressive but realistic)

* Week 1: Project scaffold, domain model, EF migrations, Identity, DI, basic UI layout.
* Week 2: Property CRUD, file upload, storage abstraction, basic dashboard.
* Week 3: Leads/messages, notifications, search, pagination.
* Week 4: Background worker, image processing, tests start, plus polishing UI.
* Week 5: Advanced features (SignalR, full-text), CI/CD pipeline, Docker polish.
* Week 6: Testing, docs, demo recording, final polish, prepare pitch materials.

Adjust to your available hours; compress or extend as needed.

---

# 30 — Example README skeleton (to paste into your repo)

```
# MyRealEstate — Admin Portal

## Overview
Production-ready admin portal for real estate companies. Built with ASP.NET Core and Razor Views using Clean Architecture.

## Features
- Role-based admin (Admin/Agent/Moderator)
- Property management with image uploads
- Lead management with email/notification
- Dashboard and analytics
- Background processing and scheduled reports
- Dockerized for local dev and production

## Getting started (dev)
Prereqs: .NET 8 SDK, Docker

1. `git clone ...`
2. `docker-compose up -d`
3. `cd src/MyRealEstate.Web`
4. `dotnet run`
5. Visit `https://localhost:5001` and login with seeded admin.

## Tech
- .NET 8, ASP.NET Core MVC (Razor)
- EF Core, Postgres
- MediatR, FluentValidation, AutoMapper
- Serilog, OpenTelemetry
- Docker, GitHub Actions

## Docs
See `ARCHITECTURE.md`, `DEPLOYMENT.md`, `SECURITY.md`
```

---

# 31 — Example code snippets (starter patterns)

**Program.cs DI registration pattern**

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthentication().AddIdentityServerCookies(); // example
// build
var app = builder.Build();
// middleware pipeline
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(...);
app.Run();
```

**MediatR command handler pattern**

```csharp
public record CreatePropertyCommand(PropertyCreateDto Dto) : IRequest<Result<Guid>>;

public class CreatePropertyHandler : IRequestHandler<CreatePropertyCommand, Result<Guid>>
{
  private readonly IApplicationDbContext _db;
  private readonly IFileStorage _fileStorage;
  public CreatePropertyHandler(IApplicationDbContext db, IFileStorage fileStorage) { ... }
  public async Task<Result<Guid>> Handle(CreatePropertyCommand request, CancellationToken ct) {
    // validate, map to entity, persist, dispatch domain events
  }
}
```

---

# 32 — Example seed data (JSON snippet)

```json
{
  "roles": ["Admin","Agent","Moderator"],
  "users": [
    {"email":"admin@local.test","password":"P@ssword123","roles":["Admin"]},
    {"email":"agent1@local.test","password":"P@ssword123","roles":["Agent"]}
  ],
  "properties": [
    {"title":"Modern 3BR apartment","price":120000,"city":"Tunis","type":"Sale","status":"Published"}
  ]
}
```

---

# 33 — How you show it on your CV / LinkedIn

* Project name + link to GitHub
* One-line: "Admin Portal for Real Estate — ASP.NET Core (Razor), Clean Architecture, EF Core, Docker, MediatR, Serilog; multi-role admin, file processing, real-time notifications."
* Bullet points:

  * Designed & implemented Clean Architecture with Domain, Application, Infrastructure and Web layers.
  * Implemented RBAC, image processing pipeline, background jobs, and a production-ready CI/CD pipeline.
  * Built extensible file storage (Local/Azure Blob), and implemented full test coverage (unit + integration).
* Add a small screenshot of dashboard and a link to demo.

---

# 34 — Final checklist for making it sellable

* Offer an installer or docker-compose with single command to run.
* Add branding/theme switcher so clients can white-label quickly.
* Provide documentation for onboarding and customization.
* Implement a small admin-configurable settings page (company name, logo, email templates).
* Create a simple contracts or SLA negotiation doc (pricing tiers).
* Provide migration/import for existing MLS/CSV data.

---

# 35 — Closing: artifact you can feed to an AI

This whole document is designed to be copy-pasted into an AI prompt as "project spec" and used to generate scaffolds, code samples, tests, README, migration, Dockerfile, and even PR pipelines.

If you want, I can now:

* generate the **full project scaffold** file-by-file (Program.cs, sample controllers, EF entities, DbContext config, one View with ViewComponent) ready to paste into files,
* or produce a **complete README + ARCHITECTURE.md** file content that you can drop into repo,
* or generate the **GitHub Actions** YAML and `Dockerfile` next.

No need to tell me anything else. I already assembled everything you asked for into this single document. Pick which output you want next and I’ll create it precisely.
