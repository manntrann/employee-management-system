# Employee Management System

Full-stack employee management system built with ASP.NET Core Web API, React, TypeScript, Vite, Entity Framework Core, and SQL Server.

## Features

- JWT authentication with role-based authorization.
- Roles: `Admin`, `Manager`, and `Employee`.
- Employee management with create, update, delete, search, pagination, department assignment, and role assignment.
- Department management with list, create, update, delete, and department detail page.
- Employee self-service profile page for updating personal information and password.
- Leave management:
  - Employees can view leave balance.
  - Employees can submit and cancel pending leave requests.
  - Admin and Manager can create leave requests for employees.
  - Admin and Manager can approve, reject, and cancel leave requests.
  - Rejected requests store and display the reject reason.
- Dashboard summary with employee counts, department statistics, leave status counts, and latest leave requests.
- Forgot password flow with email delivery of a one-time temporary password.
- Remember me flow that persists the login session and remembers the last login email.
- Toast notifications, responsive layout, empty states, and role-aware navigation.

## Project Structure

```text
EmployeeManagement.API/          ASP.NET Core Web API
EmployeeManagement.API.Tests/    xUnit service and integration tests
EmployeeManagement.Client/       React + TypeScript + Vite frontend
EmployeeManagement.sln           Solution file
```

## Tech Stack

- Backend: ASP.NET Core, Entity Framework Core, SQL Server, JWT Bearer authentication.
- Frontend: React, TypeScript, Vite, React Router.
- Tests: xUnit, ASP.NET Core integration testing, EF Core InMemory provider.

## Prerequisites

- .NET SDK 9
- Node.js and npm
- SQL Server or SQL Server Express/LocalDB
- Optional: SMTP account for forgot password email testing

## Backend Setup

Restore and build:

```powershell
dotnet restore EmployeeManagement.sln
dotnet build EmployeeManagement.sln
```

Apply database migrations:

```powershell
dotnet ef database update --project EmployeeManagement.API --startup-project EmployeeManagement.API
```

Run the API:

```powershell
dotnet run --project EmployeeManagement.API
```

Default API URL:

```text
https://localhost:7120
```

Swagger is available in development mode.

## Frontend Setup

Install dependencies:

```powershell
cd EmployeeManagement.Client
npm install
```

Optional API URL override:

```powershell
$env:VITE_API_BASE_URL="https://localhost:7120"
```

Run the frontend:

```powershell
npm run dev
```

Default frontend URL:

```text
http://localhost:5173
```

## Seeded Accounts

Development seeding creates these accounts:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@example.com` | `Admin123!` |
| Manager | `manager@example.com` | `Manager123!` |
| Employee | `employee@example.com` | `Employee123!` |

The seed also creates matching employee records, an `Engineering` department, and leave balances for the seeded employees.

## Forgot Password Email Setup

Forgot password uses SMTP settings from the `Mail` configuration section. Do not commit real SMTP credentials.

`appsettings.json` contains placeholders:

```json
"Mail": {
  "Host": "",
  "Port": 587,
  "EnableSsl": true,
  "Username": "",
  "Password": "",
  "FromEmail": "",
  "FromName": "PeopleHub"
}
```

Use .NET user-secrets for local development:

```powershell
dotnet user-secrets init --project EmployeeManagement.API
dotnet user-secrets set "Mail:Host" "smtp.gmail.com" --project EmployeeManagement.API
dotnet user-secrets set "Mail:Port" "587" --project EmployeeManagement.API
dotnet user-secrets set "Mail:EnableSsl" "true" --project EmployeeManagement.API
dotnet user-secrets set "Mail:Username" "your-email@gmail.com" --project EmployeeManagement.API
dotnet user-secrets set "Mail:Password" "your-app-password" --project EmployeeManagement.API
dotnet user-secrets set "Mail:FromEmail" "your-email@gmail.com" --project EmployeeManagement.API
dotnet user-secrets set "Mail:FromName" "PeopleHub" --project EmployeeManagement.API
```

Check local secrets:

```powershell
dotnet user-secrets list --project EmployeeManagement.API
```

For Gmail, use a Google App Password, not the normal Gmail password. If App Passwords are unavailable, use a test SMTP provider such as Mailtrap or Ethereal Email.

Forgot password behavior:

- The system generates a temporary password.
- The temporary password is stored in `Users.TemporaryPasswordHash`.
- The original `PasswordHash` is not overwritten.
- Login accepts either the main password or the temporary password.
- After a successful login with the temporary password, the temporary password hash is cleared.

## Authentication Notes

- The frontend stores JWT tokens in `localStorage` when Remember me is checked.
- The frontend stores JWT tokens in `sessionStorage` when Remember me is unchecked.
- Remember me also stores only the last login email for convenience.
- Passwords are never stored in browser storage by application code.
- Role and email changes for the current session are refreshed with a new JWT when the profile is updated.

## Common API Endpoints

```text
POST   /api/auth/login
POST   /api/auth/forgot-password

GET    /api/employees
GET    /api/employees/{id}
GET    /api/employees/myprofile
PUT    /api/employees/myprofile
POST   /api/employees
PUT    /api/employees/{id}
DELETE /api/employees/{id}

GET    /api/departments
GET    /api/departments/{id}
POST   /api/departments
PUT    /api/departments/{id}
DELETE /api/departments/{id}

GET    /api/leave-requests
GET    /api/leave-requests/mine
GET    /api/leave-requests/balance
POST   /api/leave-requests
POST   /api/leave-requests/for-employee
POST   /api/leave-requests/{id}/approve
POST   /api/leave-requests/{id}/reject
POST   /api/leave-requests/{id}/cancel
```

## Tests

Run all tests:

```powershell
dotnet test EmployeeManagement.sln --configuration Release --no-restore
```

Frontend checks:

```powershell
cd EmployeeManagement.Client
npm run lint
npm run build
```

## Security Notes

- Do not commit real SMTP passwords, JWT secrets, production connection strings, or `.env` files.
- `UserSecretsId` in the `.csproj` is safe to commit; the actual secrets are stored outside the repository.
- Use environment variables, user-secrets, or deployment secrets for sensitive settings.
