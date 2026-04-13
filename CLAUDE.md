# CLAUDE.md - BankApp-V2 Project Guide

## Project Overview
This document contains the build, test, and coding standards for the BankApp-V2 project.

**Technology Stack:**
- **Backend:** .NET 10 (C#)
- **Frontend:** Angular 20
- **Database:** PostgreSQL (with Entity Framework Core)
- **Caching:** Redis (StackExchange.Redis) / .NET 9+ Hybrid Cache
- **Storage:** Google Cloud Storage (GCP)
- **Real-Time:** SignalR (for notifications & chat)
- **Background Jobs:** Quartz.NET
- **Authentication:** JWT (JSON Web Tokens) with Refresh Tokens & BCrypt
- **AI Integration:** Google Gemini AI (REST API)
- **Logging:** Serilog (with PostgreSQL sink)
- **Validation:** FluentValidation
- **Mailing:** MailKit / MimeKit
- **Deployment:** Render / Docker

**Architecture:**
- Clean Architecture / Onion Architecture (Backend)
- Multi-Layered design for Backend (`Core`, `DAL`, `BLL`, `API`)
- **Frontend (UI) is isolated:** The `BankApp-UI-V2` directory operates as a completely separate layer and repository, communicating with the Backend strictly via REST APIs and SignalR.

## Build & Run Commands

### Backend (`BankApp-App-V2/IlkProjem.API`)
- **Restore:** `dotnet restore`
- **Build:** `dotnet build`
- **Run:** `dotnet run` (Uses port `5005` locally as defined in `launchSettings.json`)
- **EF Migrations:** `dotnet ef migrations add <Name> --project ../IlkProjem.DAL --startup-project .`
- **EF Update DB:** `dotnet ef database update --project ../IlkProjem.DAL --startup-project .`

### Frontend (`BankApp-UI-V2`)
- **Install Packages:** `npm install`
- **Build:** `npm run build`
- **Run:** `ng serve` (Typically runs on `http://localhost:4200`)

## Environment & Configuration
- **Backend Envs:** Copy `.env.example` to `.env`. Ensure `GCP_CREDENTIALS_JSON`, DB connection strings, and JWT limits are set.
- **Frontend Envs:** Check `environments/environment.ts` for API URLs.
- **Production:** In Render, all settings must be defined in the Environment Variables dashboard.

## Coding Standards

### Backend
- Use **Async/Await** for all I/O operations and keep cancellation tokens (`CancellationToken ct = default`) in signatures.
- Strictly follow **SOLID** principles. Use **Dependency Injection** for new services.
- Prefer **Primary Constructors** (C# 12+ feature) where applicable to reduce boilerplate.
- Use `IDataResult<T>`, `IResult` wrappers for standardizing API responses.
- Validation should be done strictly using `FluentValidation` separated from controllers.
- Prefer `.env` or `appsettings.json` for configurations; **no hardcoded secrets**.

### Frontend
- Exclusively use **Standalone Components** (Angular 14+ / 20 style).
- Leverage **Signals** for state management and reactivity.
- Keep components small and focused. Reuse code via Services and inject them properly.
- All routing and UI logic should prioritize a secure and user-friendly UX/UI, using localization (`TranslateModule`) for bilingual support.

## Interaction Rules
- Always provide a natural language summary before and after using any tools.
- If you cannot find a file, do not guess the path; ask me for the correct path.
- Avoid outputting raw JSON to the user; always wrap tool results in a human-readable explanation.