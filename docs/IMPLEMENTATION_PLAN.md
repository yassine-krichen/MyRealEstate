# MyRealEstate - MVP Implementation Plan

## Current State Assessment

### ✅ What's Implemented

1. **Domain Layer** - COMPLETE
    - All entities: User, Property, PropertyImage, Inquiry, ConversationMessage, Deal, PropertyView, ContentEntry
    - Value Objects: Money, Address
    - Enums: PropertyStatus, InquiryStatus, DealStatus, SenderType
    - Interfaces: IAuditable, ISoftDelete
    - Business methods on entities (Inquiry status transitions, Deal calculations)

2. **Infrastructure Layer** - MOSTLY COMPLETE
    - ApplicationDbContext with proper configuration
    - Entity configurations for all entities
    - Migrations applied
    - Database seeding (roles, admin user, content entries)
    - DependencyInjection setup
    - ASP.NET Identity integration
    - SQLite database configured

3. **Web Layer** - MINIMAL
    - Basic authentication (AccountController: Login/Register/Logout)
    - Basic Dashboard (showing statistics)
    - Folder structure in place
    - Areas/Admin structure exists

### ❌ What's Missing (MVP Requirements)

#### 1. **Application Layer** - EMPTY

- No DTOs
- No Commands/Queries (MediatR)
- No Validators (FluentValidation)
- No Mappers (AutoMapper or manual)
- No abstractions (IFileStorage, IEmailSender, etc.)
- **Impact**: Violates Clean Architecture - business logic is in controllers

#### 2. **Property Management** (EPIC 1)

- ❌ No Property CRUD controllers/views
- ❌ No image upload functionality
- ❌ No file storage implementation
- ❌ No property listing/search/filter
- ❌ No property details page
- ❌ No PropertyView tracking

#### 3. **Inquiry Management** (EPIC 2)

- ❌ No inquiry creation form
- ❌ No inquiry assignment workflow
- ❌ No inquiry list/detail views

#### 4. **Messaging** (EPIC 3)

- ❌ No conversation UI
- ❌ No reply functionality

#### 5. **Deals** (EPIC 4)

- ❌ No deal creation workflow

#### 6. **Content Management** (EPIC 6)

- ❌ No content editor

#### 7. **Testing** (EPIC 9)

- ❌ No test project
- ❌ No unit tests
- ❌ No integration tests

#### 8. **Logging & Error Handling**

- ❌ No Serilog configuration
- ❌ No global error handling middleware

#### 9. **ViewModels**

- ❌ Only ErrorViewModel exists
- ❌ No property view models
- ❌ No inquiry view models
- ❌ No form view models

#### 10. **UI Components**

- ❌ No ViewComponents
- ❌ No custom Tag Helpers

---

## Implementation Plan (Prioritized)

### Phase 1: Foundation & Architecture (Week 1) ✅ COMPLETE

**Goal**: Fix Clean Architecture violations and set up proper Application layer

#### 1.1 Application Layer Structure

- [x] Create folder structure:
    ```
    Application/
      Commands/
        Properties/
        Inquiries/
        Deals/
      Queries/
        Properties/
        Inquiries/
        Dashboard/
      DTOs/
      Validators/
      Mappings/
      Interfaces/
      Common/
        Behaviors/
        Exceptions/
        Models/
    ```

#### 1.2 Install NuGet Packages

- [x] Application Layer:
    - MediatR (12.x)
    - FluentValidation (11.x)
    - FluentValidation.DependencyInjectionExtensions
    - AutoMapper (12.x)
    - AutoMapper.Extensions.Microsoft.DependencyInjection

- [x] Infrastructure Layer:
    - Serilog.AspNetCore
    - Serilog.Sinks.Console
    - Serilog.Sinks.File

#### 1.3 Application Layer - Core Abstractions

- [x] Create `IApplicationDbContext` interface (for testability)
- [x] Create `IFileStorage` interface
- [x] Create `IEmailSender` interface
- [x] Create `ICurrentUserService` interface
- [x] Create common result/response types

#### 1.4 Application Layer - DependencyInjection

- [x] Create `DependencyInjection.cs` in Application
- [x] Register MediatR
- [x] Register FluentValidation validators
- [x] Register AutoMapper profiles
- [x] Register pipeline behaviors (validation, logging)

#### 1.5 Infrastructure - Service Implementations

- [x] Implement `LocalFileStorage` (save to wwwroot/uploads)
- [x] Implement `SmtpEmailSender` (or mock for now)
- [x] Implement `CurrentUserService` (get user from HttpContext)
- [x] Update Infrastructure DependencyInjection to register these

#### 1.6 Logging & Error Handling

- [x] Configure Serilog in Program.cs
- [x] Create global exception handling middleware
- [x] Add correlation IDs for request tracking

---

### Phase 2: Property Management (Week 2) ✅ COMPLETE

**Goal**: Complete EPIC 1 - Core asset management

#### 2.1 Application Layer - Property Commands ✅

- [x] `CreatePropertyCommand` + Handler
- [x] `UpdatePropertyCommand` + Handler
- [x] `DeletePropertyCommand` (soft delete) + Handler
- [x] `PublishPropertyCommand` + Handler
- [x] `UploadPropertyImageCommand` + Handler
- [x] `DeletePropertyImageCommand` + Handler
- [x] `SetMainImageCommand` + Handler
- [x] Validators for each command

#### 2.2 Application Layer - Property Queries ✅

- [x] `GetPropertyByIdQuery` + Handler
- [x] `GetPropertiesQuery` (with filters, pagination) + Handler
- [x] `SearchPropertiesQuery` + Handler
- [x] Property DTOs (List, Detail, Create, Update)

#### 2.3 Application Layer - Mapping ✅

- [x] AutoMapper profile: Property ↔ DTOs

#### 2.4 Web Layer - ViewModels ✅

- [x] `PropertyListViewModel`
- [x] `PropertyDetailViewModel`
- [x] `PropertyCreateViewModel`
- [x] `PropertyEditViewModel`
- [x] `PropertyImageViewModel`
- [x] `PropertySearchViewModel`

#### 2.5 Web Layer - Admin/PropertiesController ✅

- [x] Index (list with filters, pagination, sorting)
- [x] Create (GET/POST)
- [x] Edit (GET/POST)
- [x] Delete (GET/POST)
- [x] Publish (POST)
- [x] UploadImage (POST)
- [x] DeleteImage (POST)
- [x] SetMainImage (POST)
- [x] Refactored to use MediatR (thin controllers)

#### 2.6 Web Layer - Views ✅

- [x] Admin/Properties/Index.cshtml (table with filters)
- [x] Admin/Properties/Create.cshtml (form)
- [x] Admin/Properties/Edit.cshtml (form + image gallery)
- [x] Admin/Properties/Delete.cshtml (confirmation)
- [x] Admin/Properties/Details.cshtml (view property)

#### 2.7 Public Property Listing (DEFERRED - Not MVP critical)

- [ ] Public/PropertiesController (Index, Details)
- [ ] Public/Properties/Index.cshtml (search + filters)
- [ ] Public/Properties/Details.cshtml (property info + inquiry form)
- [ ] Track PropertyView on details page view

---

### Phase 3: Inquiry & Messaging (Week 3)

**Goal**: Complete EPIC 2 & 3 - Lead management & conversations

#### 3.1 Application Layer - Inquiry Commands

- [ ] `CreateInquiryCommand` + Handler (from public form)
- [ ] `AssignInquiryToAgentCommand` + Handler
- [ ] `UpdateInquiryStatusCommand` + Handler
- [ ] `AddMessageToInquiryCommand` + Handler
- [ ] `CloseInquiryCommand` + Handler
- [ ] Validators

#### 3.2 Application Layer - Inquiry Queries

- [ ] `GetInquiryByIdQuery` + Handler
- [ ] `GetInquiriesQuery` (with filters) + Handler
- [ ] `GetInquiryMessagesQuery` + Handler
- [ ] Inquiry DTOs

#### 3.3 Web Layer - ViewModels

- [ ] `InquiryListViewModel`
- [ ] `InquiryDetailViewModel`
- [ ] `InquiryCreateViewModel` (public form)
- [ ] `MessageViewModel`

#### 3.4 Web Layer - Admin/InquiriesController

- [ ] Index (list with filters)
- [ ] Details (view inquiry + messages)
- [ ] Assign (POST)
- [ ] Reply (POST - add message)
- [ ] Close (POST)

#### 3.5 Web Layer - Views

- [ ] Admin/Inquiries/Index.cshtml
- [ ] Admin/Inquiries/Details.cshtml (conversation thread)
- [ ] Admin/Inquiries/\_MessageList.cshtml (partial)
- [ ] Admin/Inquiries/\_ReplyForm.cshtml (partial)

#### 3.6 Email Notifications

- [ ] Send email when inquiry created (to admin)
- [ ] Send email when inquiry assigned (to agent)
- [ ] Email templates in ContentEntry

---

### Phase 4: Dashboard Enhancement (Week 3)

**Goal**: Complete EPIC 5 - Make dashboard production-ready

#### 4.1 Application Layer - Dashboard Queries

- [ ] `GetDashboardStatsQuery` + Handler
- [ ] `GetPropertyStatisticsQuery` + Handler
- [ ] `GetInquiryStatisticsQuery` + Handler
- [ ] `GetAgentPerformanceQuery` + Handler
- [ ] Dashboard DTOs

#### 4.2 Web Layer - Dashboard Improvements

- [ ] Refactor DashboardController to use MediatR
- [ ] Move DashboardViewModel to Web/Models
- [ ] Add charts (Chart.js)
- [ ] Add time range filters

#### 4.3 Web Layer - Dashboard View

- [ ] Enhanced Admin/Dashboard/Index.cshtml
- [ ] KPI cards
- [ ] Charts (properties by status, inquiries over time)
- [ ] Recent activity feed

---

### Phase 5: Deals & Closing (Week 4)

**Goal**: Complete EPIC 4 - Revenue tracking

#### 5.1 Application Layer - Deal Commands

- [ ] `CreateDealCommand` + Handler
- [ ] `CompleteDealCommand` + Handler
- [ ] `CancelDealCommand` + Handler
- [ ] Validators (property not already sold, etc.)

#### 5.2 Application Layer - Deal Queries

- [ ] `GetDealByIdQuery` + Handler
- [ ] `GetDealsQuery` + Handler
- [ ] Deal DTOs

#### 5.3 Web Layer - Admin/DealsController

- [ ] Index (list deals)
- [ ] Create (GET/POST)
- [ ] Details (GET)
- [ ] Complete (POST)
- [ ] Cancel (POST)

#### 5.4 Web Layer - Views

- [ ] Admin/Deals/Index.cshtml
- [ ] Admin/Deals/Create.cshtml
- [ ] Admin/Deals/Details.cshtml

#### 5.5 Business Logic

- [ ] When deal created: update Property.Status to Sold/Rented
- [ ] Close related inquiry automatically
- [ ] Calculate commission

---

### Phase 6: Content Management (Week 4)

**Goal**: Complete EPIC 6 - Simple CMS

#### 6.1 Application Layer - Content Commands

- [ ] `UpdateContentCommand` + Handler
- [ ] `GetContentByKeyQuery` + Handler

#### 6.2 Web Layer - Admin/ContentController

- [ ] Index (list all content entries)
- [ ] Edit (GET/POST)

#### 6.3 Web Layer - Views

- [ ] Admin/Content/Index.cshtml
- [ ] Admin/Content/Edit.cshtml (WYSIWYG editor or textarea)

#### 6.4 Content Service

- [ ] Create `IContentService` in Application
- [ ] Implement caching for content
- [ ] Use in views for editable content

---

### Phase 7: User & Role Management (Week 4)

**Goal**: Complete user administration

#### 7.1 Web Layer - Admin/UsersController

- [ ] Index (list users)
- [ ] Create (GET/POST) - Admin creates Agent accounts
- [ ] Edit (GET/POST)
- [ ] ManageRoles (GET/POST)
- [ ] Deactivate/Activate (POST)

#### 7.2 Web Layer - Views

- [ ] Admin/Users/Index.cshtml
- [ ] Admin/Users/Create.cshtml
- [ ] Admin/Users/Edit.cshtml
- [ ] Admin/Users/ManageRoles.cshtml

---

### Phase 8: UI Components & Polish (Week 5)

**Goal**: Reusable components and better UX

#### 8.1 ViewComponents

- [ ] `PropertyCardViewComponent` (for listings)
- [ ] `PropertyFiltersViewComponent` (search filters)
- [ ] `PaginationViewComponent`
- [ ] `RecentActivitiesViewComponent` (for dashboard)

#### 8.2 Tag Helpers

- [ ] `ImageTagHelper` (for property images with fallback)
- [ ] `StatusBadgeTagHelper` (colored badges for statuses)

#### 8.3 Layouts & Navigation

- [ ] Improve \_Layout.cshtml
- [ ] Role-based menu in \_AdminNav.cshtml
- [ ] Breadcrumbs
- [ ] User dropdown (profile, logout)

#### 8.4 Client-side

- [ ] Add DataTables.js for sortable/filterable tables
- [ ] Add Chart.js for dashboard charts
- [ ] Add image preview on upload
- [ ] Add confirmation dialogs for delete actions

---

### Phase 9: Testing (Week 5)

**Goal**: Complete EPIC 9 - Test coverage

#### 9.1 Setup Test Project

- [ ] Create MyRealEstate.Tests project
- [ ] Install packages:
    - xUnit
    - Moq
    - FluentAssertions
    - Microsoft.EntityFrameworkCore.InMemory

#### 9.2 Unit Tests - Domain

- [ ] Test Inquiry status transitions
- [ ] Test Deal commission calculation
- [ ] Test Property business methods
- [ ] Test value objects (Money, Address)

#### 9.3 Unit Tests - Application

- [ ] Test CreatePropertyCommand handler
- [ ] Test property validators
- [ ] Test inquiry workflow handlers
- [ ] Mock IApplicationDbContext, IFileStorage

#### 9.4 Integration Tests

- [ ] Test Property CRUD flow (create → read → update → delete)
- [ ] Test Inquiry → Deal flow
- [ ] Use in-memory database or Testcontainers
- [ ] Test with WebApplicationFactory

---

### Phase 10: Security & Final Polish (Week 6)

**Goal**: Production-ready security and deployment

#### 10.1 Security

- [ ] Add authorization policies (CanManageProperties, CanAssignInquiries)
- [ ] Ensure agents can only edit their properties
- [ ] Ensure agents can only see assigned inquiries
- [ ] Add CSRF protection (already in forms)
- [ ] Validate file uploads (type, size)
- [ ] Add rate limiting

#### 10.2 Validation & Error Handling

- [ ] User-friendly error pages (404, 500)
- [ ] Validation summary on all forms
- [ ] Model state errors

#### 10.3 Documentation

- [ ] Update README.md with:
    - Architecture explanation
    - How to run
    - How to seed data
    - How to test
- [ ] Add inline code comments
- [ ] Add XML documentation for public APIs

#### 10.4 Docker

- [ ] Create Dockerfile for Web project
- [ ] Create docker-compose.yml (web + db)
- [ ] Test local deployment

#### 10.5 CI/CD

- [ ] Create GitHub Actions workflow:
    - Build
    - Test
    - Publish Docker image (optional)

---

## Priority Order for Immediate Work

Based on MVP requirements, start in this order:

### CRITICAL (Do First)

1. **Application Layer Setup** (Phase 1.1-1.4)
    - This fixes the architecture violation
2. **File Storage** (Phase 1.5)
    - Required for property images
3. **Serilog** (Phase 1.6)
    - MVP requirement

### HIGH PRIORITY (Do Next)

4. **Property CRUD** (Phase 2)
    - Core feature, everything depends on it
5. **Inquiry Creation** (Phase 3.1-3.2)
    - Public form + backend
6. **Basic Dashboard** (Phase 4)
    - Already partially done, needs refactoring

### MEDIUM PRIORITY

7. **Messaging** (Phase 3.3-3.5)
8. **Deals** (Phase 5)
9. **Testing** (Phase 9.2-9.3 at minimum)

### LOWER PRIORITY (Can defer)

10. Content Management (Phase 6)
11. User Management (Phase 7)
12. ViewComponents (Phase 8)
13. Docker/CI (Phase 10)

---

## Metrics for Success (MVP Checklist)

From REALESTATEPROJECT.md, the MVP must have:

- [x] ASP.NET Identity with roles ✅
- [ ] Property CRUD ❌
- [ ] Image upload + gallery ❌
- [ ] Dashboard with KPIs and charts ⚠️ (partially done)
- [ ] Search & filtering ❌
- [ ] Leads & messaging ❌
- [ ] Role-based UI ⚠️ (authorization exists, UI not complete)
- [ ] Logging & error handling ❌
- [ ] Unit tests + 1 integration test ❌
- [ ] README + run instructions ❌

**Current MVP Completion: ~15%**

---

## Technical Debt to Address

1. **Architecture Violation**: DashboardController directly uses ApplicationDbContext
    - Fix: Create queries in Application layer
2. **No ViewModels**: Controllers return domain entities to views
    - Fix: Create ViewModels for all views
3. **Missing Abstractions**: No IFileStorage, IEmailSender
    - Fix: Create interfaces in Application, implement in Infrastructure
4. **No Validation**: No FluentValidation
    - Fix: Add validators for all commands
5. **Fat Controllers**: Business logic in controllers
    - Fix: Move to Application layer via MediatR

---

## Estimated Timeline

- **Phase 1 (Foundation)**: 2-3 days
- **Phase 2 (Properties)**: 3-4 days
- **Phase 3 (Inquiries)**: 2-3 days
- **Phase 4 (Dashboard)**: 1 day
- **Phase 5 (Deals)**: 2 days
- **Phase 6 (Content)**: 1 day
- **Phase 7 (Users)**: 1-2 days
- **Phase 8 (UI)**: 2-3 days
- **Phase 9 (Testing)**: 2-3 days
- **Phase 10 (Polish)**: 2-3 days

**Total: 18-27 days of focused work**

For MVP only (Phases 1-5 + basic testing): **12-17 days**

---

## Next Steps

Start with Phase 1.1: Create the Application layer structure and install required NuGet packages. This will fix the architecture and enable proper implementation of all other features.
