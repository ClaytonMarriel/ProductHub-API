# API Web - Product CRUD (.NET 9)

A RESTful Web API built with **ASP.NET Core 9** for product management, including **authentication**, **authorization**, **refresh token flow**, and **role-based access control**.

This project was created as part of my backend learning journey with **C#** and **.NET**, with a focus on building a solid foundation in API development, security, persistence, and clean project structure.

---

## Project Overview

The goal of this project is to practice and consolidate important backend concepts through a real-world API structure.

This application includes:

- user registration and login
- JWT authentication
- Google login
- role-based authorization (`Admin` and `User`)
- refresh token flow
- product CRUD
- pagination and filtering
- soft delete
- audit fields
- global exception handling
- interactive API documentation with **OpenAPI + Scalar**

---

## Main Goals

This project was designed to strengthen my backend skills in areas such as:

- building REST APIs with ASP.NET Core
- working with Entity Framework Core
- integrating SQL Server
- implementing authentication and authorization
- applying role-based permissions
- handling refresh tokens securely
- organizing code into clear layers
- documenting APIs with OpenAPI

---

## Tech Stack

- **C#**
- **.NET 9 / ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **ASP.NET Identity**
- **JWT Bearer Authentication**
- **Google OAuth**
- **OpenAPI**
- **Scalar**

---

## Features

### Authentication
- User registration
- User login with email and password
- Google login
- JWT generation
- Authenticated user endpoint (`/api/auth/me`)
- Refresh token generation
- Refresh token revocation

### Authorization
- `User` role
- `Admin` role
- Regular users can only view products
- Admin users can create, update, and delete products

### Product Management
- Create product
- Get all products
- Get product by id
- Update product
- Delete product (soft delete)
- Search filter
- Brand filter
- Pagination
- Barcode uniqueness validation

### Additional Backend Improvements
- Global exception middleware
- DTO-based request/response structure
- Entity audit fields
- Clean separation of concerns
- Interactive API documentation with Scalar

---

## Project Structure

The project follows a layered organization:

- `Controllers` → API endpoints
- `Services` → business rules and application logic
- `DTOs` → request and response contracts
- `Models` → domain entities
- `Data` → database context and seed logic
- `Mappings` → entity/DTO transformations
- `Middlewares` → global exception handling
- `Constants` → reusable constants such as roles

---

## Authentication Flow

### Access Token
After login or registration, the API returns an `accessToken`, which is used to access protected endpoints.

### Refresh Token
The API also returns a `refreshToken`, which can be used to generate a new access token when the current one expires.

### Role-Based Access
- `User`: read-only access to products
- `Admin`: full access to product management

---

## Product Entity

The main entity currently implemented is `Product`.

### Fields
- `Id`
- `Name`
- `Description`
- `QuantityStock`
- `BarCode`
- `Mark`
- `CreatedAt`
- `UpdatedAt`
- `IsDeleted`

---

## API Endpoints

## Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `POST /api/auth/refresh`
- `POST /api/auth/revoke`
- `GET /api/auth/google`

## Products
- `GET /api/products`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`

---

## How to Run the Project

### 1. Clone the repository

```bash
git clone <your-repository-url>
cd ApiWeb
