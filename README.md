# Fortnite Esports Performance Platform

[![Build and Test CI](https://github.com/RaunakSachdeva2004/Fortnite-Performance-Dashboard/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/RaunakSachdeva2004/Fortnite-Performance-Dashboard/actions)

A complete, professional, deployment-ready **Esports Performance Analytics SaaS Platform** built with **ASP.NET Core 8 MVC**, Entity Framework Core 8, Chart.js, and an AI-Assisted Coaching Engine.

The platform ingests live match telemetry from Fortnite-API.com, calculates 0–100 player performance ratings, maps weak telemetry areas to actionable competitive training drills, and presents multi-temporal analytics for players and coaches.

---

## Key Features

- **Player Dashboard**: High-level KPI cards (Eliminations, Wins, Matches, K/D, Win Rate, Performance Score, Recent Form indicators, Chart.js trend lines).
- **Match Telemetry History**: Dedicated paginated, filterable, and sortable match history table with manual logging capabilities.
- **Deep Analytics Overview**: Multi-temporal trend analysis across 7-day, 30-day, 90-day, and All-Time windows evaluating Combat, Survival, and Consistency metrics.
- **AI-Assisted Coaching Engine**: Context-aware recommendation generator mapping telemetry drivers to actionable aim, building, and rotation drills with severity badges.
- **Transparent Performance Score (0–100)**: Weighted rating engine calculating player skill levels (`Excellent`, `Strong`, `Developing`, `Needs Improvement`).
- **Player Profile Management**: Identity customization, preferred game mode selection, clan/team affiliation, and bio editing.
- **Administrator Console**: Platform telemetry oversight, user account status controls (enable/disable), and supported game mode configuration.
- **Health Check & Production Config**: Ready for containerized deployment with Docker and automated GitHub Actions CI/CD pipelines.

---

## Architecture & Technology Stack

```
Presentation Layer (ASP.NET Core Razor Views, Bootstrap 5, Chart.js, HTML5/CSS3)
        |
Controller Layer (ASP.NET Core MVC Controllers, Action Filters, Model Validation)
        |
Service Layer (PerformanceService, MatchService, GameModeService, RuleBasedRecommendationEngine)
        |
Data Access Layer (Entity Framework Core 8 Code-First, DbContext, Migrations)
        |
Database Engine (SQLite for Dev / SQL Server for Production)
```

- **Framework**: ASP.NET Core 8 MVC (.NET 8.0)
- **Database**: Entity Framework Core 8 (SQLite / SQL Server)
- **Authentication**: ASP.NET Core Identity / Cookie Authentication + Claims RBAC
- **External API Integration**: Typed `HttpClient` calling `Fortnite-API.com` with sliding-window rate limiting
- **Testing**: xUnit with In-Memory EF Core Provider

---

## Quick Start & Local Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### 1. Clone & Build
```bash
git clone https://github.com/RaunakSachdeva2004/Fortnite-Performance-Dashboard.git
cd Fortnite-Performance-Dashboard
dotnet restore
dotnet build
```

### 2. Run Database Migrations & Start App
```bash
dotnet run
```
Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`.

---

## Demo Credentials

The application seeds a default Administrator account and demo player accounts upon initial development boot:

| Role | Email | Password | Epic Username |
|---|---|---|---|
| **Administrator** | `admin@fortnitedashboard.local` | `ChangeMe123!` | Administrator |
| **Player** | `ninja@esports.local` | `Player123!` | `Ninja_Pro` |
| **Player** | `bucey@esports.local` | `Player123!` | `Bucey_FTW` |

*Note: For production deployments, update seed passwords via environment variables.*

---

## Configuration & Environment Variables

Create a `appsettings.json` file or use Environment Variables to configure API keys and DB providers:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=FortniteDashboard.db"
  },
  "DatabaseProvider": "SQLite",
  "FortniteApi": {
    "BaseUrl": "https://fortnite-api.com/",
    "ApiKey": "YOUR_OPTIONAL_FORTNITE_API_KEY"
  }
}
```

### Supported Environment Variables:
- `DatabaseProvider`: Set to `SqlServer` or `SQLite`.
- `ConnectionStrings__DefaultConnection`: Connection string for SQL Server or SQLite.
- `FortniteApi__ApiKey`: Optional API Key for Fortnite-API.com.

---

## Running Tests

Execute the automated xUnit test suite:
```bash
dotnet test FortniteDashboard.sln
```

---

## Docker Deployment

### Build & Run with Docker Compose
```bash
docker-compose up --build
```
Access the containerized app at `http://localhost:8080` and health check at `http://localhost:8080/health`.

---

## License
Distributed under the MIT License. See `LICENSE` for details.
