# Auth Forge API

[![C#](https://img.shields.io/badge/C%23-13.0-blue.svg)](https://dotnet.microsoft.com/)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A robust and modular authentication API built from scratch using modern software engineering principles and best practices.

AuthForge is an independent identity provider designed to serve as a centralized authentication service for applications, APIs, and microservices. The project was created with a strong focus on Clean Architecture, maintainability, security, and scalability.

![AuthForgeApi Showcase](.github/images/showcase-auth-forge-api.jpg)

## ✨ Features

* User registration
* User authentication (Login)
* JWT-based authentication and authorization
* Argon2id password hashing
* PostgreSQL integration
* EF Core for writes, Dapper for reads
* Clean Architecture implementation
* Dependency Injection
* Swagger/OpenAPI documentation


## 🏗️ Architecture

The solution follows the principles of **Clean Architecture** and **Separation of Concerns (SoC)**.

```text
AuthForge
├── Presentation
├── Application
├── Domain
├── Infrastructure
└── Tests
```

### Layers

#### Domain

Contains the core business entities and contracts.

* Entities
* Interfaces
* Domain rules

#### Application

Implements the application's business logic.

* Authentication services
* User services
* Use cases
* Application contracts

#### Infrastructure

Responsible for external concerns.

* Repositories combining EF Core (writes) and Dapper (reads)
* PostgreSQL integration
* JWT services
* Argon2id password hashing

#### Presentation

API layer responsible for handling HTTP requests and responses.

* Controllers
* Middleware configuration
* Dependency Injection
* Authentication setup


## 🔐 Security

AuthForge follows modern authentication and security practices.

### Password Hashing

Passwords are secured using **Argon2id**, one of the most recommended password hashing algorithms available today.

### Authentication

Authentication is handled through **JWT (JSON Web Tokens)** with:

* Signature validation
* Expiration validation
* Issuer validation
* Audience validation

### Secrets Management

Sensitive configuration values are stored using **User Secrets** during development and can be replaced by environment variables or secret managers in production.


## 🛠️ Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/thesampaio/AuthForge.git
cd AuthForge
```

### 2. Configure the Database

Create a PostgreSQL database (locally, via Docker, or a managed instance such as Render), then apply the EF Core migrations to create the schema:

```bash
dotnet ef database update --project Backend/Infrastructure --startup-project Backend/Presentation
```

New schema changes go through EF Core migrations rather than hand-written SQL:

```bash
dotnet ef migrations add <MigrationName> --project Backend/Infrastructure --startup-project Backend/Presentation --output-dir Persistence/Migrations
```

### 3. Configure JWT Secret

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Add your JWT secret key:

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "YourSuperLongAndSecureSecretKey"
```

### 4. Configure the Connection String

Update your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=authforge;Username=postgres;Password=YourPassword"
  }
}
```

### 5. Run the Application

```bash
dotnet run --project Presentation
```

### 6. Run the Tests

Unit tests cover the Application services (registration/login/SSO join/assign/revoke flows) and the Infrastructure security services (Argon2id hashing, JWT generation), using xUnit and Moq with mocked repositories — no database required.

```bash
dotnet test Backend/Tests/AuthForge.Tests/AuthForge.Tests.csproj
```


## 🐳 Running with Docker

`docker-compose` runs the API together with a local PostgreSQL instance — no manually installed or started Postgres required.

Provide `JWT_SECRET_KEY` and `CRYPTO_PEPPER` (e.g. in a local `.env` file, gitignored, or exported in your shell), then:

```bash
docker compose up --build
```

The API is available at `http://localhost:8080`, migrations are applied automatically on startup, and Postgres data persists in a named volume across restarts.

To build the image standalone (e.g. to test what gets deployed):

```bash
docker build -t authforge-api .
```


## ☁️ Deploying to Render

Render only supports PostgreSQL, which is why the database layer targets it. The included `render.yaml` blueprint provisions both the web service (built from the `Dockerfile`) and a managed Postgres database, and wires the connection string between them automatically.

1. Push the repository to GitHub.
2. In the Render dashboard, choose **New → Blueprint** and point it at the repository — Render reads `render.yaml` and provisions both resources.
3. `JwtSettings:SecretKey` and `CryptoSettings:Pepper` are generated automatically by Render (`generateValue: true`); no manual secret entry needed.
4. On deploy, the container runs any pending EF Core migrations on startup and exposes `GET /health` for Render's health check.

To deploy without the blueprint (manually creating the service in the dashboard instead), set the runtime to **Docker** and configure these environment variables yourself: `ConnectionStrings__DefaultConnection` (from your Render Postgres instance — the `postgres://...` URI it gives you is accepted as-is), `JwtSettings__SecretKey`, `JwtSettings__Issuer`, `JwtSettings__Audience`, `JwtSettings__ExpirationInMinutes`, `CryptoSettings__Pepper`.


## 📚 API Endpoints

| Method | Endpoint                                                | Description                                                   |
| ------ | -------------------------------------------------------- | -------------------------------------------------------------- |
| POST   | `/api/v1/admin/register`                                 | Register the central platform identity                        |
| POST   | `/api/v1/admin/login`                                    | Authenticate the central identity and receive a JWT           |
| POST   | `/api/v1/admin/applications`                             | Register a new application (Requires central JWT)             |
| GET    | `/api/v1/admin/applications`                             | List applications you administer (Requires JWT)               |
| POST   | `/api/v1/admin/applications/users`                       | Assign a user's role for an application (Requires JWT)         |
| DELETE | `/api/v1/admin/applications/{clientId}/users/{userId}`   | Revoke a user's access to an application (Requires JWT)        |
| DELETE | `/api/v1/admin/applications/{clientId}`                  | Deactivate an application (Requires JWT)                       |
| GET    | `/api/v1/admin/users`                                    | Retrieve active users (Requires central JWT)                  |
| GET    | `/api/v1/admin/users/{email}`                            | Retrieve a user by e-mail (Requires central JWT)               |
| POST   | `/api/v1/users/register`                                 | Register (or join) an end user against an application (SSO)   |
| POST   | `/api/v1/users/login`                                    | Authenticate an end user against an application (SSO)          |


## 📖 API Documentation

Once the application is running, Swagger UI will be available at:

```text
/swagger
```

This interface allows you to test and explore all available endpoints.


## 🧪 Technologies

| Category          | Technology         |
| ----------------- | ------------------ |
| Framework         | .NET 9             |
| Language          | C# 13              |
| Database          | PostgreSQL         |
| Data Access       | EF Core (writes), Dapper (reads) |
| Authentication    | JWT                |
| Password Security | Argon2id           |
| API Documentation | Swagger/OpenAPI    |
| Architecture      | Clean Architecture |


## 🤝 Contributing

Engineering conventions for this repository (Clean Architecture rules, SOLID/DRY/KISS, comment and documentation style, git workflow) are documented in [CLAUDE.md](CLAUDE.md). Read it before making changes.


## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.


## 👨‍💻 Author

**Kellvyn Sampaio**

* GitHub: https://github.com/thesampaio
* Portfolio: https://thesampaio.github.io/Portfolio/

---

### Educational Purpose

This project was developed primarily for learning, experimentation, and demonstrating software engineering best practices.
