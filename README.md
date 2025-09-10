
<img width="2863" height="2498" alt="image" src="https://github.com/user-attachments/assets/f0c60ff1-c3f7-4a7a-98de-eebf2699f341" />

# Personal Finance Tracker API

A .NET 8 Web API for importing and analyzing personal bank transaction data. Upload CSV files, automatically categorize transactions, and get financial insights.

## Features

- **CSV Import**: Bulk import bank transactions with automatic duplicate detection
- **Smart Categorization**: Automatically categorize transactions based on merchant patterns
- **Financial Analytics**: Track spending, income, and savings rates
- **Transaction Management**: Search, filter, and manage transactions with pagination
- **Category Management**: Create custom categories and bulk categorization tools

## Quick Start

### Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server LocalDB

### Setup

1. Clone the repository
1. Update the connection string in `appsettings.json`
1. Run the application:
   
   ```bash
   dotnet run
   ```
1. Visit `https://localhost:7000` for the API documentation

The database will be created automatically on first run with pre-seeded categories.

## API Endpoints

### Transactions

- `POST /api/transaction/upload-csv` - Import CSV file
- `GET /api/transaction` - Get transactions (paginated, searchable)
- `GET /api/transaction/summary` - Financial summary and analytics
- `PUT /api/transaction/{id}/category` - Update transaction category

### Categories

- `GET /api/category` - List all categories
- `POST /api/category` - Create new category
- `POST /api/category/categorize-with-pattern` - Auto-categorize similar transactions

## CSV Format

Your bank CSV should have these columns:

```csv
Date,Description,Credit,Debit,Balance
01/01/2024,"ACME STORE - Purchase",0,25.50,1234.56
02/01/2024,"Salary Payment",2500.00,0,3734.06
```

Supports various date formats (dd/MM/yyyy, d/M/yyyy, etc.).

## Key Functionality

**Duplicate Detection**: Uses transaction hash (date + description + amount) to prevent duplicate imports.

**Pattern Recognition**: Extracts business names from transaction descriptions for intelligent categorization.

```
"ACME STORE - Receipt 123" → "ACME STORE"
"McDONALD'S 456 AUTH:789" → "McDONALD'S"
```

**Financial Analytics**:

- Income vs expenses tracking
- Savings rate calculations
- Spending breakdown by category
- Option to exclude internal transfers

**Smart Categorization**: Select any transaction and automatically categorize all similar uncategorized transactions.

## Technology Stack

- .NET 8 Web API
- Entity Framework Core
- SQL Server
- Custom CSV parser
- CORS enabled for frontend integration

## Demo Data

The application comes with pre-configured categories:

- Food & Dining, Groceries, Coffee
- Transport, Petrol, Car
- Bills & Utilities, Shopping
- Income, Transfers, Fitness
- Gifts, Rebates, Leisure

## Frontend Integration

API is configured with CORS for `http://localhost:3000` (React development server). Update `ApiRegistration.cs` for different frontend URLs.

-----

*A practical financial data management solution built with .NET 8*
<img width="2863" height="1441" alt="image" src="https://github.com/user-attachments/assets/8fd2bbfd-af0a-406b-a3d3-aa7ac3164ab8" />

<img width="2863" height="1479" alt="image" src="https://github.com/user-attachments/assets/0b74121d-9000-41d9-9662-e3da87639b72" />


<img width="1460" height="2021" alt="image" src="https://github.com/user-attachments/assets/20effbb7-2903-4c40-872b-417c447eb483" />
