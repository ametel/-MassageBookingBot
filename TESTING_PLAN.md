# Comprehensive Testing Plan for Massage Booking Bot System

## Test Environment Setup Status

### ✅ Successfully Running:

- **API Server**: Running on http://localhost:5000
  - Database automatically created with SQLite
  - Sample data seeded (5 services, 56 time slots)
  - Swagger UI available at http://localhost:5000/swagger
  - Quartz.NET scheduler initialized

### ⚠️ Pending:

- **Bot Worker**: Requires Telegram Bot Token configuration
- **Admin Panel**: Requires Node.js v20+ (currently v18.12.1)

---

## 1. API ENDPOINT TESTING

### 1.1 Services Endpoints

#### GET /api/services

**Test Case 1.1.1**: Retrieve all services

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/services" -Method GET
```

**Expected Result**:

- Status: 200 OK
- Response contains 5 services:
  - Swedish Massage ($80, 60 min)
  - Deep Tissue Massage ($100, 75 min)
  - Hot Stone Massage ($120, 90 min)
  - Sports Massage ($90, 60 min)
  - Aromatherapy Massage ($110, 75 min)

**Test Case 1.1.2**: Filter active services only

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/services?activeOnly=true" -Method GET
```

**Expected Result**:

- Status: 200 OK
- All returned services have `isActive = true`

### 1.2 Bookings Endpoints

#### POST /api/bookings

**Test Case 1.2.1**: Create a new booking

```powershell
$body = @{
    userId = 1
    serviceId = 1
    bookingDateTime = "2025-12-01T10:00:00Z"
    notes = "First time client"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/api/bookings" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body
```

**Expected Result**:

- Status: 201 Created
- Response contains booking details
- `confirmationSent = false` initially
- Google Calendar event ID populated (if configured)

**Test Case 1.2.2**: Validation - Missing required fields

```powershell
$body = @{
    serviceId = 1
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/api/bookings" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body
```

**Expected Result**:

- Status: 400 Bad Request
- Error message indicating required fields

#### GET /api/bookings

**Test Case 1.2.3**: Retrieve all bookings

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/bookings" -Method GET
```

**Expected Result**:

- Status: 200 OK
- Array of booking objects

**Test Case 1.2.4**: Filter bookings by user

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/bookings?userId=1" -Method GET
```

**Expected Result**:

- Status: 200 OK
- Only bookings for userId=1

**Test Case 1.2.5**: Filter bookings by date range

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/bookings?fromDate=2025-12-01&toDate=2025-12-31" -Method GET
```

**Expected Result**:

- Status: 200 OK
- Only bookings within date range

#### DELETE /api/bookings/{id}

**Test Case 1.2.6**: Cancel a booking

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/bookings/1" -Method DELETE
```

**Expected Result**:

- Status: 204 No Content
- Booking status changed to "Cancelled"
- Time slot becomes available again

**Test Case 1.2.7**: Cancel non-existent booking

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/bookings/99999" -Method DELETE
```

**Expected Result**:

- Status: 404 Not Found

---

## 2. DATABASE TESTING

### 2.1 Verify Database Schema

**Test Steps**:

1. Locate `massagebooking.db` in the API project directory
2. Open with SQLite browser or connect via PowerShell

```powershell
# View database location
Get-ChildItem -Path "K:\SmallProjects\BOT\-MassageBookingBot" -Filter "massagebooking.db" -Recurse
```

**Expected Tables**:

- Services
- Users
- Bookings
- TimeSlots
- UserStates

### 2.2 Verify Seeded Data

**Test Query** (using SQL):

```sql
SELECT COUNT(*) FROM Services; -- Expected: 5
SELECT COUNT(*) FROM TimeSlots; -- Expected: 56
SELECT * FROM Services WHERE IsActive = 1; -- Expected: 5 records
```

### 2.3 Test Database Constraints

**Test Case 2.3.1**: Unique ReferralCode constraint

- Attempt to create two users with same referral code
- Expected: Database constraint violation

**Test Case 2.3.2**: Foreign key relationships

- Attempt to create booking with invalid serviceId
- Expected: Foreign key constraint violation

---

## 3. TELEGRAM BOT TESTING

### 3.1 Prerequisites

**Setup Required**:

1. Get Telegram Bot Token from @BotFather
2. Update `appsettings.json` in both API and BotWorker projects:

```json
{
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE"
  }
}
```

3. Restart both API and BotWorker services

### 3.2 Bot Commands Testing

#### Test Case 3.2.1: /start Command

**Steps**:

1. Open Telegram and search for your bot
2. Send `/start` command

**Expected Result**:

- Welcome message displayed
- User automatically registered in database
- Unique referral code generated and displayed
- User record created in `Users` table

**Verification**:

```sql
SELECT * FROM Users WHERE TelegramUserId = <your_telegram_id>;
```

#### Test Case 3.2.2: /book Command - Complete Flow

**Steps**:

1. Send `/book` command
2. Bot displays available services with prices
3. Select a service (e.g., "Swedish Massage")
4. Bot displays available dates
5. Select a date
6. Bot displays available time slots
7. Select a time slot
8. Bot asks for confirmation
9. Confirm the booking

**Expected Result**:

- Each step shows appropriate inline keyboard
- State persists between messages
- Final confirmation message received
- Booking created in database
- Time slot marked as booked
- Confirmation notification sent

**Verification**:

```sql
SELECT * FROM Bookings ORDER BY CreatedAt DESC LIMIT 1;
SELECT * FROM TimeSlots WHERE IsBooked = 1;
SELECT * FROM UserStates WHERE UserId = <user_id>;
```

#### Test Case 3.2.3: /mybookings Command

**Steps**:

1. Create at least one booking (using /book)
2. Send `/mybookings` command

**Expected Result**:

- List of all active bookings displayed
- Each booking shows service, date, time
- Option to cancel bookings (if implemented)

#### Test Case 3.2.4: /help Command

**Steps**:

1. Send `/help` command

**Expected Result**:

- Help message with list of available commands
- Brief description of bot functionality

### 3.3 Finite State Machine (FSM) Testing

#### Test Case 3.3.1: State Persistence

**Steps**:

1. Start booking flow with `/book`
2. Select a service
3. Close Telegram app
4. Reopen Telegram
5. Send any message

**Expected Result**:

- Bot remembers where user left off
- Continues from last state (date selection)

#### Test Case 3.3.2: State Reset

**Steps**:

1. Start booking flow with `/book`
2. Send `/start` command mid-flow

**Expected Result**:

- State resets
- User returns to main menu

### 3.4 Referral System Testing

#### Test Case 3.4.1: Referral Code Generation

**Steps**:

1. Register 3 different users via `/start`

**Expected Result**:

- Each user gets unique referral code
- Codes are 6-8 characters alphanumeric

**Verification**:

```sql
SELECT ReferralCode, COUNT(*) as count
FROM Users
GROUP BY ReferralCode
HAVING count > 1;
-- Expected: No results (all unique)
```

#### Test Case 3.4.2: Referral Tracking

**Steps**:

1. User A sends `/start` and receives referral code (e.g., "ABC123")
2. User B sends `/start` with referral code: `/start ABC123`
3. Check database

**Expected Result**:

- User A's `ReferralCount` increments by 1
- User B's `ReferredByCode` = "ABC123"
- User A's `DiscountBalance` increases

**Verification**:

```sql
SELECT ReferralCode, ReferralCount, DiscountBalance FROM Users WHERE ReferralCode = 'ABC123';
SELECT ReferredByCode FROM Users WHERE TelegramUserId = <user_b_id>;
```

---

## 4. NOTIFICATION SYSTEM TESTING

### 4.1 Quartz.NET Scheduler Verification

#### Test Case 4.1.1: Check Scheduler Status

**Verification Steps**:

1. Check API logs for Quartz.NET initialization
2. Look for: "Scheduler QuartzScheduler\_$_NON_CLUSTERED started"

**Expected Result**:

- Scheduler initialized on API startup
- Job scheduled to run every 30 minutes
- RAMJobStore configured

### 4.2 Booking Reminders Testing

#### Test Case 4.2.1: Immediate Confirmation

**Prerequisites**: Bot Worker running with valid token

**Steps**:

1. Create a booking via bot or API
2. Monitor Telegram for confirmation message

**Expected Result**:

- Confirmation message sent within seconds
- Message includes: service name, date, time
- Database field `ConfirmationSent` = true

#### Test Case 4.2.2: 24-Hour Reminder

**Steps**:

1. Create booking for exactly 24 hours from now
2. Wait for scheduler to run (or manually trigger job)
3. Check Telegram

**Expected Result**:

- Reminder message received
- Database field `Reminder24hSent` = true
- Message says "reminder 24 hours before"

#### Test Case 4.2.3: 2-Hour Reminder

**Steps**:

1. Create booking for exactly 2 hours from now
2. Wait for scheduler to run
3. Check Telegram

**Expected Result**:

- Reminder message received
- Database field `Reminder2hSent` = true
- Message says "reminder 2 hours before"

#### Test Case 4.2.4: No Duplicate Notifications

**Steps**:

1. Create booking
2. Let scheduler run multiple times
3. Count notifications received

**Expected Result**:

- Each notification type sent only once
- Flags prevent duplicate sends

**Verification**:

```sql
SELECT
    ConfirmationSent,
    Reminder24hSent,
    Reminder2hSent
FROM Bookings
WHERE Id = <booking_id>;
```

---

## 5. ADMIN PANEL TESTING

### 5.1 Prerequisites

**Setup Required**:

- Upgrade Node.js to v20.19+ or v22.12+
- Restart terminal/IDE after Node.js upgrade

**Installation**:

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\admin-panel
npm install
npm start
```

**Expected**: Development server starts on http://localhost:4200

### 5.2 Dashboard Testing

#### Test Case 5.2.1: Dashboard Metrics

**Steps**:

1. Navigate to http://localhost:4200
2. View dashboard page

**Expected Result**:

- Total bookings count displayed
- Total services count = 5
- Total clients count
- Today's appointments count

**Verification**:

- Metrics match database counts
- No console errors in browser developer tools

### 5.3 Bookings Management

#### Test Case 5.3.1: View All Bookings

**Steps**:

1. Navigate to Bookings page
2. View bookings table

**Expected Result**:

- All bookings displayed in table format
- Columns: ID, User, Service, Date/Time, Status
- Data loads without errors

#### Test Case 5.3.2: Filter Bookings

**Steps**:

1. Use date filter to select date range
2. Use search to find specific booking

**Expected Result**:

- Table updates with filtered results
- Only matching bookings shown

#### Test Case 5.3.3: Cancel Booking

**Steps**:

1. Click "Cancel" button on a booking
2. Confirm cancellation

**Expected Result**:

- Booking status changes to "Cancelled"
- Table updates immediately
- Success message displayed
- API DELETE request sent successfully

### 5.4 Services Management

#### Test Case 5.4.1: View Services

**Steps**:

1. Navigate to Services page
2. View services list

**Expected Result**:

- All 5 services displayed
- Service details: name, description, price, duration
- Active/inactive status shown

### 5.5 API Integration Testing

#### Test Case 5.5.1: CORS Configuration

**Steps**:

1. Open browser developer tools
2. Navigate admin panel pages
3. Check Network tab

**Expected Result**:

- All API requests succeed
- No CORS errors
- Proper headers in responses

#### Test Case 5.5.2: Error Handling

**Steps**:

1. Stop API server
2. Try to load bookings in admin panel

**Expected Result**:

- User-friendly error message displayed
- No application crash
- Option to retry

---

## 6. GOOGLE CALENDAR INTEGRATION TESTING

### 6.1 Mock Service Testing (Current Implementation)

#### Test Case 6.1.1: Event Creation

**Steps**:

1. Create booking via API or bot
2. Check logs for calendar service call

**Expected Result**:

- Mock service called successfully
- Fake event ID generated (format: "MOCK*EVENT*\*")
- Event ID stored in booking record

### 6.2 Real Google Calendar API Testing (Future)

**Prerequisites** (for real implementation):

1. Google Cloud project created
2. Calendar API enabled
3. OAuth credentials downloaded as `credentials.json`
4. File placed in API project root

#### Test Case 6.2.1: Authentication

**Expected**: Service authenticates with Google successfully

#### Test Case 6.2.2: Create Event

**Expected**: Event appears in Google Calendar

#### Test Case 6.2.3: Update Event

**Expected**: Changes reflected in calendar

#### Test Case 6.2.4: Delete Event

**Expected**: Event removed from calendar

---

## 7. SECURITY TESTING

### 7.1 JWT Authentication

#### Test Case 7.1.1: Access Protected Endpoints

**Steps**:

```powershell
# Without token
Invoke-WebRequest -Uri "http://localhost:5000/api/admin/settings" -Method GET
```

**Expected Result**:

- Status: 401 Unauthorized

**Steps**:

```powershell
# With valid token
$token = "YOUR_JWT_TOKEN"
Invoke-WebRequest -Uri "http://localhost:5000/api/admin/settings" `
    -Method GET `
    -Headers @{"Authorization" = "Bearer $token"}
```

**Expected Result**:

- Status: 200 OK (if endpoint exists)

### 7.2 Input Validation

#### Test Case 7.2.1: SQL Injection Attempt

```powershell
$body = @{
    userId = "1'; DROP TABLE Users;--"
    serviceId = 1
    bookingDateTime = "2025-12-01T10:00:00Z"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/api/bookings" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body
```

**Expected Result**:

- Request rejected or properly sanitized
- Database tables intact
- EF Core parameterized queries prevent injection

#### Test Case 7.2.2: XSS Attempt

```powershell
$body = @{
    userId = 1
    serviceId = 1
    bookingDateTime = "2025-12-01T10:00:00Z"
    notes = "<script>alert('XSS')</script>"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/api/bookings" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body
```

**Expected Result**:

- Script tags escaped or sanitized
- Admin panel displays notes safely

---

## 8. PERFORMANCE TESTING

### 8.1 Load Testing

#### Test Case 8.1.1: Concurrent API Requests

**Tool**: Use PowerShell or tool like Apache JMeter

```powershell
# Simulate 100 concurrent requests
1..100 | ForEach-Object -Parallel {
    Invoke-WebRequest -Uri "http://localhost:5000/api/services" -Method GET
} -ThrottleLimit 100
```

**Expected Result**:

- All requests complete successfully
- Reasonable response times (<1 second)
- No database connection errors

#### Test Case 8.1.2: Database Query Performance

**Test Queries**:

```sql
EXPLAIN QUERY PLAN SELECT * FROM Bookings WHERE UserId = 1;
EXPLAIN QUERY PLAN SELECT * FROM TimeSlots WHERE StartTime >= '2025-12-01' AND IsAvailable = 1;
```

**Expected Result**:

- Indexes being used effectively
- No full table scans on large tables

### 8.2 Bot Response Time

#### Test Case 8.2.1: Command Response Time

**Steps**:

1. Send `/start` command
2. Measure time to receive response

**Expected Result**:

- Response within 1-2 seconds
- No timeout errors

---

## 9. INTEGRATION TESTING

### 9.1 End-to-End Booking Flow

#### Test Case 9.1.1: Complete User Journey

**Steps**:

1. User registers via Telegram bot (`/start`)
2. User books appointment via bot (`/book`)
3. Admin views booking in admin panel
4. System sends 24h reminder
5. System sends 2h reminder
6. Admin cancels booking via admin panel
7. User receives cancellation notification

**Expected Result**:

- All steps complete successfully
- Data consistent across all components
- All notifications sent
- Calendar events created/deleted

### 9.2 Multi-User Scenarios

#### Test Case 9.2.1: Concurrent Booking Attempts

**Steps**:

1. Two users try to book same time slot simultaneously
2. Check database

**Expected Result**:

- Only one booking succeeds
- Second user gets error or slot shown as unavailable
- No double-booking in database

---

## 10. ERROR HANDLING & RECOVERY

### 10.1 API Error Scenarios

#### Test Case 10.1.1: Database Connection Lost

**Steps**:

1. Delete or lock database file
2. Make API request

**Expected Result**:

- Proper error message returned
- API doesn't crash
- Error logged

#### Test Case 10.1.2: Invalid JSON

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/bookings" `
    -Method POST `
    -ContentType "application/json" `
    -Body "{ invalid json }"
```

**Expected Result**:

- Status: 400 Bad Request
- Clear error message about JSON format

### 10.2 Bot Error Scenarios

#### Test Case 10.2.1: Bot Token Revoked

**Steps**:

1. Revoke bot token in BotFather
2. Observe Bot Worker behavior

**Expected Result**:

- Error logged
- Worker doesn't crash
- Graceful error handling

#### Test Case 10.2.2: Network Issues

**Steps**:

1. Disconnect internet
2. Try to send bot command

**Expected Result**:

- Bot Worker retries
- Error logged
- Recovers when connection restored

---

## 11. DATA INTEGRITY TESTING

### 11.1 Referential Integrity

#### Test Case 11.1.1: Cascade Deletes

**Steps**:

1. Create user with bookings
2. Delete user from database

**Expected Result**:

- Related bookings also deleted (or handled appropriately)
- No orphaned records

### 11.2 Data Consistency

#### Test Case 11.2.1: Time Slot Booking Status

**Verification Query**:

```sql
-- Find time slots marked as booked but with no booking reference
SELECT * FROM TimeSlots
WHERE IsBooked = 1 AND BookingId IS NULL;

-- Expected: No results
```

#### Test Case 11.2.2: Booking Status Consistency

**Verification Query**:

```sql
-- Find confirmed bookings without slots
SELECT b.* FROM Bookings b
LEFT JOIN TimeSlots t ON b.Id = t.BookingId
WHERE b.Status = 'Confirmed' AND t.Id IS NULL;

-- Expected: No results
```

---

## 12. DOCUMENTATION TESTING

### 12.1 API Documentation

#### Test Case 12.1.1: Swagger UI Functionality

**Steps**:

1. Navigate to http://localhost:5000/swagger
2. Try "Try it out" feature on GET /api/services
3. Execute request

**Expected Result**:

- Swagger UI loads correctly
- All endpoints documented
- Example requests/responses shown
- "Try it out" executes successfully

### 12.2 README Accuracy

#### Test Case 12.2.1: Setup Instructions

**Steps**:

1. Follow README.md step-by-step on fresh machine/environment
2. Note any discrepancies

**Expected Result**:

- Instructions are accurate and complete
- All prerequisites mentioned
- Commands work as documented

---

## TEST EXECUTION CHECKLIST

### Priority 1: Critical Tests (Must Pass)

- [ ] API builds and runs successfully
- [ ] Database creates and seeds data
- [ ] GET /api/services returns 5 services
- [ ] POST /api/bookings creates booking
- [ ] Bot responds to /start command
- [ ] Bot completes full booking flow
- [ ] Notifications sent (at least confirmation)

### Priority 2: Important Tests (Should Pass)

- [ ] Admin panel loads and displays data
- [ ] Bookings can be cancelled via API
- [ ] Referral system tracks referrals
- [ ] FSM maintains state between messages
- [ ] All 3 notification types working
- [ ] CORS configured correctly
- [ ] Input validation working

### Priority 3: Nice-to-Have Tests (May Pass)

- [ ] Google Calendar integration (if configured)
- [ ] Load testing passes
- [ ] JWT authentication enforced
- [ ] Performance benchmarks met
- [ ] Security tests pass

---

## KNOWN ISSUES & LIMITATIONS

### Current Environment:

1. **Node.js Version**: v18.12.1 installed, but admin panel requires v20.19+
   - **Resolution**: Upgrade Node.js to run admin panel
2. **Telegram Bot Token**: Not configured

   - **Resolution**: Create bot via @BotFather and update appsettings.json

3. **Google Calendar**: Using mock implementation
   - **Resolution**: Configure real API credentials for production

### Testing Environment Notes:

- SQLite suitable for testing, consider PostgreSQL for production
- Quartz.NET using in-memory job store (jobs lost on restart)
- Development HTTPS redirection warning (can be ignored in dev)

---

## AUTOMATED TEST SUITE (Future Enhancement)

### Unit Tests

```csharp
// Example test structure
[Fact]
public async Task CreateBooking_ValidData_ReturnsCreatedBooking()
{
    // Arrange
    var bookingDto = new CreateBookingDto { /* ... */ };

    // Act
    var result = await _bookingsController.CreateBooking(bookingDto);

    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    Assert.NotNull(createdResult.Value);
}
```

### Integration Tests

- Use TestServer for API testing
- Mock Telegram Bot client
- In-memory database for tests

### E2E Tests

- Selenium for admin panel UI testing
- Telegram Bot API testing with test bot

---

## REPORTING

### Test Report Template

```
Test Execution Date: [DATE]
Tester: [NAME]
Environment: Development/Production

Summary:
- Total Tests: X
- Passed: X
- Failed: X
- Blocked: X

Critical Issues:
1. [Description]
2. [Description]

Notes:
[Additional observations]
```

### Success Criteria

- ✅ All Priority 1 tests pass
- ✅ 90%+ of Priority 2 tests pass
- ✅ No critical security vulnerabilities
- ✅ No data loss scenarios
- ✅ All documented features work as described

---

## QUICK START TESTING GUIDE

### Minimal Test Suite (10 minutes):

1. **Verify API**: `curl http://localhost:5000/api/services`
2. **Check Database**: Confirm massagebooking.db exists with data
3. **Test Swagger**: Open http://localhost:5000/swagger, try one endpoint
4. **Create Booking**: Use Swagger or PowerShell to POST booking
5. **Check Logs**: Verify no errors in console output

### Complete Test Suite (2-4 hours):

- Follow all test cases in sections 1-8
- Document results
- Report any failures

---

_End of Testing Plan_
