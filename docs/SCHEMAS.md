## 1 — Message vs Request: what they are and how to model them correctly

Short: a **Request (Inquiry)** is a business object representing *a client intent* (someone asking about a property, scheduling a visit, etc.). A **Message** is a single communication unit (a row in a conversation) that belongs to a Request.

Why separate them:

* The request is the thing you triage, assign, measure and close.
* Messages are the thread of conversation that explains how the request progressed.

Merging everything into a single table (one table for both) looks tidy at first, but it blurs:

* status lifecycle (New → Assigned → Answered → Closed)
* assignment and SLA tracking
* analytics (conversion of requests → deals)

**Recommendation (best practice):**

* Keep `Inquiry` (Request/Lead) as the root.
* Add `ConversationMessage` (or `Communication`) as child rows linked to an `Inquiry`.
* Keep first contact fields on the `Inquiry` (visitorName, visitorEmail, initialMessage) for fast queries.

This gives you both business-sense and auditability.

---

## 2 — What is `Content` and why you should keep it (don’t be lazy)

`Content` = editable site text stored in DB (home hero, about section, contact page, email templates).

Why real devs do this:

* Admins must update marketing/copy without a deploy.
* Partial Views are code. Non-devs can’t edit them.
* Storing content in DB + a small WYSIWYG editor = low-cost flexibility.

So yes: keep `Content` rows keyed by `Key` (HomeHero, AboutHtml, FooterText, EmailTemplate_LeadReceived). Pull them into Razor via a `ContentService`. It’s trivial and extremely useful for sales.

---

## 3 — Tracking closed deals (who closed the sale) and stats we need

You asked which agent closed a deal — that’s a core KPI. Add a `Deal` (Transaction) entity:

Deal ties property + agent + financials + timestamps. When a deal is recorded:

* Property.Status → Sold (or Rented)
* Property.ClosedDealId → Deal.Id
* Agent gets credited in stats

Also track `PropertyView` events and `Inquiry` → `Deal` conversion.

Important stat queries we must support (design schema to make these cheap):

1. `TotalPropertiesByStatus()` (Draft/Published/Sold)
2. `NewInquiriesLast30Days()` (time-series)
3. `ConversionRate(agent)` = deals_closed_by_agent / inquiries_assigned_to_agent
4. `AverageTimeToClose(agent)` = avg(Deal.ClosedAt - Inquiry.CreatedAt) for agent
5. `TopPropertiesByViews()` last N days
6. `TopAgentsByDeals()` last N months
7. `PropertiesPublishedPerMonth()` time-series
8. `LeadsPerSource()` if you capture source
9. `OpenInquiries()` and SLA breaches (e.g., >24h unassigned/unanswered)
10. `RevenueByAgent()` and `CommissionStats()` (if you track financials)

To calculate these: keep `Inquiry.AssignedAgentId`, `Inquiry.CreatedAt`, `Inquiry.ClosedAt` (nullable), and `Deal.ClosedAt`, `Deal.AgentId`. Keep `PropertyView` events for view counts and time-series.

Indexes: create indexes on `Inquiry(AssignedAgentId, Status, CreatedAt)`, `Deal(AgentId, ClosedAt)`, `Property(City, Status, Price)`, `PropertyView(PropertyId, ViewedAt)`.

---

## 4 — Roles & responsibilities (Admin vs Agent)

Keep it simple and sane:

* **Admin**

  * Full access: user management, role assignment, content editing, configuration
  * Can manage any property, any inquiry, any deal
  * Can seed system settings, run data cleanup

* **Agent**

  * Can manage properties they own/are assigned to (Create/Edit if authorized)
  * Can view and respond to inquiries assigned to them
  * Can mark inquiries as answered/closed or escalate
  * Can record/close deals for properties they handled

Authorization rules:

* Admin = global superpower
* Agent = limited to assigned resources (Enforce via policy handlers)
* Separation of concerns: controllers call Application services/commands that check authorization, not views.

---

## 5 — Full class-diagram (textual; C#-style with types & explanations)

This is the canonical shape you want in your architecture layer (Domain entities). I include audit fields and soft-delete. Use `decimal` for money, `Guid` for ids. Value objects follow after.

### Value objects

```csharp
public class Money {
  public decimal Amount { get; private set; }
  public string Currency { get; private set; } // "TND", "USD", etc.
}

public class Address {
  public string Line1 { get; private set; }
  public string Line2 { get; private set; }
  public string City { get; private set; }
  public string State { get; private set; }
  public string PostalCode { get; private set; }
  public string Country { get; private set; }
  public decimal? Latitude { get; private set; } // optional
  public decimal? Longitude { get; private set; } // optional
}
```

### Entities

```csharp
public class User {               // Identity user (extend IdentityUser<Guid>)
  public Guid Id { get; set; }
  public string Email { get; set; }
  public string FullName { get; set; }
  public bool IsActive { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? LastLoginAt { get; set; }
  public bool IsDeleted { get; set; }
}

public class Property {
  public Guid Id { get; set; }
  public string Title { get; set; }
  public string Slug { get; set; }               // SEO, optional
  public string Description { get; set; }
  public Money Price { get; set; }
  public string PropertyType { get; set; }      // "Apartment","House","Land"
  public PropertyStatus Status { get; set; }    // enum: Draft, Published, UnderOffer, Sold, Rented, Archived
  public Address Address { get; set; }
  public int Bedrooms { get; set; }
  public int Bathrooms { get; set; }
  public decimal AreaSqM { get; set; }
  public Guid? AgentId { get; set; }            // owner/agent responsible (nullable)
  public Guid? ClosedDealId { get; set; }       // set when sold/rented
  public int ViewsCount { get; set; }           // cached aggregate for fast UI
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public DateTime? DeletedAt { get; set; }
  public bool IsDeleted { get; set; }
  public ICollection<PropertyImage> Images { get; set; }
}

public class PropertyImage {
  public Guid Id { get; set; }
  public Guid PropertyId { get; set; }
  public string FilePath { get; set; }       // storage path or url
  public string FileName { get; set; }
  public bool IsMain { get; set; }
  public int Width { get; set; }
  public int Height { get; set; }
  public long FileSize { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class Inquiry {   // Request / Lead
  public Guid Id { get; set; }
  public Guid? PropertyId { get; set; }      // which property the inquiry is about
  public string VisitorName { get; set; }
  public string VisitorEmail { get; set; }
  public string VisitorPhone { get; set; }   // optional
  public string InitialMessage { get; set; } // the first message (for quick preview)
  public InquiryStatus Status { get; set; }  // enum: New, Assigned, InProgress, Answered, Closed
  public Guid? AssignedAgentId { get; set; } // who is handling it
  public DateTime CreatedAt { get; set; }
  public DateTime? ClosedAt { get; set; }
  public Guid? RelatedDealId { get; set; }   // if leads -> deal
  public bool IsDeleted { get; set; }
  public ICollection<ConversationMessage> Messages { get; set; }
}

public class ConversationMessage {
  public Guid Id { get; set; }
  public Guid InquiryId { get; set; }        // required: message belongs to an inquiry
  public SenderType SenderType { get; set; } // enum: Visitor, Agent, Admin, System
  public Guid? SenderUserId { get; set; }    // null for visitor or system
  public string Body { get; set; }
  public bool IsInternalNote { get; set; }   // private note visible only to staff
  public DateTime CreatedAt { get; set; }
}

public class Deal {   // Transaction
  public Guid Id { get; set; }
  public Guid PropertyId { get; set; }
  public Guid AgentId { get; set; }          // who closed the deal
  public decimal SalePrice { get; set; }     // final price
  public decimal? CommissionPercent { get; set; }
  public decimal? CommissionAmount { get; set; }
  public string BuyerName { get; set; }      // optionally store buyer info
  public DateTime ClosedAt { get; set; }
  public DealStatus Status { get; set; }     // Pending, Completed, Cancelled
  public DateTime CreatedAt { get; set; }
}

public class PropertyView {  // analytics event (for counting & time-series)
  public Guid Id { get; set; }
  public Guid PropertyId { get; set; }
  public string SessionId { get; set; }      // anonymous session token
  public string IpAddress { get; set; }
  public string UserAgent { get; set; }
  public DateTime ViewedAt { get; set; }
}

public class ContentEntry {
  public Guid Id { get; set; }
  public string Key { get; set; }            // e.g., HomeHeroHtml, AboutHtml
  public string HtmlValue { get; set; }
  public Guid UpdatedByUserId { get; set; }
  public DateTime UpdatedAt { get; set; }
}
```

### Enums

```csharp
public enum PropertyStatus { Draft, Published, UnderOffer, Sold, Rented, Archived }
public enum InquiryStatus { New, Assigned, InProgress, Answered, Closed }
public enum SenderType { Visitor, Agent, Admin, System }
public enum DealStatus { Pending, Completed, Cancelled }
```

### Audit & soft-delete pattern

All entities have `CreatedAt/UpdatedAt` and either `IsDeleted` or `DeletedAt`. Add an `IAuditable` interface and `ISoftDelete`.

Implement triggers/DbContext interceptors to maintain timestamps automatically.

---

## 6 — Database & Index recommendations (short)

* `Property`:

  * Index on `(Status, City, Price)`
  * Index on `AgentId`
* `Inquiry`:

  * Index on `(Status, AssignedAgentId, CreatedAt)`
* `Deal`:

  * Index on `(AgentId, ClosedAt)`
* `PropertyView`:

  * Index on `(PropertyId, ViewedAt)`
* `ContentEntry`: unique index on `Key`
* Use `AsNoTracking()` for read-only queries and projections for lists.

---

## 7 — Validation & invariants (business logic)

* `Property.Price` > 0 when published.
* `Inquiry.VisitorEmail` must be a valid email.
* `Inquiry.ClosedAt` must be set when Status == Closed; if RelatedDealId assigned, ensure Deal exists.
* Only `Admin` can set `Property.Status=Published` unless Agent has publishing rights.
* When Deal created: validate Property.Status != Sold and update Property.ClosedDealId and Property.Status → Sold atomically in a transaction.

Enforce via Application layer services / commands and domain exceptions.

---

## 8 — How this supports the important stats (mapping)

* ConversionRate(agent) = `COUNT(Deal WHERE AgentId = X AND ClosedAt BETWEEN) / COUNT(Inquiry WHERE AssignedAgentId = X AND CreatedAt BETWEEN)`
* AvgTimeToClose = `AVG(Deal.ClosedAt - Inquiry.CreatedAt)` join Deal→Inquiry (Inquiry.RelatedDealId)
* Views per property and time series = `SELECT ViewedAt, COUNT(*) FROM PropertyView WHERE PropertyId = X GROUP BY DATE(ViewedAt)`
* SLA: `SELECT * FROM Inquiry WHERE Status != Answered AND CreatedAt < NOW() - INTERVAL '24 hours'`
* Revenue by agent = `SUM(Deal.SalePrice * CommissionPercent/100)`

We designed the schema so all these are straightforward, indexable, and audit-friendly.

---

## 9 — Little extras to prevent “oopsies”

* Keep `Inquiry.InitialMessage` for fast list preview; store full thread in messages.
* Keep `Property.ViewsCount` as a denormalized cached integer updated periodically (increment in DB or via worker) to avoid heavy aggregation on UI list pages.
* Use `RelatedDealId` on Inquiry so you can trace which lead became a deal.
* Keep `ClosedByUserId` on Deal if you need human auditing.
