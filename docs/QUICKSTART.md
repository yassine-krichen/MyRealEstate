# 🚀 Quick Start Guide - Phase 3 Testing

## 1️⃣ Reset Database & Seed Data (1 minute)

```powershell
# Drop existing database
dotnet ef database drop --project src/MyRealEstate.Infrastructure --startup-project src/MyRealEstate.Web

# Run app (auto-migrates and seeds)
dotnet run --project src/MyRealEstate.Web
```

## 2️⃣ Login (30 seconds)

**URL**: https://localhost:5001 (or http://localhost:5000)

**Credentials**:

- Email: `admin@myrealestate.com`
- Password: `Admin@123456`

## 3️⃣ Navigate to Inquiries (10 seconds)

After login → Click **"Inquiries"** in the admin menu

You should see **6 sample inquiries**.

## 4️⃣ Key Things to Test (10 minutes)

### ✅ Test 1: View Conversation (2 min)

1. Click "View Details" on **Karim Bouazizi** (In Progress)
2. You'll see a **chat-style conversation** with 4 messages
3. Notice the **yellow "Internal Note"** (private staff note)

### ✅ Test 2: Assign Inquiry (1 min)

1. Go back to list
2. Click "View Details" on **Mohamed Trabelsi** (New)
3. Select **"Ahmed Ben Ali"** from dropdown
4. Click **"Assign"**
5. ✨ Status changes to "Assigned"

### ✅ Test 3: Reply & Auto-Status (2 min)

1. Still on Mohamed's inquiry
2. Type reply: `"Hi Mohamed, I can show you the villa this Tuesday!"`
3. Click **"Send Reply"**
4. ✨ **Status automatically changes to "In Progress"**
5. Your message appears in the conversation

### ✅ Test 4: Filter Inquiries (1 min)

1. Go back to list
2. Select **"New"** from Status filter
3. Click "Filter"
4. You'll see only unassigned inquiries

### ✅ Test 5: Search (1 min)

1. Clear filters
2. Type **"villa"** in search box
3. Click "Filter"
4. You'll find Mohamed's inquiry about the villa

### ✅ Test 6: Close Inquiry (1 min)

1. View **Salma Hamdi** inquiry (Assigned)
2. Click **"Close Inquiry"** button
3. ✨ Status changes to "Closed"
4. You're redirected to the list

### ✅ Test 7: Agent Login (2 min)

1. Logout
2. Login as **agent1@myrealestate.com** / `Agent@123456`
3. Navigate to Inquiries
4. You can see and manage all inquiries (same as admin)

---

## 🎯 What You Should See

### On Inquiries List:

- **6 inquiries** with different statuses
- **Colored status badges**:
    - 🔵 New (blue)
    - 🟡 In Progress (yellow)
    - 🟢 Answered (green)
    - ⚫ Closed (gray)
- **Message count badges** (0-4 messages)
- **Assigned agent names** for some inquiries

### On Details Page:

- **Inquiry header**: Visitor name, email, phone
- **Property link** (if inquiry is about specific property)
- **Conversation thread**: Chat-style messages
    - Visitor messages: Left, gray background
    - Agent messages: Right, blue background
    - Internal notes: Yellow background, marked "Internal"
- **Reply form**: Textarea to send messages
- **Assignment form**: Dropdown to assign agents
- **Close button**: To mark inquiry as resolved

---

## 📊 Seed Data Summary

**Users**:

- 1 Admin (admin@myrealestate.com)
- 2 Agents (Ahmed, Fatma)

**Inquiries**:

1. **Mohamed** → New, unassigned, about villa
2. **Salma** → Assigned to Ahmed, no replies yet
3. **Karim** → In Progress with Fatma, 4 messages
4. **Leila** → Answered by Ahmed, 2 messages
5. **Youssef** → Closed by Fatma, about pet policy
6. **Amira** → New, general inquiry (no property)

---

## 🎬 Expected Flow Demo

### Typical Inquiry Lifecycle:

```
1. VISITOR submits inquiry
   └─> Status: NEW (blue badge)

2. ADMIN/AGENT assigns to agent
   └─> Status: ASSIGNED (assigned badge, agent name shows)

3. AGENT replies to visitor
   └─> Status: IN PROGRESS (yellow badge)
   └─> Message appears in conversation
   └─> Status changed AUTOMATICALLY! ⭐

4. AGENT/VISITOR exchange more messages
   └─> Status stays: IN PROGRESS

5. AGENT closes when resolved
   └─> Status: CLOSED (gray badge)
```

---

## ❓ Common Questions

**Q: Why doesn't the visitor get a notification?**  
A: Email notifications are Phase 3.5 (future enhancement).

**Q: Can visitors reply to agents?**  
A: Not yet. Public reply form is future feature. For now, simulate in-person/phone conversation.

**Q: Can I mark a reply as "Internal Note"?**  
A: UI checkbox not implemented yet. For now, internal notes are only in seed data.

**Q: Why does status change automatically when agent replies?**  
A: This is by design! It reflects conversation state:

- **New** = No one assigned
- **Assigned** = Agent assigned but hasn't responded
- **In Progress** = Conversation started
- **Answered** = Fully answered
- **Closed** = No further action needed

**Q: Can I reassign an inquiry to different agent?**  
A: Yes! Just use the assignment form again (even if already assigned).

**Q: What if I assign inquiry to wrong agent?**  
A: Just reassign - no problem! Status will adjust accordingly.

---

## 🐛 Known Issues

None currently! Report any bugs you find.

---

## 📁 Documentation Files

1. **PHASE3_INQUIRY_FEATURE.md** → Full technical documentation
2. **TESTING_CHECKLIST.md** → Detailed test cases
3. **QUICKSTART.md** (this file) → Fast testing guide

---

## 🆘 Troubleshooting

**Problem**: Can't login  
**Solution**: Make sure you dropped and recreated database

**Problem**: No inquiries showing  
**Solution**: Check that seeder ran (should see console output on startup)

**Problem**: Error accessing /Admin/Inquiries  
**Solution**: Make sure you're logged in with Admin or Agent role

**Problem**: Changes not saving  
**Solution**: Check browser console (F12) for JavaScript errors

---

## ✅ Testing Complete?

Once you've tested everything:

1. ✅ Mark items in TESTING_CHECKLIST.md
2. 📝 Note any bugs or suggestions
3. 🎉 We can move to next phase!

**Estimated Testing Time**: 15-20 minutes for basic flow, 30-45 minutes for comprehensive testing.

---

Happy Testing! 🚀
