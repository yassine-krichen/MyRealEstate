# MyRealEstate

*ASP.NET Core MVC application for real estate agencies: property listings, visitor inquiries with a trackable conversation, deals with commission, and an admin panel.*

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Database](https://img.shields.io/badge/EF%20Core-SQLite-informational)
![License](https://img.shields.io/badge/license-PolyForm%20Strict%201.0.0-lightgrey)

## Why

Small agencies often track listings in one tool, inquiries in a mailbox and deals in a spreadsheet, so nobody can tell which inquiry led to which sale. This project keeps the whole path in one system: a property is published, a visitor sends an inquiry without creating an account, an agent answers it, and a closed deal records the sale price and commission.

## Features

**Public site**
- Browse and search published properties.
- Send an inquiry about a property without registering. The visitor gets a private tracking link (token) to follow the conversation and reply.
- Editable site text (home, about, contact) served from the database.

**Admin and agent panel** (roles `Admin` and `Agent`)
- Property management: create, edit, publish, delete, image upload, main image selection.
- Inquiries: list, filter, assign to an agent, change status (`New`, `Assigned`, `InProgress`, `Answered`, ...), reply to the visitor, and keep internal notes.
- Deals: create, update, complete or cancel; commission amount is computed from a commission percentage; deal statistics.
- Dashboard with most-viewed properties, based on recorded property views.
- Content editor (TinyMCE) for the public site text.

## Architecture

Clean Architecture with four projects. Dependencies point inwards.

```text
src/
  MyRealEstate.Domain/          entities, value objects (Money, Address), enums; no external dependencies
  MyRealEstate.Application/     MediatR commands and queries, FluentValidation validators, DTOs, AutoMapper profiles,
                                interfaces (IFileStorage, IEmailSender, IApplicationDbContext, ...)
  MyRealEstate.Infrastructure/  EF Core (SQLite) context, entity configurations, migrations, ASP.NET Identity,
                                seeding, local file storage, a fake email sender
  MyRealEstate.Web/             MVC controllers and Razor views (public site and Admin area), middleware, Serilog
```

- Commands and queries go through MediatR with validation and logging pipeline behaviours; commands return a `Result` instead of throwing for expected failures.
- Business rules live on the entities (for example `Property.Publish()`, `Deal.CalculateCommission()`).
- Authorization is by role on the Admin area controllers.
- `docs/CONTEXT.md` is a longer architecture write-up written during development.

Stack: .NET 8, ASP.NET Core MVC, EF Core 8 (SQLite), ASP.NET Identity, MediatR, FluentValidation, AutoMapper, Serilog, Bootstrap 5.

## Run

Requirements: .NET 8 SDK.

The first start applies migrations and seeds demo data (users, properties, inquiries, deals). The demo user passwords are not in the repository; provide them as user-secrets (or environment variables `Seed__AdminPassword` and `Seed__AgentPassword`). Passwords must satisfy the Identity policy (8+ characters, upper, lower, digit, symbol).

```bash
dotnet user-secrets set "Seed:AdminPassword" "<choose a password>" --project src/MyRealEstate.Web
dotnet user-secrets set "Seed:AgentPassword" "<choose a password>" --project src/MyRealEstate.Web
# optional, for the rich-text editor in the content admin page
dotnet user-secrets set "TinyMCE:ApiKey" "<your TinyMCE key>" --project src/MyRealEstate.Web

dotnet run --project src/MyRealEstate.Web
```

Seeded accounts: `admin@estateflow.com` (Admin) and `agent1@estateflow.com`, `agent2@estateflow.com` (Agent). The database is a local SQLite file created next to the web project and ignored by git.

## Status

There are no automated tests yet. Emails are not sent: `FakeEmailSender` stands in for a real provider.

## License

**Source-available, not open source.** Published under the
[PolyForm Strict License 1.0.0](LICENSE) so you can read the code, run it, and learn
from it for personal, research, or evaluation use.

You may **not** use it commercially, redistribute it, rebrand it, or publish modified
versions without written permission. Copyright (c) 2026 Yassine Krichen. All rights
not granted by the license are reserved.

For commercial licensing or collaboration: krichenyassine22@gmail.com
