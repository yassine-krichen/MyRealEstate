# Phase 3: Inquiry & Messaging System - Complete Documentation

## 📋 Overview

This feature implements a complete **Lead Management System** that allows potential buyers/renters (visitors) to inquire about properties, and enables Admin/Agent users to manage, assign, and respond to these inquiries with threaded conversations.

---

## 🎯 What This Feature Does

### For Visitors (Future Public Form)

- Submit inquiries about specific properties or general real estate needs
- Provide contact information (name, email, phone)
- Write an initial message with their questions

### For Admin/Agents

- **View All Inquiries**: List with filters by status, assigned agent, property, and search
- **Assign Inquiries**: Assign leads to specific agents for follow-up
- **Reply to Inquiries**: Have threaded conversations with inquiry history
- **Track Status**: Automatic status progression (New → Assigned → In Progress → Answered → Closed)
- **Internal Notes**: Add private notes visible only to staff
- **Close Inquiries**: Mark inquiries as resolved

---

## 🏗️ Architecture & Implementation

### 1. **Domain Layer** (Already Existed)

Located in: `src/MyRealEstate.Domain/Entities/`

#### **Inquiry Entity** (`Inquiry.cs`)

Core properties:

- **Visitor Information**: `VisitorName`, `VisitorEmail`, `VisitorPhone`
- **Inquiry Content**: `InitialMessage` (the first message from visitor)
- **Relationships**:
    - `PropertyId` (optional - can be general inquiry)
    - `AssignedAgentId` (which agent is handling this)
    - `Messages` collection (conversation thread)
- **Status Tracking**: `Status` enum (New, Assigned, InProgress, Answered, Closed)
- **Timestamps**: `CreatedAt`, `UpdatedAt` (from BaseEntity)

**Domain Methods** (Business Logic):

```csharp
public void AssignToAgent(Guid agentId)  // Assigns agent and sets status to Assigned
public void StartProgress()               // Moves to InProgress status
public void MarkAsAnswered()             // Moves to Answered status
public void Close()                       // Closes the inquiry
```

#### **ConversationMessage Entity** (`ConversationMessage.cs`)

Core properties:

- **Message Content**: `Body` (the message text)
- **Sender Info**:
    - `SenderType` enum (Visitor, Agent, Admin, System)
    - `SenderUserId` (Guid? - null for visitor messages)
    - `SenderUser` navigation property
- **Message Type**: `IsInternalNote` (true = only visible to staff)
- **Relationships**: `InquiryId` (which inquiry this belongs to)

#### **Enums**

- **InquiryStatus**: `New`, `Assigned`, `InProgress`, `Answered`, `Closed`
- **SenderType**: `Visitor`, `Agent`, `Admin`, `System`

---

### 2. **Application Layer** (NEW - Created in Phase 3)

Located in: `src/MyRealEstate.Application/`

#### **DTOs** (`DTOs/Inquiry/InquiryDto.cs`)

Data transfer objects to move data between layers:

1. **InquiryDto**: List view data (id, visitor name, property title, status, dates)
2. **InquiryDetailDto**: Full inquiry with conversation messages
3. **MessageDto**: Individual message in conversation
4. **CreateInquiryDto**: Data for creating new inquiry

#### **Commands** (Write Operations using MediatR)

##### **CreateInquiryCommand** (`Commands/Inquiries/CreateInquiryCommand.cs`)

- **Purpose**: Create a new inquiry from visitor submission
- **Input**: Visitor name, email, phone, message, optional propertyId
- **Validation**:
    - Visitor name required (2-100 chars)
    - Valid email format
    - Message required (10-2000 chars)
    - Phone optional (max 20 chars)
- **Output**: Returns new inquiry Guid
- **Status**: Sets to `New`

##### **AssignInquiryCommand** (`Commands/Inquiries/AssignInquiryCommand.cs`)

- **Purpose**: Assign inquiry to an agent
- **Input**: InquiryId, AgentId
- **Validation**: Both IDs required
- **Business Logic**: Calls `inquiry.AssignToAgent(agentId)` domain method
- **Status**: Changes to `Assigned`

##### **UpdateInquiryStatusCommand** (`Commands/Inquiries/UpdateInquiryStatusCommand.cs`)

- **Purpose**: Change inquiry status manually
- **Input**: InquiryId, NewStatus
- **Business Logic**: Uses domain methods based on status:
    - `InProgress` → calls `StartProgress()`
    - `Closed` → calls `Close()`
- **Note**: Not all status changes need this (some automatic)

##### **AddMessageCommand** (`Commands/Inquiries/AddMessageCommand.cs`)

- **Purpose**: Add a reply to inquiry conversation
- **Input**: InquiryId, Message text, SenderType, SenderId
- **Validation**: Message required (1-2000 chars)
- **Business Logic**:
    - Creates new `ConversationMessage` entity
    - **Auto-status progression**:
        - If inquiry is `New` and Agent replies → set to `Assigned`
        - If inquiry is `Assigned` and Agent replies → set to `InProgress`
- **Output**: Returns message Guid

#### **Queries** (Read Operations using MediatR)

##### **GetInquiryByIdQuery** (`Queries/Inquiries/GetInquiryByIdQuery.cs`)

- **Purpose**: Get single inquiry with full conversation thread
- **Input**: InquiryId
- **Includes**:
    - Property details (title, etc.)
    - Assigned Agent info (full name)
    - All Messages with sender user info
    - Ordered by CreatedAt
- **Output**: `InquiryDetailDto` or null
- **Calculated Fields**: `RespondedAt` (earliest agent message timestamp)

##### **GetInquiriesQuery** (`Queries/Inquiries/GetInquiriesQuery.cs`)

- **Purpose**: Get paginated, filtered list of inquiries
- **Filters**:
    - `Status`: Filter by inquiry status
    - `AssignedToId`: Show only inquiries for specific agent
    - `PropertyId`: Show inquiries for specific property
    - `SearchTerm`: Search in visitor name, email, or initial message
- **Pagination**: `PageNumber`, `PageSize` (default 10 per page)
- **Sorting**: By CreatedAt descending (newest first)
- **Output**: `InquiryListResult` with:
    - `Items`: Array of `InquiryDto`
    - `TotalCount`: Total matching records
    - `PageNumber`, `PageSize`: Current page info
    - `TotalPages`: Calculated total pages

#### **Validators** (`Validators/InquiryValidators.cs`)

Uses FluentValidation for input validation:

1. **CreateInquiryCommandValidator**: Validates inquiry creation
2. **AssignInquiryCommandValidator**: Validates assignment
3. **AddMessageCommandValidator**: Validates message replies

---

### 3. **Web Layer** (NEW - Created in Phase 3)

Located in: `src/MyRealEstate.Web/Areas/Admin/`

#### **View Models** (`Models/InquiryViewModels.cs`)

View-specific models with data annotations:

1. **InquiryListViewModel**: For index page with pagination
2. **InquiryDetailViewModel**: For details page with full conversation
3. **CreateInquiryViewModel**: For creating new inquiry (future public form)
4. **ReplyInquiryViewModel**: For agent reply form
5. **AssignInquiryViewModel**: For assignment dropdown
6. **InquirySearchViewModel**: For filter/search form

#### **Controller** (`Controllers/InquiriesController.cs`)

Admin-area controller with 5 actions:

**Authorization**: `[Authorize(Roles = "Admin,Agent")]` - Only staff can access

##### **Index Action** (GET `/Admin/Inquiries`)

- **Purpose**: List all inquiries with filters and pagination
- **Query Params**: status, assignedTo, propertyId, search, page
- **Process**:
    1. Sends `GetInquiriesQuery` to MediatR
    2. Gets list of agents for filter dropdown (UserManager)
    3. Populates `InquiryListViewModel`
- **View**: Displays table with filters, status badges, pagination

##### **Details Action** (GET `/Admin/Inquiries/Details/{id}`)

- **Purpose**: Show single inquiry with full conversation
- **Process**:
    1. Sends `GetInquiryByIdQuery` to MediatR
    2. Gets list of agents for assignment dropdown
    3. Populates `InquiryDetailViewModel`
- **View**: Shows:
    - Inquiry header (visitor info, property link)
    - Conversation thread (messages styled by sender type)
    - Reply form
    - Assignment form (if not assigned or for reassignment)
    - Close button

##### **Assign Action** (POST `/Admin/Inquiries/Assign`)

- **Purpose**: Assign inquiry to agent
- **Input**: `AssignInquiryViewModel` (InquiryId, AgentId)
- **Process**:
    1. Validates model
    2. Sends `AssignInquiryCommand` to MediatR
    3. Shows success message
    4. Redirects to Details page
- **UI**: Called from assignment form on Details page

##### **Reply Action** (POST `/Admin/Inquiries/Reply`)

- **Purpose**: Add message to conversation
- **Input**: `ReplyInquiryViewModel` (InquiryId, Message)
- **Process**:
    1. Gets current user (UserManager)
    2. Creates `AddMessageCommand` with:
        - SenderType = Agent
        - SenderId = currentUser.Id (Guid, not string!)
    3. Sends command to MediatR
    4. Shows success message
    5. Redirects to Details page
- **UI**: Called from reply form on Details page

##### **Close Action** (POST `/Admin/Inquiries/Close/{id}`)

- **Purpose**: Close inquiry (mark as resolved)
- **Process**:
    1. Sends `UpdateInquiryStatusCommand` with status = Closed
    2. Shows success message
    3. Redirects to Index
- **UI**: Called from close button on Details page

#### **Views** (`Views/Inquiries/`)

##### **Index.cshtml**

**Features**:

- **Filter Section**:
    - Status dropdown (All, New, Assigned, InProgress, Answered, Closed)
    - Assigned Agent dropdown (All agents + unassigned)
    - Search box (name, email, message)
    - Filter button + Clear button
- **Inquiries Table**:
    - Columns: Visitor, Property, Status, Created Date, Assigned To, Actions
    - Status badges with colors (New=primary, InProgress=warning, etc.)
    - Message count badge
    - "View Details" link
- **Pagination**:
    - Previous/Next buttons
    - Page numbers (active page highlighted)
    - Shows "X-Y of Z inquiries"

**UI Framework**: Bootstrap 5 with custom styling

##### **Details.cshtml**

**Layout**: Three main sections

1. **Inquiry Header** (top card):
    - Visitor info (name, email, phone)
    - Property link (if inquiry is for specific property)
    - Status badge
    - Created date
    - Assignment info

2. **Conversation Thread** (middle section):
    - **Chat-style messages**:
        - Visitor messages: Left-aligned, light gray background
        - Agent messages: Right-aligned, blue background
        - Each message shows:
            - Sender name (or "Visitor")
            - Timestamp
            - Message body
        - Internal notes: Yellow background, marked as "Internal Note"
    - Messages ordered chronologically (oldest first)

3. **Action Forms** (bottom section):
    - **Reply Form** (textarea + Send button)
    - **Assignment Form** (if not assigned):
        - Agent dropdown
        - Assign button
    - **Close Button** (if not already closed)

**UX Details**:

- Forms use AJAX-style POST with redirect
- Success messages via TempData
- Responsive layout (mobile-friendly)

---

## 🗃️ Database Seeding

### Added Seed Data (`DatabaseSeeder.cs`)

#### **Agent Users**

Created 2 test agents:

- **agent1@myrealestate.com** (Ahmed Ben Ali) - Password: `Agent@123456`
- **agent2@myrealestate.com** (Fatma Mansour) - Password: `Agent@123456`

Both added to "Agent" role.

#### **6 Sample Inquiries**

1. **New Inquiry** (Unassigned)
    - From: Mohamed Trabelsi
    - About: Luxury Villa in La Marsa
    - Status: New
    - Created: 2 hours ago
    - No assignment yet

2. **Assigned Inquiry** (No Response Yet)
    - From: Salma Hamdi
    - About: Apartment in Lac 2
    - Status: Assigned
    - Assigned to: Ahmed (agent1)
    - Created: 1 day ago

3. **In Progress Inquiry** (Active Conversation)
    - From: Karim Bouazizi
    - About: House in Sidi Bou Said
    - Status: InProgress
    - Assigned to: Fatma (agent2)
    - Created: 3 days ago
    - **Has 4 messages** (2 agent, 1 internal note, 1 visitor)

4. **Answered Inquiry** (Waiting for Visitor)
    - From: Leila Ben Salem
    - About: Investment inquiry
    - Status: Answered
    - Assigned to: Ahmed (agent1)
    - Created: 5 days ago
    - **Has 2 messages** (1 agent, 1 internal note)

5. **Closed Inquiry** (Completed)
    - From: Youssef Slimani
    - About: Studio pet policy
    - Status: Closed
    - Assigned to: Fatma (agent2)
    - Created: 10 days ago
    - **Has 2 messages** (agent declined, visitor acknowledged)

6. **General Inquiry** (No Property)
    - From: Amira Khaled
    - General search request
    - Status: New
    - No property link
    - Created: 5 hours ago

---

## 🧪 How to Test This Feature

### Step 1: Setup & Database Migration

```powershell
# Drop existing database to apply new seed data
dotnet ef database drop --project src/MyRealEstate.Infrastructure --startup-project src/MyRealEstate.Web

# Apply migrations and run seeder
dotnet run --project src/MyRealEstate.Web
```

The seeder will automatically create:

- Admin user
- 2 Agent users
- Sample properties
- 6 Inquiries with conversation messages

### Step 2: Login Credentials

**Admin Account:**

- Email: `admin@myrealestate.com`
- Password: `Admin@123456`

**Agent Accounts:**

- Email: `agent1@myrealestate.com` (Ahmed Ben Ali)
- Password: `Agent@123456`

- Email: `agent2@myrealestate.com` (Fatma Mansour)
- Password: `Agent@123456`

### Step 3: Test Scenarios

#### ✅ **Test 1: View Inquiry List**

1. Login as Admin or Agent
2. Navigate to `/Admin/Inquiries` or click "Inquiries" in admin menu
3. **Expected**: See list of 6 inquiries with different statuses
4. **Verify**:
    - Status badges show correct colors
    - Created dates are displayed
    - Assigned agents show for inquiries 2-5
    - Message count badges appear (0-4 messages)

#### ✅ **Test 2: Filter by Status**

1. On Inquiries page, select "New" from Status dropdown
2. Click "Filter"
3. **Expected**: See only 2 inquiries (Mohamed and Amira)
4. Try other statuses: Assigned, InProgress, Answered, Closed
5. **Verify**: Each filter shows correct inquiries

#### ✅ **Test 3: Filter by Assigned Agent**

1. Select "Ahmed Ben Ali" from Assigned To dropdown
2. Click "Filter"
3. **Expected**: See inquiries #2 and #4 (Salma and Leila)
4. Try "Fatma Mansour"
5. **Expected**: See inquiries #3 and #5 (Karim and Youssef)
6. Select "Unassigned"
7. **Expected**: See inquiries #1 and #6 (Mohamed and Amira)

#### ✅ **Test 4: Search Functionality**

1. Enter "villa" in search box
2. Click "Filter"
3. **Expected**: See Mohamed's inquiry (mentions villa)
4. Try searching "pet"
5. **Expected**: See Youssef's inquiry
6. Try searching email: "karim.bouazizi"
7. **Expected**: See Karim's inquiry

#### ✅ **Test 5: View Inquiry Details with Conversation**

1. Click "View Details" on inquiry #3 (Karim Bouazizi - InProgress)
2. **Expected**: See:
    - Inquiry header with visitor info
    - Property link (House in Sidi Bou Said)
    - Status badge "In Progress"
    - Assigned to "Fatma Mansour"
    - **4 messages in conversation**:
        - Agent welcome (Fatma, right-aligned, blue)
        - Internal note (yellow background, marked as internal)
        - Visitor response (left-aligned, gray)
        - Agent confirmation (Fatma, right-aligned, blue)
3. **Verify**:
    - Messages are chronologically ordered
    - Internal note only visible to staff (marked clearly)
    - Sender names display correctly

#### ✅ **Test 6: Assign Inquiry**

1. View inquiry #1 (Mohamed - New status)
2. **Expected**: See "Assignment" section with agent dropdown
3. Select "Ahmed Ben Ali" from dropdown
4. Click "Assign" button
5. **Expected**:
    - Success message: "Inquiry assigned successfully"
    - Redirected back to Details page
    - Status changed to "Assigned"
    - "Assigned To" now shows "Ahmed Ben Ali"
6. **Verify in database**:
    - `AssignedAgentId` is set
    - `Status` = Assigned (enum value 1)

#### ✅ **Test 7: Reply to Inquiry (Auto-Status Progression)**

1. Still on inquiry #1 (Mohamed - now Assigned)
2. Scroll to "Reply" section
3. Enter message: "Hi Mohamed, I'd be happy to arrange a viewing. Let me check our schedule."
4. Click "Send Reply"
5. **Expected**:
    - Success message: "Reply sent successfully"
    - Redirected to Details
    - **New message appears** in conversation (right-aligned, blue)
    - **Status automatically changed to "In Progress"** ⭐ (this is the auto-progression)
6. **Verify**:
    - Your message shows your name (Ahmed Ben Ali)
    - Timestamp is current
    - Status badge now shows "In Progress"

#### ✅ **Test 8: Add Multiple Messages**

1. On same inquiry, add another reply: "We have availability on Tuesday at 2 PM."
2. Click "Send Reply"
3. **Expected**: New message appears below previous one
4. **Status remains "In Progress"** (doesn't change on subsequent replies)
5. Add a third message
6. **Verify**: All messages display in chronological order

#### ✅ **Test 9: Internal Notes (Private Staff Notes)**

**Note**: Current implementation doesn't have UI checkbox for internal notes yet. This would be added in future iteration. For now, internal notes are seeded in database and display correctly.

1. View inquiry #3 (Karim - has internal note)
2. **Expected**: See yellow-background message marked "Internal Note: Serious buyer, relocating from France..."
3. **Verify**: Internal note is visually distinct from regular messages

#### ✅ **Test 10: Close Inquiry**

1. View inquiry #2 (Salma - Assigned status)
2. Scroll to bottom
3. Click "Close Inquiry" button
4. **Expected**:
    - Success message: "Inquiry closed successfully"
    - Redirected to Inquiries list
    - Inquiry now shows "Closed" status (gray badge)
5. View Details again
6. **Expected**: Close button is gone (already closed)

#### ✅ **Test 11: Pagination**

**Note**: Need more than 10 inquiries to test pagination. You can:

1. Manually create more inquiries via database
2. Or reduce page size in code temporarily

If you have 20+ inquiries:

1. Go to Inquiries list
2. **Expected**: See "Previous" (disabled), page 1, page 2, "Next" buttons
3. Click "Next" or page 2
4. **Expected**: See next 10 inquiries
5. **Verify**: Page numbers update, URLs have `?page=2`

#### ✅ **Test 12: View General Inquiry (No Property)**

1. View inquiry #6 (Amira - general inquiry)
2. **Expected**:
    - No property link displayed
    - Shows "General Inquiry" or similar indication
    - All other features work normally

#### ✅ **Test 13: Agent Access Control**

1. Logout as Admin
2. Login as `agent1@myrealestate.com`
3. Navigate to Inquiries
4. **Expected**: See all inquiries (not filtered to own)
5. Can assign, reply, close any inquiry
6. **Verify**: Same permissions as Admin

**Note**: If you wanted agents to only see their own inquiries, you'd add filter in `GetInquiriesQuery` handler.

#### ✅ **Test 14: Unauthorized Access**

1. Logout
2. Try to access `/Admin/Inquiries` directly
3. **Expected**: Redirected to login page
4. Login as regular user (if you create one without Admin/Agent role)
5. Try to access inquiries
6. **Expected**: 403 Forbidden or redirected (role authorization)

### Step 4: Edge Cases to Test

#### 🔍 **Edge Case 1: Empty Filters**

1. Apply filters but select "All" for everything
2. **Expected**: Show all inquiries (no filtering)

#### 🔍 **Edge Case 2: No Results**

1. Search for "zzzzzzz" (gibberish)
2. **Expected**: "No inquiries found" message

#### 🔍 **Edge Case 3: Reply with Empty Message**

1. Try to send reply with empty message
2. **Expected**: Validation error (client-side or server-side)

#### 🔍 **Edge Case 4: Assign Without Selecting Agent**

1. Try to assign without selecting agent
2. **Expected**: Validation error

#### 🔍 **Edge Case 5: View Non-Existent Inquiry**

1. Navigate to `/Admin/Inquiries/Details/00000000-0000-0000-0000-000000000000`
2. **Expected**: 404 Not Found or error message

---

## 🔍 What to Look For When Testing

### ✅ Functionality Checks

- [ ] All inquiries display correctly in list
- [ ] Filters work (status, agent, search)
- [ ] Pagination displays and works (if enough data)
- [ ] Details page shows full conversation
- [ ] Assign inquiry updates status and agent
- [ ] Reply adds message and auto-progresses status (New→Assigned→InProgress)
- [ ] Close inquiry works
- [ ] Messages display in correct order (oldest first)
- [ ] Internal notes are visually distinct

### ✅ UI/UX Checks

- [ ] Status badges have correct colors
- [ ] Visitor messages align left with gray background
- [ ] Agent messages align right with blue background
- [ ] Timestamps are formatted nicely
- [ ] Success messages appear after actions
- [ ] Forms validate input (required fields)
- [ ] Buttons are clearly labeled
- [ ] Page is responsive (test on mobile size)

### ✅ Data Integrity Checks

- [ ] No duplicate messages created
- [ ] Status changes persist in database
- [ ] Assignments persist
- [ ] Timestamps are UTC
- [ ] Sender info is correct (agent name shows)

### ✅ Security Checks

- [ ] Non-authenticated users can't access
- [ ] Users without Admin/Agent role can't access
- [ ] No sensitive data exposed in client-side

---

## 🐛 Known Limitations & Future Enhancements

### Current Limitations:

1. **No Public Inquiry Form**: Visitors can't submit inquiries yet (only seeded data)
2. **No Email Notifications**: Agents don't get notified when assigned
3. **No Visitor Reply Channel**: Visitors can't reply to agent messages
4. **No Internal Note Checkbox**: Can't mark replies as internal from UI
5. **No File Attachments**: Can't attach documents/images to messages
6. **No Bulk Actions**: Can't assign/close multiple inquiries at once

### Future Enhancements (Not in Current Scope):

- Public-facing inquiry form on property detail pages
- Email notifications (new inquiry, assignment, reply)
- SMS notifications for visitors
- Real-time updates (SignalR for live conversation)
- File attachment support
- Rich text editor for messages
- Inquiry templates (canned responses)
- Export inquiries to CSV
- Dashboard widget (new inquiries count)
- Search with autocomplete
- Advanced filters (date range, property type)

---

## 📊 Technical Decisions & Why

### 1. **Why MediatR for Commands/Queries?**

- **Separation of Concerns**: Each operation is a separate class
- **Testability**: Easy to unit test handlers in isolation
- **Pipeline Behaviors**: Can add logging, validation, transactions globally
- **CQRS Pattern**: Clear distinction between reads (queries) and writes (commands)

### 2. **Why Auto-Status Progression?**

Instead of making agents manually change status:

- **UX**: Fewer clicks, more intuitive
- **Logic**: Status reflects conversation state automatically
    - New = Just received, no response
    - Assigned = Agent assigned but hasn't replied
    - InProgress = Conversation started (agent replied)
    - Answered = Agent provided full answer
    - Closed = No further action needed

### 3. **Why Domain Methods for Status Changes?**

```csharp
inquiry.AssignToAgent(agentId); // Instead of: inquiry.Status = Assigned
```

- **Business Logic in Domain**: Keeps rules centralized
- **Consistency**: Can't accidentally set invalid state
- **Future Proofing**: Easy to add validation (e.g., can't assign closed inquiry)

### 4. **Why Nullable SenderId?**

`public Guid? SenderId { get; init; }`

- Visitor messages don't have user account → SenderId is null
- Agent messages have SenderId → links to User entity
- **Type Safety**: Forces handling of both cases

### 5. **Why Separate InquiryDto and InquiryDetailDto?**

- **Performance**: List view doesn't need full conversation (lazy loading)
- **Payload Size**: List returns smaller objects
- **Clarity**: Different views need different data shapes

---

## 🏗️ Code Quality Notes

### What Went Well:

✅ Clean separation of concerns (Domain → Application → Web)
✅ Consistent naming conventions
✅ FluentValidation for input validation
✅ Proper use of async/await throughout
✅ Entity Framework includes optimized (no N+1 queries)
✅ Domain methods encapsulate business logic

### What to Improve Later:

⚠️ Add more comprehensive unit tests
⚠️ Add integration tests for query logic
⚠️ Consider specification pattern for complex filtering
⚠️ Add caching for agent dropdown (frequently accessed)
⚠️ Add API endpoints (currently only MVC views)
⚠️ Add audit logging (who changed what when)

---

## 🎓 Learning Points

### For Understanding Clean Architecture:

1. **Domain Layer**: Only entities and business rules (no dependencies)
2. **Application Layer**: Use cases (commands/queries) depend on Domain
3. **Infrastructure Layer**: EF Core, database, depends on Application
4. **Web Layer**: Controllers, views, depends on Application (not Infrastructure directly)

### For Understanding CQRS:

- **Commands**: Change state (Create, Assign, UpdateStatus, AddMessage)
- **Queries**: Read state (GetById, GetList)
- Separate models for reads vs writes (DTOs vs Entities)

### For Understanding MediatR:

```csharp
// Send command
var inquiryId = await _mediator.Send(new CreateInquiryCommand { ... });

// Send query
var inquiry = await _mediator.Send(new GetInquiryByIdQuery(id));
```

- Controller doesn't know about handlers (loose coupling)
- Easy to add middleware (logging, validation, transactions)

---

## 📝 Summary

**Files Created**: 12 new files

- 1 DTO file (4 DTOs)
- 4 Command files with handlers
- 2 Query files with handlers
- 1 Validator file (3 validators)
- 1 ViewModel file (6 view models)
- 1 Controller (5 actions)
- 2 Views (Index, Details)

**Files Modified**: 1 file

- DatabaseSeeder.cs (added agents and inquiries)

**Lines of Code**: ~1,500 lines

**Testing Time Estimate**: 30-45 minutes to test all scenarios

---

## 🚀 Next Steps After Testing

Once you've tested everything:

1. **Document Bugs**: Note any issues you find
2. **Test Edge Cases**: Try to break it (invalid IDs, etc.)
3. **Performance Check**: Add 100+ inquiries and test pagination
4. **Mobile Test**: Check responsive design on phone
5. **Security Review**: Ensure proper authorization
6. **Give Feedback**: What features are missing? What UX could improve?

Then we can move to:

- **Phase 3.5**: Public inquiry form
- **Phase 3.6**: Email notifications
- **Phase 4**: Dashboard enhancements
