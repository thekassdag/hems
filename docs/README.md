# Holistic Examination Management System (HEMS)

## Actores
Exam Coordinator
• Login
• Create / Update / Delete Exam
• Add Questions and Options for each Question
• Set correct answer(s)
• Publish Exam
• Generate Reports
Student
• Login
• Take Exam
• Navigate Questions
• Flag Questions
• Submit Exam
• View Result

## Functionalities
• Secure authentication & authorization
• Multiple exams
• MCQ questions with variable number of options
• Automatic grading
• Per-student exam attempt tracking
• Flagging questions
• Flexible number of questions per page
• Report generation

## Data Models


<!-- # 1️⃣ Users & Roles

### • User

| Column       | Type / Notes                  | Description                                     |
| ------------ | ----------------------------- | ----------------------------------------------- |
| id           | bigint, primary key           | Unique user ID                                  |
| role         | enum('student', 'instructor','admin','superAdmin')| user role on sys
| username     | string                        | Login username                                  |
| phone        | string                        | Phone number                                    |
| email        | string                        | Email address                                   |
| idNumber     | string                        | Student ID: STU[12345], Employee ID: EMP[12345] |
| fullName     | string                        | Full name                                       |
| Gender       | string                        | User gender                                     |
| isActive     | boolean                       | Whether account is active                       |
| createdAt    | timestamp                     | Record creation time                            |
| updatedAt    | timestamp                     | Last record update time                         | -->

<!-- # userSession
- id
- userId
- otpCode
- deviceInfo
- ipAddress
- lastSeen
- isLoggedIn
- expiredAt
- createdAt -->


<!-- # 4️⃣ Exams & Questions

### • Exam

| Column          | Type / Notes        | Description                         |
| --------------- | ------------------- | ----------------------------------- |
| id              | bigint, PK          | Unique exam ID                      |
| title           | string              | Exam title                          |
| description     | text                | Exam description                    |
| durationMinutes | integer             | Exam duration in minutes            |
| passMarks       | integer (%)         | Minimum marks required to pass      |
| startTime       | timestamp, nullable | Set by admin when starting exam     |
| userId          | bigint              | Exam coordinator ID                 |
| isActive     | boolean             | Whether exam is visible to students |
| batch     | number             |  |
| createdAt       | timestamp           | Record creation time                |
| updatedAt       | timestamp           | Last record update time             | -->

<!-- topics
- id
- title
- description
- createdAt
- updatedAt -->


<!-- exam topics
- id
- examId
- topicId
- assignedQuestionAmount
- assignedInstructor
- deadlineToSubmit
- createdAt
- updatedAt -->


### • Question

| Column       | Type / Notes | Description                     |
| ------------ | ------------ | ------------------------------- |
| id           | bigint, PK   | Unique question ID              |
| examId       | bigint       | Linked exam ID                  |
examTopicsId
| content | text         | Question content                |
| image_url | text         | Question content                |
| marks        | integer      | Marks assigned to this question |
| createdAt    | timestamp    | Record creation time            |
| updatedAt    | timestamp    | Last record update time         |

### • Choice

| Column     | Type / Notes | Description                         |
| ---------- | ------------ | ----------------------------------- |
| id         | bigint, PK   | Unique choice ID                    |
| questionId | bigint       | Linked question ID                  |
| choiceText | text         | Choice content                      |
| isCorrect  | boolean      | Marks if this is the correct answer |
| createdAt  | timestamp    | Record creation time                |
| updatedAt  | timestamp    | Last record update time             |

---

# 5️⃣ Student Answers

### • EnrolledStudent
    - id
    - examId
    - userId
    - status: ['paused','active','disqualified','inactive','pending']
    - statusText: e.g: disqualified for opening another tab
    - createdAt
    - updatedAt


### • StudentAnswer

| Column      | Type / Notes | Description                                 |
| ----------- | ------------ | ------------------------------------------- |
| id          | bigint, PK   | Unique answer ID                            |
| examId      | bigint       | Exam ID                                     |
| userId      | bigint       | Student ID                                  |
| questionId  | bigint       | Question ID                                 |
| choiceId    | bigint       | Selected choice ID                          |
| isFlagged   | boolean      | Marks suspicious answers                    |
| submittedAt | timestamp    | Set when saving the answer                  |
| updatedAt   | timestamp    | Auto-managed by DB                          |

---

# 6️⃣ Exam Results

### • ExamResult

| Column              | Type / Notes | Description                        |
| ------------------- | ------------ | ---------------------------------- |
| id                  | bigint, PK   | Unique result ID                   |
| userId              | bigint       | Student ID                         |
| examId              | bigint       | Exam ID                            |
| totalCorrectAnswers | integer      | Number of correct answers          |
| score               | decimal(5,2) | Final exam score                   |
| status              | enum         | ['disqualified','passed','failed'] |
| createdAt           | timestamp    | Record creation time               |
| updatedAt           | timestamp    | Last record update time            |

---

# 7️⃣ Audit Log

### • ExamActivityLog
    - id
    - examId
    - questionId
    - userId
    - action: const examActions = [
  'created',           // Exam created
  'updated',           // Exam updated
  'deleted',           // Exam deleted
  'published',         // Exam published
  'unpublished',       // Exam unpublished
  'started',           // User started exam
  'ended',             // User finished exam
  'openedTab',         // User switched tab
  'openedDevTool',     // Dev tools opened
  'triedToCopy',       // Copying attempted
  'lostFocus',         // Window lost focus
  'offline',           // User went offline
  'zoomedOut',         // User zoomed out browser
  'screenshotAttempt', // User tried to take screenshot (if detectable)
  'rightClickBlocked', // Right-click attempted
  'pasteAttempt',      // User tried to paste text
  'printAttempt'       // User tried to print page
  'disabledJS',       // JS stopped running
  'tamperDetected',   // Someone modified DOM or blocked scripts
  'ipChanged',        // User switched IP during exam
  'multiSession'      // Second device/browser detected
];
    - metadata JSON / Text (optional) Extra info: e.g., IP, user agent, tab count, etc.
    - createdAt
    - updatedAt



---

<!-- ## LabMachines
- id	UUID / BigInt	Unique machine/lab ID
- labId	UUID / BigInt	Which lab this machine belongs to
- machineName	String	Optional: e.g., “Lab1-PC3”
- ipAddress	String	Static or registered IP of the machine
- status	Enum	['active','inactive','blocked']
- createdAt	Timestamp	When machine was registered
- updatedAt	Timestamp	Last update / activity -->


# packages
Built-in: ASP.NET Core Identity

Comes built-in with ASP.NET Core (Microsoft.AspNetCore.Identity package).

Provides:

User management (UserManager<TUser>)

Role management (RoleManager<TRole>)

Claims & permissions

Password hashing, email confirmation, lockout, 2FA

You can plug it into Entity Framework Core for storing users/roles in a database.

Works with JWT tokens, cookies, or both.

NuGet package (usually already included in ASP.NET Core templates):

Microsoft.AspNetCore.Identity.EntityFrameworkCore


Role-based & Policy-based Authorization

ASP.NET Core has role-based and policy-based authorization built-in, no extra package needed.

Example:
[Authorize(Roles = "Admin,Coordinator")]
public IActionResult CreateExam() { ... }

[Authorize(Policy = "CanCreateExam")]
public IActionResult CreateExam() { ... }


# pages
## User & Role Management Pages
- Users List / Management
- View all users (Admin only)
- Create / Edit / Deactivate users
- Assign roles to users
- Roles List / Management
- View roles
- Create / Edit / Delete roles (Admin only)
- Permissions View (optional)
- View which role has which permissions
- Assign/remove permissions from roles

## Exams & Questions Pages
- Exams List
- View all exams (Admin/Coordinator)
- Create new exam (Coordinator/Admin)
- Publish / Unpublish exam
- Start / End exam
- Exam Details Page
- View exam info, total marks, questions
- Edit exam details (if not published or Admin)
- Questions Management
- Add / Edit / Delete questions for an exam
- Manage choices for each question

## Student Workflow Pages
- Exam Attempt Page
- List of available exams for a student
- Take exam interface (questions + choices)
- Submit answers
- Exam Result Page
- View student’s own results
- Highlight passed/failed/disqualified
- Flagged Answers (for Coordinators/Admins)
- Review flagged answers
- Mark as suspicious / remove flag

## Audit & Reporting Pages
- Audit Log
- List all actions with user, timestamp, description
- Filter by user/action/date
- Reports / Stats (optional)
- Exam performance
- Question difficulty (based on student answers)
- Coordinator performance (average scores for exams they manage)

## Auth & Profile Pages
- Login / Logout
- Password Reset / Forgot Password
- User Profile
- View / Edit own details (phone, email, password)
- Role-based content
- Pages visible based on role/permissions


# folder
/Controllers
  UserController.cs
  RoleController.cs
  ExamController.cs
  QuestionController.cs

/Models
  User.cs
  Role.cs
  Exam.cs
  Question.cs
  Choice.cs
  StudentAnswer.cs
  ExamResult.cs
  AuditLog.cs
  Permission.cs

/Views
    /Auth
        Login.cshtml, 
        ForgotPassword.cshtml, 
        ResetPassword.cshtml

    /User
        Index.cshtml
        Edit.cshtml
        Create.cshtml
    /Exam
        Index.cshtml
        Details.cshtml
        Create.cshtml
    /Question
        Index.cshtml
        Create.cshtml

/Views/Shared
    _Layout.cshtml
    _AdminNav.cshtml

/Data
    AppDbContext.cs

## security
-> we pause the exam if
  - offline for long time
  - opening other tab
    document.addEventListener('visibilitychange', () => {
      if (document.hidden) {
        // User switched tab
        pauseExam();
      }
    });
-> randomize the question order and choices for each user
-> Only allow exam from the registered IP range (or via a secure token tied to device).
  Backend checks the request IP against allowed list.
  Optionally, only allow one session per user to prevent multiple logins.

-> not allowing coping text
  document.addEventListener('copy', e => e.preventDefault());
  document.addEventListener('cut', e => e.preventDefault());
  document.addEventListener('paste', e => e.preventDefault());
-> not allowing disabling js
-> not allowing context menu and blurring on dev tool oping
  document.addEventListener('contextmenu', e => e.preventDefault());
-> tamperDetected => const observer = new MutationObserver(() => {
    logActivity('tamperDetected');
  });

  observer.observe(document.body, {
    childList: true,
    subtree: true,
    attributes: true
  });
  Detect if your JS code gets paused

  If someone opens debugger and pauses scripts → you can detect freeze.

  let last = Date.now();

  setInterval(() => {
    const now = Date.now();
    if (now - last > 2000) {
      logActivity('tamperDetected');
    }
    last = now;
  }, 1000);

  1. Detect if your exam DOM gets modified

  If a user tries to remove elements, hide timers, edit question text, or delete JS hooks, this fires.

  const observer = new MutationObserver(() => {
    logActivity('tamperDetected');
  });

  observer.observe(document.body, {
    childList: true,
    subtree: true,
    attributes: true
  });


  This catches things like:

  removing timers

  editing question text

  hiding elements

  injecting HTML

  2. Detect if your JS code gets paused

  If someone opens debugger and pauses scripts → you can detect freeze.

  let last = Date.now();

  setInterval(() => {
    const now = Date.now();
    if (now - last > 2000) {
      logActivity('tamperDetected');
    }
    last = now;
  }, 1000);


  If time jumps too far, JS was paused.

  3. Detect if console is opened (not 100% foolproof but works)

  This catches most DevTools attempts.

  let devOpen = false;

  setInterval(() => {
    const start = performance.now();
    debugger;  
    const end = performance.now();

    if (end - start > 10 && !devOpen) {
      devOpen = true;
      logActivity('openedDevTool');
      logActivity('tamperDetected');
    }
  }, 500);

  4. Detect removal of your script tags

  If someone deletes <script> elements.

  const scripts = document.querySelectorAll('script');
  scripts.forEach(s => {
    const parent = s.parentElement;
    new MutationObserver(() => {
      if (!parent.contains(s)) {
        logActivity('tamperDetected');
      }
    }).observe(parent, { childList: true });
  });

  5. Detect attempts to disable JS

  If your backend stops receiving exam heartbeats → JS stopped running → tampering.

  setInterval(() => {
    fetch('/exam/heartbeat', { method: 'POST' });
  }, 3000);


  On backend:

  if heartbeat stops for 6–8 seconds → disabledJS

  mark as tamperDetected too

  6. Detect blocked network requests (like someone trying to bypass logging)
  window.addEventListener('error', (e) => {
    if (e.message.includes('Failed to fetch')) {
      logActivity('tamperDetected');
    }
  });

  7. Detect if exam data is being altered in localStorage

  Set traps around storage usage.

  const originalSet = localStorage.setItem;
  localStorage.setItem = function (k, v) {
    logActivity('tamperDetected');
    originalSet.apply(this, arguments);
  };

  8. Detect resizing tricks (users trying to reveal hidden windows)
  window.addEventListener('resize', () => {
    if (window.outerHeight - window.innerHeight > 200) {
      logActivity('tamperDetected');
    }
  });

  This is enough to catch 95% of cheats

  No system is perfect. But these together will detect:

  dev tools

  script pausing

  modifying DOM

  blocking your scripts

  removing elements

  editing questions

  pausing timers

  replacing JS files

  resizing windows to inspect DOM

  trying to bypass logs

## higlevel security
I get exactly what you're trying to build — a **tamper‑proof answer submission system** where the user can’t delete/edit their activity logs to cheat or bypass the workflow.

Here’s the clean, safe, correct flow in human terms:

---

# **✔️ The Secure Flow You Described**

### **1. User clicks “Submit Answer”**

Before saving the answer, you **emit an event** like:

```
answeringAttempt
```

### **2. Backend creates an activity log row**

Example row:

| activityId | examId | questionId | userId | action           |
| ---------- | ------ | ---------- | ------ | ---------------- |
| 12983      | 10     | 42         | 5      | answeringAttempt |

### **3. Backend returns a value to the frontend**

* return only **an encrypted token**
* token contains the `activityId` and maybe a timestamp
- expires withn seconds (They still need the token created 1 second earlier
and they can’t forge it.)

Example (just conceptually):

```
eyJhbGciOi...encryptedActivityToken...F9=
```

Frontend never sees the real ID.
User cannot manipulate it.

---

# **✔️ 4. User submits the actual answer**

POST body example:

```json
{
  "questionId": 42,
  "answer": "C",
  "attemptToken": "eyJhbGciOi..." 
}
```

---

# **✔️ 5. Backend decrypts the token**

Backend now gets back:

* activityId
* examId
* questionId
* userId
* timestamp

This lets the backend confirm:

### **You accept the answer ONLY if:**

1. `activityId` exists
2. action = `"answeringAttempt"`
3. `questionId` matches
4. `userId` matches
5. time difference < allowed window (for anti‑replay)

---

# **✔️ 6. After validation, save the answer**

Then update activity:

```
action = "answered"
```

Or add a new log row for auditing.

---

# **✔️ Result**

This prevents:

### ❌ editing logs

### ❌ deleting logs

### ❌ submitting answers without logging

### ❌ faking frontend requests

### ❌ replaying old tokens

### ❌ multi‑session cheating

### ❌ tampering with requests

Because everything is **server-issued**, **signed**, and **server-validated**.

---

# **🔐 Recommended token contents**

Inside the encrypted/signed token:

```json
{
  "activityId": 12983,
  "userId": 5,
  "questionId": 42,
  "examId": 10,
  "nonce": "random-uuid",
  "iat": 1738243824
}
```

You can sign this using:

* **AES encryption** (symmetric)
* or **JWT (HS256)**
* or **JWE (encrypted JWT)**

JWT with signing works fine since you aren’t exposing sensitive data — just IDs.
