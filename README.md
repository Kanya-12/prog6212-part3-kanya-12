# prog6212-part3-kanya-12
POE

Project Overview

RaceDay is a full-stack event management system designed for the South African road running, walking and cycling community. The system allows Event Organisers to create and manage sporting events, while Participants can browse events, enrol in categories and view their results.

The project is developed in three parts for PROG6212 (Programming 2B).

# RaceDay

##

## Project Overview

RaceDay is a full-stack event management system designed for the South African road running, walking and cycling community. The system allows Event Organisers to create and manage sporting events, while Participants can browse events, enrol in categories and view their results.

The project is developed in three parts for PROG6212 (Programming 2B).

| Part   | Focus                                                                                    | Status             |
| ------ | ---------------------------------------------------------------------------------------- | ------------------ |
| Part 1 | System planning, ERD, API endpoint plan and SQL database                                 | Complete           |
| Part 2 | RESTful API using ASP.NET Core, Entity Framework Core, authentication, testing and CI/CD | Complete           |
| Part 3 | MVC web application, Azure Blob Storage and Docker                                       | Current submission |


## User Roles

Organiser

An Organiser can:

Register and log in as an Organiser.

Create, view, edit and delete their own events.

Manage event categories and routes.

View enrolments for their events.

Update enrolment statuses.

Capture and correct participants' finishing times and positions.

View and update their own profile.

Organisers cannot manage events belonging to other Organisers or enrol as Participants.

Participant

A Participant can:

Register and log in as a Participant.

Browse available events.

View event categories and routes.

Enrol in an event by selecting a category.

View and cancel their own enrolments.

View their own results and performance history.

View and update their own profile.

Participants cannot create or manage events, categories, routes or results.


## Solution Structure

The solution contains the following main projects and folders:

# RaceDay

## Project Overview

RaceDay is a full-stack event management system designed for the South African road running, walking and cycling community. The system allows Event Organisers to create and manage sporting events, while Participants can browse events, enrol in categories and view their results.

The project is developed in three parts for PROG6212 (Programming 2B).

| Part   | Focus                                                                                    | Status             |
| ------ | ---------------------------------------------------------------------------------------- | ------------------ |
| Part 1 | System planning, ERD, API endpoint plan and SQL database                                 | Complete           |
| Part 2 | RESTful API using ASP.NET Core, Entity Framework Core, authentication, testing and CI/CD | Complete           |
| Part 3 | MVC web application, Azure Blob Storage and Docker                                       | Current submission |

## User Roles

### Organiser

An Organiser can:

- Register and log in as an Organiser.
- Create, view, edit and delete their own events.
- Manage event categories and routes.
- View enrolments for their events.
- Update enrolment statuses.
- Capture and correct participants' finishing times and positions.
- View and update their own profile.

Organisers cannot manage events belonging to other Organisers or enrol as Participants.

### Participant

A Participant can:

- Register and log in as a Participant.
- Browse available events.
- View event categories and routes.
- Enrol in an event by selecting a category.
- View and cancel their own enrolments.
- View their own results and performance history.
- View and update their own profile.

Participants cannot create or manage events, categories, routes or results.

## Solution Structure

The solution contains the following main projects and folders:

```text
RaceDay/
├── RaceDay.sln
├── docs/
│   ├── RaceDay_ERD.png
│   ├── RaceDay_ERD.pdf
│   ├── RaceDay_API_Endpoint_Plan.md
│   ├── RaceDay_API_Endpoint_Plan.docx
│   └── RaceDay_Database.sql
├── RaceDay.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── Filters/
│   ├── Migrations/
│   ├── Models/
│   ├── Properties/
│   ├── appsettings.json
│   └── Program.cs
├── RaceDay.Contracts/
│   └── Dtos.cs
├── RaceDay.Api.Tests/
├── RaceDay.Mvc/
├── .github/
│   └── workflows/
└── README.md
```

Main projects:

RaceDay.Api: ASP.NET Core RESTful Web API.

RaceDay.Mvc: ASP.NET Core MVC web application.

RaceDay.Contracts: Shared request and response DTOs.

RaceDay.Api.Tests: Automated API unit tests.

docs: Part 1 planning and database documents.

.github/workflows: GitHub Actions CI/CD workflows.


## Part 1 - Planning and Database

Part 1 focused on planning and designing the RaceDay system before implementation.

The planning documents are available in the /docs folder.

| Deliverable | File |
|---|---|
| Entity Relationship Diagram | [RaceDay_ERD.pdf](https://github.com/RCNMBCampus/prog6212-part-1-Kanya-12/blob/ce7c92df92390fc9564417d4c0ef1fcf9eb84456/docs/RaceDay_ERD.pdf) |
| API Endpoint Plan | [RaceDay_API_Endpoint_Plan.pdf](https://github.com/RCNMBCampus/prog6212-part-1-Kanya-12/blob/ce7c92df92390fc9564417d4c0ef1fcf9eb84456/docs/RaceDay_API_Endpoint_Plan.pdf) |
| SQL Database Script | [RaceDay_Database.sql](https://github.com/RCNMBCampus/prog6212-part-1-Kanya-12/blob/ce7c92df92390fc9564417d4c0ef1fcf9eb84456/docs/RaceDay_Database.sql) |

The database has six entities: User, Event, Category, Route, Enrolment, and Result. Enrolment is the associative entity resolving the many-to-many relationship between Participants and Events via a chosen Category. Result has a 1:0..1 relationship with Enrolment because a result only exists once the organiser captures it.

The Part 1 workflow (.github/workflows/part1-ci.yml) checks that the /docs folder and required files exist.

## Part 1 CI screenshot:

<img width="813" height="54" alt="image" src="https://github.com/user-attachments/assets/06b55ae6-450c-409b-bdab-0b66e0aae5cb" />

## Successful build screenshot:

<img width="898" height="54" alt="image" src="https://github.com/user-attachments/assets/dcaeff56-6205-4a48-85fe-27f0fff2af01" />

## Part 1 video (unlisted):
- https://youtu.be/DlgdIhYUSgA

## Part 2

*Technologies Used:*
- ASP.NET Core 8 Web API (C#), built with Visual Studio 2022
- Entity Framework Core 8, Code-First, with SQL Server
- Server-side sessions for authentication (AddSession)
- ASP.NET Core PasswordHasher for password hashing
- Swashbuckle (Swagger UI) with XML documentation comments
- xUnit, Microsoft.AspNetCore.Mvc.Testing and the EF Core in-memory provider for tests
- GitHub Actions for CI/CD

## Project Structure

```text
RaceDay/
├── RaceDay.sln
├── docs/                         # Part 1 planning documents
├── RaceDay.Api/                  # ASP.NET Core Web API
│   ├── Controllers/              # Auth, Profile, Events, Categories,
│   │                             # Routes, Enrolments, Results
│   ├── Data/                     # RaceDayDbContext
│   ├── Filters/                  # RequireRoleAttribute (role enforcement)
│   ├── Migrations/               # Entity Framework Core migrations
│   ├── Models/                   # AppUser, Event, Category, RaceRoute,
│   │                             # Enrolment, Result, Role
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   └── Program.cs
├── RaceDay.Contracts/            # Shared request and response DTOs
│   └── Dtos.cs
├── RaceDay.Api.Tests/            # xUnit automated tests
├── .github/
│   └── workflows/
│       ├── part1-ci.yml
│       └── part2-ci.yml
└── README.md
```

## Database Setup:
1. You need a SQL Server instance. LocalDB is installed with Visual Studio; SQL Server Express also works.
2. Open RaceDay.Api/appsettings.json and set the Server value in DefaultConnection to your own instance. It must keep the doubled backslash inside the JSON file.
3. Create the database from the committed migrations using Visual Studio or the terminal with `dotnet ef database update --project RaceDay.Api`.
4. Open SQL Server Management Studio and expand RaceDayDb → Tables to confirm the Users, Events, Categories, Routes, Enrolments, Results, and __EFMigrationsHistory tables exist.

The API does not seed any data. Register accounts through Swagger as described below.

The docs/RaceDay_Database.sql script from Part 1 creates a separate database called RaceDay; the Part 2 database is RaceDayDb and is generated from EF Core migrations.
