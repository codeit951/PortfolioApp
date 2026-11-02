# 🚀 PortfolioApp - Modern Developer Portfolio

[![.NET](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512bd4.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![SQLite](https://img.shields.io/badge/Database-SQLite-003b57.svg)](https://www.sqlite.org/index.html)

A portfolio site with a built-in admin panel, built with **Blazor Server** and **Clean Architecture**. Visitors see your profile, experience, skills, projects, qualifications and CV; you manage all of it from `/admin`.

---

## ✨ Features

### 🌐 Public site
- **Home**: hero section, stats computed from your content (years of experience, projects, technologies, companies) and featured projects.
- **Experience timeline**, **skill matrix** (by category, with proficiency levels) and **qualifications**.
- **Projects**: paged, case-insensitive search across title, description and tech stack.
- **CV page**: printable, with a PDF download when you've uploaded one.
- **Contact form**: messages go to the admin inbox, with optional email notification. Spam protection includes a honeypot field, a minimum fill time and per-sender and global hourly limits.
- Dark/light mode, page titles and descriptions driven by your profile, and a custom 404 page.

### 🔐 Admin panel (`/admin`)
- Manage the profile (including the profile photo and CV PDF), experience, qualifications, skills and projects.
- **Message inbox**: read/unread state, reply by email, delete.
- **Account & Password**: change your password and see which sign-in providers are linked.
- Works on mobile, with a collapsible menu.
- Uploads are checked on the server by file signature (JPG/PNG/WebP images, PDF for the CV). Replaced or abandoned uploads are deleted automatically.
- Login lockout: 5 failed attempts lock the account for 15 minutes.

---

## 🛠️ Technology Stack

- **.NET 8** (ASP.NET Core, Blazor Server with interactive server rendering)
- **Entity Framework Core 8** + **SQLite**
- **ASP.NET Core Identity** (optional Google / GitHub sign-in)
- Bootstrap 5, Bootstrap Icons, Blazored.Toast
- xUnit tests

## 🏗️ Project Structure

| Project | Purpose |
|---|---|
| `PortfolioApp.Domain` | Entities and their validation rules |
| `PortfolioApp.Application` | Service interfaces |
| `PortfolioApp.Infrastructure` | EF Core context, migrations, seeding, service implementations, SMTP notifier |
| `PortfolioApp.Web` | Blazor UI, auth endpoints, file storage |
| `PortfolioApp.Tests` | Unit and integration tests (in-memory SQLite) |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer

### Run locally

```bash
git clone https://github.com/Codeit951/PortfolioApp.git
cd PortfolioApp
dotnet restore

# Choose the admin account's initial password (stored outside the repo):
dotnet user-secrets --project PortfolioApp.Web set "AdminUser:Password" "Your!StrongPassw0rd"

dotnet run --project PortfolioApp.Web
```

On first run the app creates the SQLite database at `PortfolioApp.Web/App_Data/portfolio.db`, applies migrations and seeds **sample content**. Replace that content from the admin panel.

### 🔑 Admin access

Sign in at `/admin/login` with the email in `AdminUser:Email` (default `admin@codeit.com.ng` in `appsettings.json`) and the password you set.

- The admin account is created **only when no admin exists yet**. Changing the settings later never changes an existing account.
- If `AdminUser:Password` is empty, a random password is generated and printed **once** in the startup log.
- Change your password anytime at **`/admin/account`**.

No credentials are stored in source control. Keep secrets in user-secrets locally and in environment variables in production (for example `AdminUser__Password`).

---

## ⚙️ Configuration

All settings live in `PortfolioApp.Web/appsettings.json`. Each one can be overridden with an environment variable, using `__` in place of `:`.

| Setting | Default | Description |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Data Source=App_Data/portfolio.db` | SQLite file. Relative paths resolve against the web project folder. |
| `AdminUser:Email` / `AdminUser:Password` | see above | First-run admin account. |
| `Storage:UploadsPath` | *(empty → `wwwroot/uploads`)* | Where admin uploads are stored. They are always served at `/uploads`. |
| `DataProtection:KeysPath` | *(empty → default location)* | Persist auth-cookie keys, e.g. in containers. |
| `Smtp:*` | *(empty → disabled)* | Email notification for new contact messages. |
| `Authentication:Google:*`, `Authentication:GitHub:*` | *(empty → disabled)* | Optional social sign-in. |

### 📧 Email notifications (optional)

```bash
dotnet user-secrets --project PortfolioApp.Web set "Smtp:Host" "smtp.example.com"
dotnet user-secrets --project PortfolioApp.Web set "Smtp:UserName" "you@example.com"
dotnet user-secrets --project PortfolioApp.Web set "Smtp:Password" "app-password"
dotnet user-secrets --project PortfolioApp.Web set "Smtp:From" "you@example.com"
# Smtp:To defaults to the email on your profile
```

If sending fails, the message is still saved to the inbox and the error is logged.

### 🛡️ Social sign-in (optional)

Google and GitHub sign-in are enabled only for providers whose `ClientId` and `ClientSecret` are set:

1. Register an OAuth app in the [Google Cloud Console](https://console.cloud.google.com/) and/or [GitHub Developer Settings](https://github.com/settings/developers).
   Callback URLs: `https://<your-host>/signin-google` and `https://<your-host>/signin-github`.
2. Store the credentials:
   ```bash
   dotnet user-secrets --project PortfolioApp.Web set "Authentication:Google:ClientId" "..."
   dotnet user-secrets --project PortfolioApp.Web set "Authentication:Google:ClientSecret" "..."
   ```

External sign-in **never creates accounts**. The first time you sign in with a provider, it is linked to the existing admin account **with the same email**; any other account is rejected.

---

## 🐳 Docker

```bash
ADMIN_EMAIL=you@example.com docker compose up -d --build
docker compose logs portfolio | grep "generated password"   # if you didn't set ADMIN_PASSWORD
```

The site is served on port 8080. The database, uploads and data-protection keys are stored in the `portfolio-data` volume (`/app/data`). Put a reverse proxy in front for HTTPS; the image trusts `X-Forwarded-*` headers.

---

## 🧪 Tests

```bash
dotnet test
```

The tests cover the data services (including repeated edits, search, rate limiting and notification failure), entity validation, upload type detection and admin seeding.

## 🗄️ Database migrations

```bash
dotnet tool restore
dotnet dotnet-ef migrations add <Name> --project PortfolioApp.Infrastructure --startup-project PortfolioApp.Infrastructure --output-dir Data/Migrations
```

Migrations are applied automatically at startup.

---

## 📄 License
This project is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.

## 🤝 Contributing
Contributions are welcome! Fork the project, create a feature branch, commit your changes and open a pull request.

---

Developed with ❤️ by [Peter Anthony](https://github.com/codeit951)
