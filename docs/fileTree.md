app/
│
├── Controllers/
│     └── HomeController.cs          // e.g., returns Index view
│
├── Models/
│     └── User.cs                    // e.g., simple model class
│
├── Views/
│     ├── Shared/
│     │     └── _Layout.cshtml       // e.g., main layout
│     │
│     └── Home/
│           └── Index.cshtml         // e.g., homepage view
│
├── wwwroot/
│     ├── css/                       // e.g., site.css
│     ├── js/                        // e.g., site.js
│     └── images/                    // e.g., logo.png
│
├── appsettings.json                 // e.g., connection strings
└── Program.cs                       // minimal hosting + routes