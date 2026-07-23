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
* SQL Server integration
* Dapper-based data access
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

* Dapper repositories
* SQL Server integration
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

Execute the SQL scripts located in the `Database` folder to create the database structure and required stored procedures.

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
    "DefaultConnection": "Your SQL Server connection string"
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
| Database          | SQL Server         |
| Data Access       | Dapper             |
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
