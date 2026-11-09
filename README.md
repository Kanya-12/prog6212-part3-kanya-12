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


