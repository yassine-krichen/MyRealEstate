# TASK BREAKDOWN

Designed so an agent can’t “speedrun” it into a broken system. This is professional, layered, boring in the right way.

I’ll structure this as **Epics → Features → Tasks → Acceptance criteria**.
Language: clean, neutral, production-grade.

---

## EPIC 0 — Foundations & Architecture

> If this is weak, everything above it collapses quietly.

### 0.1 Solution structure

**Tasks**

* Create solution with layers:

  * `Domain`
  * `Application`
  * `Infrastructure`
  * `Web` (MVC)
* Enforce dependency rule:
  `Web → Application → Domain`
  `Infrastructure → Application`

**Acceptance**

* Domain has no framework dependencies
* EF Core only exists in Infrastructure
* Controllers are thin (no business logic)

---

### 0.2 Identity & Roles

**Tasks**

* Configure ASP.NET Identity with `User` entity
* Seed roles: `Admin`, `Agent`
* Seed one Admin user

**Acceptance**

* Role-based authorization works
* Admin can log in and see admin-only routes
* Agent cannot access user management

---

## EPIC 1 — Property Management

> Core asset. Everything else orbits this.

### 1.1 Property CRUD

**Tasks**

* Create `Property` entity + EF mapping
* Implement Create/Edit/Delete (soft delete)
* Draft vs Published logic
* Validation rules (price > 0, required address, etc.)

**Acceptance**

* Properties default to `Draft`
* Cannot publish invalid property
* Soft-deleted properties never appear in public listings

---

### 1.2 Property Media

**Tasks**

* Implement `PropertyImage`
* Upload, delete, mark main image
* Store files in filesystem or blob storage

**Acceptance**

* One image can be main
* Images load correctly in listings
* Deleting a property does not delete physical files (safe cleanup later)

---

### 1.3 Property Public Listing

**Tasks**

* Public listing page (search + filter)
* Property details page
* Increment `PropertyView`

**Acceptance**

* Filters work (city, price, type)
* Views are counted once per session
* Page load does not aggregate views live

---

## EPIC 2 — Inquiry (Request) Lifecycle

> This is where business actually happens.

### 2.1 Inquiry Creation (Public)

**Tasks**

* Public inquiry form on property page
* Create `Inquiry` with initial message
* Status = `New`
* Optional email notification

**Acceptance**

* Inquiry saved correctly
* InitialMessage populated
* No agent assigned by default

---

### 2.2 Inquiry Assignment & Workflow

**Tasks**

* Admin assigns inquiry to agent
* Status transitions:

  * New → Assigned
  * Assigned → InProgress
  * InProgress → Answered / Closed
* Enforce valid transitions

**Acceptance**

* Invalid transitions are blocked
* Assigned agent is persisted
* SLA queries are possible

---

## EPIC 3 — Messaging (Conversation)

> Messages support decisions, not the other way around.

### 3.1 Conversation Messaging

**Tasks**

* Create `ConversationMessage`
* Agent/Admin can reply
* Visitor replies via public tokenized link
* Internal notes support

**Acceptance**

* Messages are ordered chronologically
* Internal notes visible only to staff
* Inquiry list shows last message preview

---

## EPIC 4 — Deals & Closing

> This is where money enters the chat.

### 4.1 Deal Creation

**Tasks**

* Create `Deal` entity
* Link to Property + Inquiry
* Assign closing agent
* Validate property not already sold

**Acceptance**

* Deal cannot exist without property + agent
* Property status updates to Sold/Rented
* Inquiry closes automatically

---

### 4.2 Commission & Revenue Tracking

**Tasks**

* Calculate commission amount
* Persist financial fields
* Protect against negative values

**Acceptance**

* Commission calculations are correct
* Revenue queries return consistent data

---

## EPIC 5 — Back Office Dashboard

> This is what your professor actually wants to see.

### 5.1 Dashboard Metrics

**Tasks**

* Implement queries for:

  * Total properties by status
  * New inquiries (7/30 days)
  * Open inquiries
  * Deals this month
  * Top agents by deals

**Acceptance**

* Dashboard loads under 300ms
* Queries use indexes
* No N+1 issues

---

### 5.2 Agent Performance

**Tasks**

* Agent KPI page:

  * Inquiries assigned
  * Deals closed
  * Avg time to close
  * Conversion rate

**Acceptance**

* Metrics match database truth
* Time-based filters work

---

## EPIC 6 — Content Management (Mini CMS)

> This is not fluff. It saves deploys.

### 6.1 Content Entries

**Tasks**

* CRUD for `ContentEntry`
* HTML-safe editing
* Cache content in memory

**Acceptance**

* Admin edits reflect instantly
* No code redeploy needed
* Missing content keys fail gracefully

---

## EPIC 7 — Statistics & Analytics

> If it’s not measurable, it didn’t happen.

### 7.1 Analytics Events

**Tasks**

* Persist `PropertyView`
* Background job to aggregate counts
* Clean up old raw events if needed

**Acceptance**

* View counts accurate
* Historical trends available
* System does not degrade over time

---

## EPIC 8 — Security, Validation & Quality

> Invisible, but mandatory.

### 8.1 Authorization Rules

**Tasks**

* Policy-based authorization:

  * Agent can access only assigned inquiries
  * Admin has global access

**Acceptance**

* Direct URL access is blocked
* No reliance on UI hiding

---

### 8.2 Validation & Error Handling

**Tasks**

* FluentValidation for commands
* Centralized exception handling
* User-friendly error messages

**Acceptance**

* Invalid data never reaches DB
* Errors are logged, not swallowed

---

## EPIC 9 — Testing & Hardening

> Optional in class. Mandatory in reality.

### 9.1 Tests

**Tasks**

* Unit tests for:

  * Inquiry transitions
  * Deal creation
* Integration tests for:

  * Inquiry → Deal flow

**Acceptance**

* Core workflows covered
* Tests are deterministic

---

## Final note (important)

This breakdown:

* Prevents scope creep
* Enforces correct business flow
* Makes “sloppy agent output” hard
* Maps cleanly to your schema
* Looks professional in front of any professor or reviewer