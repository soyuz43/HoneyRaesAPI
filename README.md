# HoneyRaesAPI

This is a learning project built with ASP.NET Core Minimal APIs for managing service tickets at a fictional repair service called Honey Rae's Repairs.

## Features

- ✅ In-memory database using C# collections
- ✅ Minimal API endpoints for:
  - Customers
  - Employees
  - Service Tickets
- ✅ Support for:
  - Create, Read, Update, Delete (CRUD)
  - Nested DTOs (including related data)
  - Custom logic endpoints (e.g., complete a ticket)

## Tech Stack

- ASP.NET Core 8 (Minimal API)
- C# 12
- Postman / cURL for testing
- GitHub for version control

## Getting Started

```bash
dotnet watch run
```
Then visit: http://localhost:5328/swagger
> ℹ️ Replace 5238 with the port shown in your terminal if different.
## API Examples

### 📬 Create a ticket
```bash
curl -X POST http://localhost:5238/servicetickets \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "description": "Garage door won'\''t open",
    "emergency": false
  }'
```
### ✅ Complete a ticket
```bash
curl -X POST http://localhost:5238/servicetickets/3/complete
```

---