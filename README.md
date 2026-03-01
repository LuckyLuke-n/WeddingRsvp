# WeddingRsvp Solution

A modern, cloud-native Wedding RSVP management system built with **.NET 10**, **Aspire**, and **Blazor**. This solution leverages a microservices-friendly architecture to manage guest responses and dynamic wedding information.

## 🚀 Features
Click to expand for more details.
<details>
  <summary>Guest management to track RSVPs, dietary requirements, and guest counts. All hidden behind a admin login.</summary>
  <img width="1649" height="1122" alt="image" src="https://github.com/user-attachments/assets/955c98c4-fd94-4d9e-8f2b-5cf84952db55" /> <br/>
  <img width="1649" height="1122" alt="image" src="https://github.com/user-attachments/assets/0cf47978-99bf-4f4f-8ca7-2943d5a7fe6e" /> <br/>
</details>

<details>
  <summary>Multi-language dynamic information to manage wedding details like invite text, the schedule and FAQs in the languages needed for your weeding guests.</summary>
  <img width="1649" height="1122" alt="image" src="https://github.com/user-attachments/assets/14447c50-5b7d-4b20-9b9f-b3c760a8c4d2" />
</details>

<details>
  <summary>Email notifications sent to the organizers upon RSVP submission.</summary>
  <img width="1641" height="476" alt="image" src="https://github.com/user-attachments/assets/e55e9442-a57d-43f2-af22-6d7710a1ddea" />
</details>

<details>
  <summary>Customized invite page for every guest based on the number of guests and the language selected. The invite page will be exactly as needed to the specific guest.</summary>
  <img width="1634" height="1124" alt="image" src="https://github.com/user-attachments/assets/9ce317a9-a93b-4ea5-8c88-80323842704b" /> <br/>
  <img width="1634" height="1124" alt="image" src="https://github.com/user-attachments/assets/763f28e1-64b2-40af-87ac-c8898b2548d4" />
</details>

<details>
  <summary>Aspire orchestration for seamless local development orchestration, service discovery, and monitoring. This includes open telemtry metrics.</summary>
  <img width="1647" height="1124" alt="image" src="https://github.com/user-attachments/assets/8abff86a-75a5-4e81-9477-91b86924d5e2" />
</details>

## 🏗️ Architecture & Project Structure

The solution is divided into several logical components:

### Core Services (`src-docker`)
- **WeddingRsvp.Api**: An ASP.NET Core API providing the backend logic, MongoDB integration, and email notification services.
- **WeddingRsvp.WebApp**: A Blazor Web App (Interactive SSR/Server) for the guest-facing interface.

### Shared Libraries (`src-dotnet`)
- **WeddingRsvp.AppHost**: The **.NET Aspire** orchestrator project. It manages the lifecycle of the API, WebApp, and MongoDB container.
- **WeddingRsvp.ServiceDefaults**: Standardized configurations for resilience, service discovery, and telemetry.
- **WeddingRsvp.Abstractions**: Shared DTOs (Data Transfer Objects) and models used across the solution.
- **WeddingRsvp.Client**: A HttpClient wrapper for interacting with the RSVP API.

### Infrastructure
- **MongoDB**: Used as the primary data store, managed via Aspire in development.
- **Prometheus**: Scrapes metrics from the Api and WebApp.
- **Grafana**: Vizualize ASPNET Core metrics and custom application metrics.
- **Email Service**: Configurable email service leveraging features of [SendGrid](https://sendgrid.com/en-us) for sending RSVP confirmation notifications.

### Test coverage
- **Unit Tests** (`test`): Comprehensive unit tests covering core functionality.
- **Integration Tests** (`integration`): Integration tests ensuring seamless interaction between system boundaries.

## 🛠️ Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Aspire container resources)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/)

### Running locally with Aspire
1. Clone the repository.
2. Set `WeddingRsvp.AppHost` as the startup project.
3. Press `F5` or `Run`.
4. The Aspire Dashboard will open, showing the status of the API, WebApp, and MongoDB.

### Seeding the Database
The AppHost includes a custom command to seed the database.
1. Open the Aspire Dashboard.
2. Locate the `api` resource.
3. Click the **"Clean and seed Database"** button (Database icon).

## ⚙️ Configuration

Key settings are managed via `appsettings.json` or environment variables:
- `WeddingRsvp:ApiKey`: Secures the API communications.
- `WeddingRsvp:AdminIdentifier`: Unique ID for administrative access.
- Email service configuration for RSVP confirmation notifications (see `EmailServiceConfiguration` section).
