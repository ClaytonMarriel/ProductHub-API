# API Web - Product CRUD (.NET)

A RESTful API built with **ASP.NET Core** for product management, including full CRUD operations (Create, Read, Update, Delete), **Entity Framework Core** integration, and **SQL Server** persistence.

This project is my first backend version in C#, focused on solid fundamentals, clean growth, and portfolio building for Full Stack / Backend opportunities.

## Project Goal

Build a functional and well-structured API to strengthen backend fundamentals with C#/.NET, including:

- REST endpoint design
- relational database integration
- migrations/versioning workflow
- code structure ready for future improvements

## Tech Stack

- **C#**
- **.NET / ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **Swagger / OpenAPI**
- **EF Core Migrations (Code First)**

## Current Features

- ✅ Create products
- ✅ List products
- ✅ Get product by ID
- ✅ Update products
- ✅ Delete products
- ✅ SQL Server persistence
- ✅ Scallar endpoint documentation

## Main Entity

The API currently uses a product table with the following fields:

- `Id`
- `Name`
- `Description`
- `QuantityStock`
- `BarCode`
- `Mark`

## Endpoints (v1)

- `GET /api/products` → list all products
- `GET /api/products/{id}` → get product by id
- `POST /api/products` → create product
- `PUT /api/products/{id}` → update product
- `DELETE /api/products/{id}` → delete product

## How to Run

1. Clone this repository
2. Set your connection string in `appsettings.json`
3. Run database migrations:
   ```bash
   dotnet ef database update
