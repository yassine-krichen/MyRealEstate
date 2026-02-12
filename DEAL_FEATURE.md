# 📋 Deal Feature - Implementation Plan

## 🎯 Business Context

**What is a Deal?**
A Deal represents a closed sale in the real estate business. It tracks when an inquiry successfully converts to a property sale, recording all financial details, commission calculations, and updating property/inquiry statuses accordingly.

## 🏗️ Architecture Overview

### Domain Layer (Already Exists)

**Deal Entity** should include:

- `Id` (Guid) - Unique identifier
- `PropertyId` (Guid) - The property being sold
- `InquiryId` (Guid?) - Originating inquiry (optional, can be direct sale)
- `AgentId` (Guid) - The selling agent
- `BuyerName` (string) - Customer name
- `BuyerEmail` (string) - Contact email
- `BuyerPhone` (string?) - Contact phone
- `SalePrice` (Money/Decimal + Currency) - Final sale price
- `CommissionRate` (decimal) - Percentage (e.g., 5.0 for 5%)
- `CommissionAmount` (decimal) - Calculated commission
- `Status` (DealStatus enum) - Pipeline stage
- `Notes` (string?) - Additional notes
- `ClosedAt` (DateTime?) - When deal was finalized
- `CreatedAt`, `UpdatedAt` (IAuditable)

**DealStatus Enum** (check if exists):

- `Draft` = 0 - Just created, not finalized
- `Completed` = 1 - Successfully closed
- `Cancelled` = 2 - Deal fell through

### Relationships:

- `Property` → One-to-One with `Deal` (Property.ClosedDealId)
- `Deal` → Many-to-One with `Property`
- `Deal` → Many-to-One with `User` (Agent)
- `Inquiry` → One-to-One with `Deal` (Inquiry.RelatedDealId)

---

## 📦 Implementation Steps

### **Phase 1: Backend - Commands (CQRS)**

#### 1.1 CreateDealCommand

```
Location: Application/Commands/Deals/CreateDealCommand.cs
Handler: CreateDealCommandHandler.cs
```

**Purpose:** Convert an inquiry into a deal or create a direct sale

**Properties:**

- `PropertyId` (required)
- `InquiryId` (optional - can be null for walk-in sales)
- `AgentId` (required)
- `BuyerName` (required)
- `BuyerEmail` (required)
- `BuyerPhone` (optional)
- `SalePrice` (required)
- `CommissionRate` (required, default to 5%)
- `Notes` (optional)

**Business Logic:**

- Validate property exists and is available (not already sold)
- Calculate commission: `CommissionAmount = SalePrice * (CommissionRate / 100)`
- Update Property: Set `ClosedDealId`, change `Status` to `Sold`
- If InquiryId provided: Update Inquiry `Status` to `Closed`, set `RelatedDealId`
- Set Deal `Status` to `Draft` initially
- Set `CreatedAt`, `UpdatedAt`
- Return Deal ID

#### 1.2 CompleteDealCommand

```
Location: Application/Commands/Deals/CompleteDealCommand.cs
```

**Purpose:** Finalize a deal (mark as completed)

**Properties:**

- `Id` (Deal ID)
- `Notes` (optional final notes)

**Business Logic:**

- Change Status from `Draft` to `Completed`
- Set `ClosedAt` to current timestamp
- Ensure Property status is `Sold`

#### 1.3 CancelDealCommand

```
Location: Application/Commands/Deals/CancelDealCommand.cs
```

**Purpose:** Cancel a deal that fell through

**Properties:**

- `Id` (Deal ID)
- `CancellationReason` (optional)

**Business Logic:**

- Change Status to `Cancelled`
- Update Property: Clear `ClosedDealId`, change `Status` back to `Available`
- If linked Inquiry exists: Update `Status` back to `Active` or `InProgress`
- Append cancellation reason to Notes

#### 1.4 UpdateDealCommand

```
Location: Application/Commands/Deals/UpdateDealCommand.cs
```

**Purpose:** Update deal details (before completion)

**Properties:**

- `Id` (Deal ID)
- `SalePrice`, `CommissionRate`, `BuyerName`, `BuyerEmail`, `BuyerPhone`, `Notes`

**Business Logic:**

- Only allow updates if Status is `Draft`
- Recalculate commission if SalePrice or CommissionRate changed
- Update `UpdatedAt`

---

### **Phase 2: Backend - Queries**

#### 2.1 GetDealByIdQuery

```
Location: Application/Queries/Deals/GetDealByIdQuery.cs
```

**Returns:** `DealDetailDto` with:

- All Deal properties
- Property details (Title, Address, Images)
- Agent details (FullName, Email, Phone)
- Inquiry details (if linked)

#### 2.2 GetAllDealsQuery

```
Location: Application/Queries/Deals/GetAllDealsQuery.cs
```

**Parameters:**

- `Page`, `PageSize` (pagination)
- `AgentId?` (filter by agent)
- `Status?` (filter by status)
- `FromDate?`, `ToDate?` (date range)
- `SearchTerm?` (search buyer name or property title)

**Returns:** Paginated list of `DealListDto`

#### 2.3 GetDealStatisticsQuery

```
Location: Application/Queries/Deals/GetDealStatisticsQuery.cs
```

**Parameters:**

- `AgentId?` (optional, for specific agent)
- `FromDate?`, `ToDate?` (date range, default: last 30 days)

**Returns:** `DealStatisticsDto`:

- `TotalDeals` (count)
- `CompletedDeals` (count)
- `CancelledDeals` (count)
- `TotalRevenue` (sum of sale prices)
- `TotalCommission` (sum of commissions)
- `AverageSalePrice` (decimal)
- `AverageCommission` (decimal)

#### 2.4 GetMonthlyRevenueQuery (Optional - for charts)

**Returns:** List of `{ Month, Year, TotalRevenue, DealCount }`

---

### **Phase 3: Infrastructure Layer**

#### 3.1 Repository (if needed)

```
Location: Infrastructure/Repositories/DealRepository.cs
Interface: Application/Interfaces/IDealRepository.cs
```

Only create if complex queries needed beyond basic EF Core

#### 3.2 EF Core Configuration (if not exists)

```
Location: Infrastructure/Data/Configurations/DealConfiguration.cs
```

- Configure relationships
- Set decimal precision for money fields
- Configure indexes for performance

---

### **Phase 4: Frontend - Admin Controllers**

#### 4.1 DealsController (Admin Area)

```
Location: Areas/Admin/Controllers/DealsController.cs
```

**Actions:**

1. **Index (GET)** - List all deals
    - Pagination, filtering, sorting
    - Show Property, Agent, Buyer, Sale Price, Status, Date
    - Filter by Agent, Status, Date Range

2. **Details (GET)** - View single deal
    - All deal information
    - Property card with image
    - Agent information
    - Linked inquiry (if exists)
    - Timeline/history

3. **Create (GET)** - Show create form
    - Dropdown: Select Property (only Available properties)
    - Dropdown: Select Inquiry (optional, filtered by selected property)
    - Agent (auto-populate from inquiry or select)
    - Buyer details (auto-populate from inquiry or enter)
    - Sale Price, Commission Rate

4. **Create (POST)** - Submit new deal
    - Validation
    - Create deal via Command
    - Redirect to Details

5. **Edit (GET)** - Show edit form (only for Draft status)

6. **Edit (POST)** - Update deal

7. **Complete (POST)** - Finalize deal
    - Confirmation modal
    - Execute CompleteDealCommand

8. **Cancel (POST)** - Cancel deal
    - Cancellation reason modal
    - Execute CancelDealCommand

---

### **Phase 5: Frontend - Views**

#### 5.1 Views/Admin/Deals/Index.cshtml

**Layout:**

- Page header with "Create New Deal" button
- Filter panel (Status, Agent, Date Range)
- Table with columns:
    - Property
    - Buyer
    - Agent
    - Sale Price
    - Commission
    - Status (badge)
    - Closed Date
    - Actions (View, Edit, Complete, Cancel)

#### 5.2 Views/Admin/Deals/Details.cshtml

**Layout:**

- Breadcrumb navigation
- Two-column layout:
    - **Left (col-lg-8):**
        - Deal Information card (Sale Price, Commission, Status, Dates)
        - Buyer Information card
        - Property Details card (with image, link to property)
        - Notes section
    - **Right (col-lg-4):**
        - Agent card
        - Inquiry link (if exists)
        - Actions: Edit, Complete, Cancel buttons
        - Timeline/history

#### 5.3 Views/Admin/Deals/Create.cshtml

**Form Fields:**

- Property dropdown (required)
- Inquiry dropdown (optional, filtered by property)
- Buyer Name (text input or auto-fill from inquiry)
- Buyer Email (text input or auto-fill)
- Buyer Phone (text input or auto-fill)
- Sale Price (number input with currency symbol)
- Commission Rate (number input with %, default 5%)
- Commission Amount (calculated readonly field)
- Notes (textarea)

**JavaScript:**

- Auto-calculate commission when price/rate changes
- Load inquiry details when inquiry selected
- Filter inquiries when property selected

#### 5.4 Edit.cshtml

Same as Create, but:

- Show "Deal #ID" in header
- Pre-fill all fields
- Only allow edit if Status = Draft
- Show warning if trying to change property

---

### **Phase 6: Dashboard Integration**

#### 6.1 Update Dashboard/Index.cshtml

Add new cards:

- **Total Revenue** (last 30 days)
- **Total Deals** (completed count)
- **Average Sale Price**
- **Total Commission Earned**

Add new section:

- **Recent Deals** table (last 5 deals)
    - Property, Buyer, Sale Price, Commission, Date

#### 6.2 Update DashboardController

- Call GetDealStatisticsQuery
- Add to ViewModel

---

### **Phase 7: Navigation & UI Polish**

#### 7.1 Update \_Layout.cshtml

Add "Deals" menu item in admin navigation:

```html
<li class="nav-item">
    <a
        class="nav-link text-dark"
        asp-area="Admin"
        asp-controller="Deals"
        asp-action="Index"
    >
        <i class="bi bi-cash-coin"></i> Deals
    </a>
</li>
```

#### 7.2 Inquiries Integration

In Inquiries/Details view:

- Add "Convert to Deal" button if status allows
- Show linked deal if exists

---

## 🧪 Testing Checklist

1. **Create Deal from Inquiry**
    - ✅ Property status updates to Sold
    - ✅ Inquiry status updates to Closed
    - ✅ Commission calculates correctly

2. **Create Direct Deal** (no inquiry)
    - ✅ Can create deal without inquiry
    - ✅ Property status updates

3. **Complete Deal**
    - ✅ Status changes to Completed
    - ✅ ClosedAt timestamp set

4. **Cancel Deal**
    - ✅ Property reverts to Available
    - ✅ Inquiry reverts if linked
    - ✅ Status changes to Cancelled

5. **Edit Deal**
    - ✅ Only works for Draft status
    - ✅ Commission recalculates

6. **Validation**
    - ✅ Cannot sell already sold property
    - ✅ Sale price must be positive
    - ✅ Commission rate 0-100%

7. **Dashboard**
    - ✅ Statistics display correctly
    - ✅ Recent deals show

---

## 📊 Estimated Implementation Order

1. **Commands** (2-3 hours)
    - CreateDealCommand + Handler
    - CompleteDealCommand + Handler
    - CancelDealCommand + Handler
    - UpdateDealCommand + Handler

2. **Queries** (1-2 hours)
    - GetDealByIdQuery + Handler
    - GetAllDealsQuery + Handler
    - GetDealStatisticsQuery + Handler

3. **DTOs & Interfaces** (30 min)
    - DealDetailDto
    - DealListDto
    - DealStatisticsDto

4. **Controller** (1-2 hours)
    - DealsController with all actions

5. **Views** (2-3 hours)
    - Index, Details, Create, Edit views
    - Modals for Complete/Cancel

6. **Dashboard Integration** (1 hour)
    - Update dashboard with deal stats

7. **Testing & Polish** (1 hour)
    - Test all workflows
    - Fix bugs
    - UI polish

**Total Estimated Time: 8-12 hours**

---

📋 Deal Feature Implementation Checklist
Phase 1: Backend - Commands
1.1 CreateDealCommand + Handler
1.2 CompleteDealCommand + Handler
1.3 CancelDealCommand + Handler
1.4 UpdateDealCommand + Handler
Phase 2: Backend - Queries
2.1 GetDealByIdQuery + Handler + DTO
2.2 GetAllDealsQuery + Handler + DTO
2.3 GetDealStatisticsQuery + Handler + DTO
Phase 3: Infrastructure
3.1 EF Core Configuration (if needed)
3.2 Repository (if needed)
Phase 4: Admin Controller
4.1 DealsController with all actions
Phase 5: Views
5.1 Index view
5.2 Details view
5.3 Create view
5.4 Edit view
Phase 6: Dashboard Integration
6.1 Update Dashboard with Deal statistics
Phase 7: Navigation & Polish
7.1 Update navigation menu
7.2 Test and verify
