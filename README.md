# 🚀 PortfolioApp - Modern Developer Portfolio

[![.NET Core](https://img.shields.io/badge/.NET-9.0-512bd4.svg)](https://dotnet.microsoft.com/download)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512bd4.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![SQLite](https://img.shields.io/badge/Database-SQLite-003b57.svg)](https://www.sqlite.org/index.html)

A feature-rich, production-ready portfolio management system built with **Blazor Server** and **Clean Architecture**. This application allows developers to showcase their professional journey, projects, and skills through a beautiful, responsive interface, while providing a robust administrative backend for content management.

---

## ✨ Key Features

### 🌐 Public Portfolio
- **Dynamic Homepage**: Professional hero section with a summary of expertise.
- **Experience Timeline**: Detailed professional history with company roles and achievements.
- **Skill Matrix**: Categorized skills (Languages, Frameworks, Tools) with proficiency levels.
- **Project Showcase**: Filterable project gallery with technical stack details and live/GitHub links.
- **Qualifications**: Educational background and certifications.
- **Contact Form**: Secure messaging system for potential collaborators/employers.
- **CV Access**: Integrated CV view and download functionality.

### 🔐 Admin Dashboard
- **Content Management System (CMS)**: CRUD operations for all portfolio sections.
- **Project Manager**: Upload project images, manage descriptions, and tech stacks.
- **Profile Customization**: Update bio, social links, and contact information in real-time.
- **Message Inbox**: View and manage inquiries received through the contact form.
- **Skill & Experience Editor**: Keep your professional growth updated with ease.
- **Secure Authentication**: Built with ASP.NET Core Identity.

---

## 🛠️ Technology Stack

- **Framework**: .NET 9.0 (ASP.NET Core)
- **Frontend**: Blazor Server, HTML5, Vanilla CSS, Bootstrap 5
- **Data Access**: Entity Framework Core (EF Core)
- **Database**: SQLite (Perfect for portability and low-latency)
- **Architecture**: Clean Architecture (Domain-Driven Design)
- **UI Components**: Blazored.Toast (for notifications), Bootstrap Icons

---

## 🏗️ Project Structure

The solution follows a multi-layered **Clean Architecture** pattern to ensure scalability and maintainability:

- **`PortfolioApp.Domain`**: Core entities, value objects, and domain logic.
- **`PortfolioApp.Application`**: Business logic, interfaces, and service definitions.
- **`PortfolioApp.Infrastructure`**: External concerns like database context, migrations, and service implementations.
- **`PortfolioApp.Web`**: The Blazor Server UI, components, and application entry point.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (or VS Code / JetBrains Rider)

### Installation & Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/Codeit951/PortfolioApp.git
   cd PortfolioApp
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Initialize the Database**:
   The application uses a self-seeding mechanism. On the first run, it will automatically create the SQLite database (`portfolio.db`) and seed initial data.

4. **Run the application**:
   ```bash
   dotnet run --project PortfolioApp.Web
   ```

5. **Access the app**:
   Open your browser and navigate to `https://localhost:5001` (or the port shown in your terminal).

### 🔑 Admin Access
To access the admin dashboard, navigate to `/admin/login`. 

**Default Credentials (from Seeder):**
- **Email**: `admin@codeit.com.ng`
- **Password**: `admin@123`

> [!IMPORTANT]
> For security, please change the default credentials immediately after your first login via the Profile Manager.

---

## 🛡️ Authentication Configuration

### Social Login (Optional)
The project is structured to support **Google** and **GitHub** authentication. To enable them:
1. Register your application in the [Google Cloud Console](https://console.cloud.google.com/) and [GitHub Developer Settings](https://github.com/settings/developers).
2. Add your Client IDs and Secrets to `appsettings.json` or use Secret Manager:
   ```json
   "Authentication": {
     "Google": { "ClientId": "...", "ClientSecret": "..." },
     "GitHub": { "ClientId": "...", "ClientSecret": "..." }
   }
   ```

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

---

## 🤝 Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

Developed with ❤️ by [Peter Anthony](https://github.com/codeit951)