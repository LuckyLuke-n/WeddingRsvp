
# WeddingRsvp Solution

A modern, cloud-native Wedding RSVP management system built with **.NET 10**, **Aspire**, and **Blazor**. This solution leverages a microservices-friendly architecture to manage guest responses and dynamic wedding information.

## 🚀 Features

- **Guest Management**: Track RSVPs, dietary requirements, and guest counts.
- **Email Notifications**: Automated confirmation emails sent to organizer upon RSVP submission.
- **Dynamic Information**: Manage wedding details (locations, schedules, etc.) via a dedicated API.
- **Aspire Integration**: Seamless local development orchestration, service discovery, and monitoring.
- **Blazor WebApp**: An interactive UI for guests to submit their replies.
- **MongoDB Backend**: Flexible document storage for RSVP data.
- **Docker Ready**: Includes Dockerfiles and a `compose.yaml` for containerized deployment.

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