# 🧪 Phase 3 Testing Checklist

## Pre-Testing Setup

```powershell
# 1. Drop and recreate database with seed data
dotnet ef database drop --project src/MyRealEstate.Infrastructure --startup-project src/MyRealEstate.Web

# 2. Run application (migrations and seeding run automatically)
dotnet run --project src/MyRealEstate.Web

# 3. Open browser to https://localhost:5001 (or http://localhost:5000)
```

## Login Credentials

- **Admin**: admin@myrealestate.com / Admin@123456
- **Agent 1**: agent1@myrealestate.com / Agent@123456 (Ahmed Ben Ali)
- **Agent 2**: agent2@myrealestate.com / Agent@123456 (Fatma Mansour)

---

## Test Cases

### 1. Basic Navigation

- [ ] Login as Admin
- [ ] Navigate to Inquiries menu
- [ ] See list of 6 inquiries
- [ ] All inquiries display correctly

### 2. Status Filter

- [ ] Filter by "New" status → See 2 inquiries
- [ ] Filter by "Assigned" → See 1 inquiry
- [ ] Filter by "In Progress" → See 1 inquiry
- [ ] Filter by "Answered" → See 1 inquiry
- [ ] Filter by "Closed" → See 1 inquiry
- [ ] Select "All" → See all 6 inquiries

### 3. Agent Filter

- [ ] Filter by "Ahmed Ben Ali" → See 2 inquiries
- [ ] Filter by "Fatma Mansour" → See 2 inquiries
- [ ] Filter by "Unassigned" → See 2 inquiries
- [ ] Clear filters → Back to all inquiries

### 4. Search

- [ ] Search "villa" → Find Mohamed's inquiry
- [ ] Search "pet" → Find Youssef's inquiry
- [ ] Search "karim" → Find Karim's inquiry
- [ ] Search "zzz" → No results message

### 5. View Details

- [ ] Click "View Details" on inquiry #3 (Karim - In Progress)
- [ ] See inquiry header with visitor info
- [ ] See property link (House in Sidi Bou Said)
- [ ] See 4 messages in conversation
- [ ] Messages ordered oldest first
- [ ] Internal note has yellow background
- [ ] Agent messages align right (blue)
- [ ] Visitor messages align left (gray)

### 6. Assign Inquiry

- [ ] View inquiry #1 (Mohamed - New)
- [ ] See assignment dropdown
- [ ] Select "Ahmed Ben Ali"
- [ ] Click "Assign"
- [ ] See success message
- [ ] Status changes to "Assigned"
- [ ] "Assigned To" shows Ahmed

### 7. Reply to Inquiry (Auto Status Change)

- [ ] On inquiry #1 (now Assigned)
- [ ] Enter reply: "Hi Mohamed, happy to help!"
- [ ] Click "Send Reply"
- [ ] See success message
- [ ] New message appears in conversation
- [ ] **Status automatically changes to "In Progress"** ⭐
- [ ] Message shows your name and timestamp

### 8. Multiple Replies

- [ ] Add second reply
- [ ] Status stays "In Progress"
- [ ] Add third reply
- [ ] All messages display in order

### 9. Close Inquiry

- [ ] View inquiry #2 (Salma)
- [ ] Click "Close Inquiry"
- [ ] See success message
- [ ] Redirected to list
- [ ] Status shows "Closed" (gray badge)
- [ ] View details again → Close button gone

### 10. General Inquiry (No Property)

- [ ] View inquiry #6 (Amira)
- [ ] No property link shown
- [ ] Everything else works normally

### 11. Agent Login

- [ ] Logout
- [ ] Login as agent1@myrealestate.com
- [ ] Access Inquiries
- [ ] Can see all inquiries
- [ ] Can assign, reply, close

### 12. Unauthorized Access

- [ ] Logout completely
- [ ] Try to access /Admin/Inquiries directly
- [ ] Redirected to login page

---

## UI/UX Verification

- [ ] Status badges have colors (blue, yellow, green, gray)
- [ ] Message count badges show correct numbers
- [ ] Timestamps display in readable format
- [ ] Forms have proper validation
- [ ] Success messages appear after actions
- [ ] Page is responsive (resize browser)
- [ ] No console errors (F12 developer tools)

---

## Database Verification

After assigning inquiry and replying:

```powershell
# Connect to database and check
# Inquiry #1 should have:
# - AssignedAgentId = [Ahmed's Guid]
# - Status = 2 (InProgress)
# - At least 1 message in ConversationMessages table
```

---

## Edge Cases

- [ ] Try to reply with empty message → Validation error
- [ ] Try invalid inquiry ID in URL → 404 or error
- [ ] Filter with all dropdowns on "All" → Shows everything
- [ ] Very long message (1000+ chars) → Works fine
- [ ] Message with special characters → Displays correctly

---

## Performance Check (Optional)

If you want to test with more data:

1. Manually add 50 inquiries via database
2. Test pagination (should show 10 per page)
3. Test search with many results
4. Check page load speed

---

## ✅ Sign-Off

**Tested By**: ******\_******  
**Date**: ******\_******  
**Status**: ☐ Pass ☐ Fail  
**Issues Found**: ******\_******

---

## 🐛 Bug Report Template

If you find bugs, document them:

**Bug #**: \_\_\_  
**Severity**: ☐ Critical ☐ High ☐ Medium ☐ Low  
**Steps to Reproduce**:

1.
2.
3.

**Expected**: **_  
**Actual**: _**  
**Screenshot**: \_\_\_
