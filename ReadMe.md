# Simple Dating App

A minimal dating app skeleton with Backend: .NET 9 (C#) and Frontend: Angular (TypeScript).
This README only covers what the project is and how to run it locally.

---

## 1. Project Overview
 - Goal: Quick-start a simple, split FE/BE app for learning and experiments.
 - Architecture: Decoupled frontend (Angular) and backend (ASP.NET Core).

### Tech Stack
 - Backend: ASP.NET Core (.NET 9), Controllers, Swagger/postman
 - Frontend: Angular (v18+), TypeScript, HttpClient, Angular Router.

### Project Layout
```
simple_dating_app
├─ API
│  ├─ API.csproj
│  ├─ API.http
│  ├─ appsettings.Development.json
│  ├─ appsettings.json
│  ├─ Controllers
│  │  └─ WeatherForecastController.cs
│  ├─ Program.cs
│  ├─ Properties
│  │  └─ launchSettings.json
│  └─ WeatherForecast.cs
├─ ReadMe.md
└─ simple_dating_app.sln
```

## 2. Prerequisites
 - .NET SDK 9
 - Node.js 22.16.0 LTS + npm

## 3. Run Locally
### 3.1. Backend (.NET 9)
1. Restore, build, and run:
```bash
cd api
dotnet restore
dotnet build
dotnet run
```
2. The API typically listens on one of these ports (machine-dependent):
 - http://localhost:5000 or http://localhost:5188
 - Swagger: http://localhost:<port>/swagger

### 3.2. Frontend (Angular)
1. Install deps and start the dev server:
```bash
cd ../client
npm ci
npm start   # same as: ng serve
```
2. The app runs at:
 - http://localhost:4200
 - Note: Configure the API base URL in your Angular code (e.g., environment.ts) to match the backend port.

## 4. Quick Connectivity Test
1. Open Swagger at http://localhost:<api-port>/swagger and call GET /api/ping.
2. In the Angular app, call the same endpoint via HttpClient and show the response to verify FE ⇄ BE communication.

---
**Project for personal education only!**