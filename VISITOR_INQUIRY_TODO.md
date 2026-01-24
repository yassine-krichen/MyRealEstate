# Visitor Inquiry System - Implementation Checklist

**Start Date**: January 24, 2026  
**Status**: In Progress

---

## Phase 1: Backend Foundation ⏳

### 1.1 Update CreateInquiryCommand Handler

- [ ] Modify CreateInquiryCommandHandler to generate 32-char access token
- [ ] Use cryptographically secure random generator
- [ ] Set AccessToken on new inquiry entity before saving
- **Location**: `src/MyRealEstate.Application/Commands/Inquiries/CreateInquiryCommandHandler.cs`

### 1.2 Create GetInquiryByTokenQuery

- [ ] Create query record: `GetInquiryByTokenQuery.cs`
- [ ] Create query handler: `GetInquiryByTokenQueryHandler.cs`
- [ ] Return DTO with all inquiry details + messages
- [ ] Add validation for token format
- **Location**: `src/MyRealEstate.Application/Queries/Inquiries/`

### 1.3 Update Database Seeding

- [ ] Add access token generation for existing inquiries in seed data
- [ ] Ensure all seeded inquiries have unique tokens
- **Location**: `src/MyRealEstate.Infrastructure/Data/Seed/DatabaseSeeder.cs`

---

## Phase 2: Public Controllers 🎯

### 2.1 Create Public PropertiesController

- [ ] Create `Controllers/PropertiesController.cs` (NOT in Areas)
- [ ] `Index` action - List all PUBLISHED properties only
- [ ] `Details(Guid id)` action - Show property details with inquiry form
- [ ] Filter: Status == Published
- [ ] No [Authorize] attribute (public access)
- **Location**: `src/MyRealEstate.Web/Controllers/PropertiesController.cs`

### 2.2 Create Public InquiriesController

- [ ] Create `Controllers/InquiriesController.cs` (NOT in Areas)
- [ ] `Create` POST action - Create inquiry with token generation
- [ ] `Track(string token)` GET action - View inquiry by token
- [ ] `AddMessage` POST action - Visitor reply
- [ ] `MarkAnswered` POST action - Mark as answered
- [ ] `Close` POST action - Close inquiry
- [ ] No [Authorize] attribute (public access)
- [ ] Validate token in each action
- **Location**: `src/MyRealEstate.Web/Controllers/InquiriesController.cs`

---

## Phase 3: ViewModels 📋

### 3.1 Public Property ViewModels

- [ ] `PublicPropertyListViewModel` - For property listing
- [ ] `PublicPropertyDetailViewModel` - For property details + inquiry form
- [ ] `CreateInquiryViewModel` - Form for creating inquiry
- **Location**: `src/MyRealEstate.Web/Models/`

### 3.2 Public Inquiry ViewModels

- [ ] `InquiryTrackingViewModel` - For viewing inquiry status
- [ ] `InquiryCreatedViewModel` - Success page with tracking link
- [ ] `VisitorReplyViewModel` - For visitor replies
- **Location**: `src/MyRealEstate.Web/Models/`

---

## Phase 4: Views 🎨

### 4.1 Properties Views

- [ ] Create `/Views/Properties/Index.cshtml` - Property listing grid
- [ ] Create `/Views/Properties/Details.cshtml` - Property detail + inquiry form
- [ ] Responsive design with Bootstrap 5
- [ ] Display property images in carousel/gallery
- **Location**: `src/MyRealEstate.Web/Views/Properties/`

### 4.2 Inquiries Views

- [ ] Create `/Views/Inquiries/Track.cshtml` - Track inquiry conversation
- [ ] Create `/Views/Inquiries/Created.cshtml` - Success page with token link
- [ ] Show conversation thread (visitor + agent messages)
- [ ] Reply form for visitor
- [ ] Mark as answered / Close buttons
- **Location**: `src/MyRealEstate.Web/Views/Inquiries/`

### 4.3 Shared Components

- [ ] Update main navigation to include "Properties" link
- [ ] Add inquiry form partial: `_InquiryForm.cshtml`
- [ ] Add conversation thread partial: `_ConversationThread.cshtml`
- **Location**: `src/MyRealEstate.Web/Views/Shared/`

---

## Phase 5: Testing & Validation ✅

### 5.1 Unit Testing

- [ ] Test access token generation (32 chars, unique)
- [ ] Test GetInquiryByTokenQuery
- [ ] Test visitor authorization (can only access own inquiry via token)

### 5.2 Integration Testing

- [ ] Browse published properties as visitor
- [ ] Create inquiry from property page
- [ ] Create general inquiry (no property)
- [ ] Receive tracking link
- [ ] View inquiry status via tracking link
- [ ] Reply to agent message as visitor
- [ ] Mark inquiry as answered
- [ ] Close inquiry
- [ ] Test security: Cannot access inquiry without correct token
- [ ] Test security: Cannot access admin endpoints

### 5.3 Edge Cases

- [ ] Invalid token format
- [ ] Non-existent token
- [ ] Closed inquiry - no further actions allowed
- [ ] Empty/whitespace messages
- [ ] XSS prevention in messages
- [ ] SQL injection prevention

---

## Phase 6: Documentation 📝

- [ ] Update README with visitor flow
- [ ] Document API endpoints
- [ ] Add screenshots/demo
- [ ] Security considerations document

---

## Notes & Decisions

**Security Model**:

- Access token = 32-character cryptographically secure random string
- No authentication required for visitors
- Token acts as secure key (similar to password reset links)
- Tokens are permanent (don't expire) but inquiry can be closed

**URL Structure**:

- Public Properties: `/Properties` and `/Properties/Details/{id}`
- Track Inquiry: `/Inquiries/Track?token=ABC123...`
- Admin continues to use: `/Admin/Properties` and `/Admin/Inquiries`

**Visitor Capabilities**:

- ✅ View published properties
- ✅ Create inquiries (general or property-specific)
- ✅ Track their inquiry via token
- ✅ Reply to agent messages
- ✅ Mark as answered
- ✅ Close inquiry
- ❌ Cannot assign agents
- ❌ Cannot view other inquiries
- ❌ Cannot access admin panel

---

**Current Task**: Phase 1.2 - Create GetInquiryByTokenQuery
**Last Updated**: January 24, 2026
