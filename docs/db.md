# phase 1: user,login session

### User

* **id**: bigint — primary key, unique user ID
* **idNumber**: string — student ID like STU12345 or employee ID like EMP12345
* **fullName**: string — full name
* **email**: string — email address
* **phone**: string — phone number ,use for to contact that user
* **Gender**: string — gender
* **role**: enum('student','instructor','admin','superAdmin') — user role
* **isActive**: boolean — account active status
* **createdAt**: timestamp — record creation time
* **updatedAt**: timestamp — last update time

### userSession

* **id**: bigint — primary key
* **userId**: bigint — linked user
* **otpCode**: string — OTP sent to user
* **otpExpireAt**: timestamp — OTP expiration time
* **deviceInfo**: string — device details
* **ipAddress**: string — user’s IP
* **lastSeen**: timestamp — last activity time
* **isLoggedIn**: boolean — current session login state
* **expiredAt**: timestamp — session expiration time
* **createdAt**: timestamp — session creation time


# phase 2: topics,LabMachines
topics
- id
- title
- description
- createdAt
- updatedAt

LabMachines
- id	UUID / BigInt	Unique machine/lab ID
- labCode	string	Which lab this machine belongs to
- machineName	String	Optional: e.g., “Lab1-PC3”
- deviceId: genrated by using [FingerprintJS] from default browser
- status	Enum	['active','inactive','blocked']
- createdAt	Timestamp	When machine was registered
- updatedAt	Timestamp	Last update / activity

# phase 3: exam & exam topics
exam

| id              | bigint, PK          | Unique exam ID                      |
| title           | string              | Exam title                          |
| description     | text                | Exam description                    |
| durationMinutes | integer             | Exam duration in minutes            |
| startTime       | timestamp, nullable | Set by admin when starting exam     |
| userId          | bigint              | Exam coordinator ID                 |
| isActive     | boolean             | Whether exam is visible to students |
| batch     | number             |  |
| createdAt       | timestamp           | Record creation time                |
| updatedAt       | timestamp           | Last record update time             |

exam topics
- id
- examId: 
- topicId: 
- totalQuestions: amount questions the instrctor to assigned to bring arround that topics
- userId: id of the instructor who assigned to the exam
- deadline: to submite the question
- createdAt
- updatedAt
=> unquire key will be [examId + topicId]


# phase 4: questions & choices
dashboard/exams/{id}/questions => create
dashboard/exams/{id}/questions/{id}/choice/add => add choices

Question

| Column       | Type / Notes | Description                     |
| ------------ | ------------ | ------------------------------- |
| id           | bigint, PK   | Unique question ID              |
| examId       | bigint       | Linked exam ID                  |
examTopicsId
| content | text         | Question content                |
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
