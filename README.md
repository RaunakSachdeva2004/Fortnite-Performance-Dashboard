<div align="center">

<img src="./assets/images/fortnite_ai_coaching.jpg" alt="Fortnite Performance Dashboard Analytics" width="750" style="border-radius: 12px; margin-bottom: 15px;" />

# 🏆 Fortnite Performance Dashboard
### ASP.NET Core 8 MVC Esports Analytics & AI-Assisted Coaching System

[![Framework](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Database](https://img.shields.io/badge/SQL%20Server-2022-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/en-us/sql-server/)
[![ORM](https://img.shields.io/badge/EF%20Core-8.0-68217A?style=for-the-badge&logo=nuget&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![Frontend](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![Visuals](https://img.shields.io/badge/Chart.js-4.4-FF6384?style=for-the-badge&logo=chartdotjs&logoColor=white)](https://www.chartjs.org/)
[![API](https://img.shields.io/badge/API-FortniteAPI.io-0078D4?style=for-the-badge&logo=fortnite&logoColor=white)](https://fortniteapi.io/)

<p align="center">
  <b>An esports analytics web platform engineered with ASP.NET Core MVC. Syncs player match stats via FortniteAPI.io, computes esports KPIs, renders Chart.js trends, and generates rule-based AI coaching recommendations.</b>
</p>

[Key Features](#-key-features) • [MVC Architecture](#-architecture--mvc-design-pattern) • [System Flowcharts](#-system-flowcharts--diagrams) • [Database Schema](#-database-schema--erd) • [AI Coaching Engine](#-ai-assisted-coaching-engine) • [Installation](#-installation--getting-started)

---

</div>

## 📌 Case Study & Overview

In competitive Battle Royale games like **Fortnite**, players generate rich match telemetries—eliminations, survival placement, weapon accuracy, and K/D ratios. However, checking stats across scattered tools obscures historical improvement trends.

The **Fortnite Performance Dashboard** unifies player match data into a clean ASP.NET Core 8 MVC web application. Integrating `FortniteAPI.io`, Entity Framework Core, SQL Server, and Chart.js, the system imports digital match metrics, computes performance KPIs, renders dynamic trend graphs, and executes a rule-based AI coaching engine to highlight actionable areas for improvement.

---

## ✨ Key Features

- **🎮 Fortnite Account Sync**: Link Epic Games usernames to fetch real-time match statistics from `FortniteAPI.io`.
- **📊 Chart.js Analytics**: Visual breakdowns of K/D Ratios, Win Rates, Weapon Accuracy, and Match Volume trends.
- **🤖 Rule-Based AI Coaching**: Automated advice generator evaluating player metrics against competitive thresholds.
- **🛡️ Clean MVC & Service Layer**: Decoupled N-tier architecture keeping controllers lightweight and business logic encapsulated.
- **👥 Role-Based Access Control (RBAC)**: Distinct permissions for **Players** (sync & view stats/coaching) and **Administrators** (manage roster & game mode categories).
- **⚡ Rate-Limit Protection**: Built-in cooldown handling for `FortniteAPI.io` free-tier rate limits (10 requests/min).

---

## 🏗️ Architecture & MVC Design Pattern

The application enforces a **Layered (N-Tier) ASP.NET Core MVC Architecture**, ensuring clear separation of concerns:

```
       +-------------------------------------------------------------+
       |               PRESENTATION LAYER (Razor Views)              |
       |             HTML5 / Bootstrap 5 / Chart.js Data             |
       +------------------------------+------------------------------+
                                      |
                                      v
       +-------------------------------------------------------------+
       |                 CONTROLLER LAYER (Thin MVC)                 |
       |       DashboardController | AccountController | Admin       |
       +------------------------------+------------------------------+
                                      |
                                      v
       +-------------------------------------------------------------+
       |                   BUSINESS SERVICE LAYER                    |
       |  FortniteApiClient  |  StatsService  | RecommendationEngine  |
       +------------------------------+------------------------------+
                                      |
                                      v
       +-------------------------------------------------------------+
       |                  DATA ACCESS LAYER (EF Core)                |
       |               ApplicationDbContext / Entity Models          |
       +------------------------------+------------------------------+
                                      |
                                      v
       +-------------------------------------------------------------+
       |                     DATABASE ENGINE                         |
       |                   Microsoft SQL Server                      |
       +-------------------------------------------------------------+
```

### Applied Design Patterns

| Pattern | Location | Architectural Purpose |
| :--- | :--- | :--- |
| **MVC Pattern** | Web Core | Decouples HTTP request routing, data representation, and UI views. |
| **Service Layer** | `Services/` | Encapsulates third-party API calls and KPI computations outside controllers. |
| **Strategy Pattern** | `IRecommendationEngine.cs` | Allows swapping rule-based coaching for AI/LLM models without modifying DB/Controllers. |
| **Repository Semantics**| EF Core `DbContext` | Native unit-of-work state tracking and LINQ abstractions. |
| **Dependency Injection**| ASP.NET Core Native DI | Registers services with scoped lifecycles for testability and loose coupling. |

---

## 📊 System Flowcharts & Diagrams

### 1. Request & Execution Sequence

```mermaid
sequenceDiagram
    autonumber
    actor Player as 🎮 Player
    participant Ctrl as 🎛️ DashboardController
    participant Service as ⚙️ StatsService
    participant API as 🌐 FortniteAPI.io
    participant AI as 🧠 RecommendationEngine
    participant DB as 💾 SQL Server (EF Core)

    Player->>Ctrl: Click "Sync Stats"
    Ctrl->>Service: SyncAsync(playerId)
    Service->>API: Fetch Player Stats (FortniteUsername)
    API-->>Service: Return JSON Match Data
    Service->>Service: Calculate K/D Ratio & Win Rate %
    Service->>DB: Upsert Stats Record
    Service->>AI: GenerateRecommendations(Stats)
    AI-->>Service: Return Strategy Advice Rules
    Service->>DB: Save Recommendations Record
    DB-->>Ctrl: Update Completed
    Ctrl-->>Player: Render Updated Dashboard with Chart.js
```

### 2. End-to-End System Workflow

```mermaid
flowchart TD
    A([🔑 User Login / Auth]) --> B{Has Linked Fortnite Account?}
    B -- No --> C[📝 Link Epic Games Username]
    C --> D[💾 Save Profile to Players Table]
    B -- Yes --> E[📊 View Player Dashboard]
    D --> E
    E --> F[🔄 Trigger Stat Sync]
    F --> G{Rate Limit Cooldown Active?}
    G -- Yes --> H[⚠️ Display Cooldown Notice]
    G -- No --> I[🌐 Request FortniteAPI.io Telemetry]
    I --> J{API Response Valid?}
    J -- No/Private --> K[❌ Display Error Message]
    J -- Success --> L[📐 Compute K/D Ratio & Win Rate %]
    L --> M[💾 Save Stats in SQL Database]
    M --> N[🧠 Execute Rule-Based AI Engine]
    N --> O[✍️ Insert Recommendations]
    O --> P[📈 Refresh Chart.js Visualizations & Insights]
    P --> E
```

---

## 🗄️ Database Schema & ERD

Designed with strict relational integrity in **Microsoft SQL Server**:

```mermaid
erDiagram
    USERS ||--|| PLAYERS : "has profile"
    PLAYERS ||--|| STATS : "maintains current"
    PLAYERS ||--o{ RECOMMENDATIONS : "receives many"

    USERS {
        int UserId PK
        string Name
        string Email
        string PasswordHash
        string Role "Player | Administrator"
    }

    PLAYERS {
        int PlayerId PK
        int UserId FK
        string FortniteUsername
        string Game
        string Team
    }

    STATS {
        int StatId PK
        int PlayerId FK
        int Eliminations
        int Wins
        float Accuracy
        float KDRatio
        int MatchesPlayed
        datetime LastUpdated
    }

    RECOMMENDATIONS {
        int RecommendationId PK
        int PlayerId FK
        string RecommendationText
        datetime CreatedDate
    }
```

---

## 📈 Performance KPI Mockups

```
+-----------------------------------------------------------------------+
|  K/D RATIO PROGRESSION (Last 5 Syncs)                                 |
|                                                                       |
|  4.0 |                                                     * (3.85)   |
|  3.5 |                                      * (3.40)                |
|  3.0 |                       * (2.95)                                 |
|  2.5 |        * (2.40)                                               |
|  2.0 | * (1.80)                                                       |
|      +------------------------------------------------------------    |
|        Sync #1       Sync #2       Sync #3     Sync #4     Sync #5    |
+-----------------------------------------------------------------------+
|  ACCURACY DELTA & WIN RATE DISTRIBUTION                               |
|  [████████████████████░░░░░░░░░] 68% Shot Accuracy (Assault/DMR)      |
|  [██████████████░░░░░░░░░░░░░░░] 48% Win Rate (Solo / Squads)         |
+-----------------------------------------------------------------------+
```

---

## 🤖 AI-Assisted Coaching Engine

The **RecommendationEngine** evaluates match performance against competitive thresholds:

```csharp
public class RuleBasedRecommendationEngine : IRecommendationEngine 
{
    public IEnumerable<string> GenerateRecommendations(Stats stat)
    {
        var recommendations = new List<string>();

        if (stat.Accuracy < 0.25f) {
            recommendations.Add("🎯 Low Accuracy (<25%): Focus on bloom control and trigger discipline.");
        }
        if (stat.KDRatio < 1.5f) {
            recommendations.Add("⚔️ Low K/D (<1.5): Work on high-ground retakes during mid-game rotations.");
        }
        if (stat.Wins / (float)Math.Max(1, stat.MatchesPlayed) < 0.10f) {
            recommendations.Add("🏆 Win Rate < 10%: Avoid hot dropping; prioritize edge POIs for safer looting.");
        }
        return recommendations;
    }
}
```

---

## 📁 Repository Structure

```
Fortnite-Performance-Dashboard/
├── assets/
│   └── images/                  # Graphic asset for README header
│       └── fortnite_ai_coaching.jpg
├── Controllers/
│   ├── AccountController.cs     # Auth, Registration, Roles
│   ├── AdminController.cs       # Roster management & admin stats
│   └── DashboardController.cs   # Dashboard rendering & sync trigger
├── Data/
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   └── Migrations/              # Database migration scripts
├── Models/
│   ├── User.cs                  # User entity & credentials
│   ├── Player.cs                # Linked Fortnite profile model
│   ├── Stats.cs                 # Match stats & computed KPIs
│   └── Recommendation.cs       # AI recommendation entity
├── Services/
│   ├── IFortniteApiClient.cs    # FortniteAPI.io client interface
│   ├── FortniteApiClient.cs     # API client implementation
│   ├── IStatsService.cs         # Stat processing interface
│   ├── StatsService.cs          # Core business logic & database updates
│   └── RecommendationEngine.cs  # Rule-based AI coaching engine
├── Views/
│   ├── Dashboard/               # Razor Views with Chart.js
│   ├── Admin/                   # Admin panel views
│   └── Shared/                  # Navigation & layout templates
├── appsettings.json             # API keys & SQL connection strings
├── Program.cs                   # Middleware & Dependency Injection setup
└── README.md                    # Project documentation
```

---

## ⚙️ Quick Start

```bash
# 1. Clone repository
git clone https://github.com/RaunakSachdeva2004/Fortnite-Performance-Dashboard.git
cd Fortnite-Performance-Dashboard

# 2. Update appsettings.json with your SQL Connection String & FortniteAPI.io Key

# 3. Apply migrations
dotnet ef database update

# 4. Run application
dotnet run
```

---

<div align="center">

Developed with ❤️ by **Group 15** for ASP.NET Core MVC Capstone.

</div>
