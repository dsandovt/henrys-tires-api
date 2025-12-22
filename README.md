# Henry's Tires Inventory API

Production-ready REST API for multi-branch tire inventory management built with .NET 8, MongoDB, and JWT authentication following hexagonal architecture principles.

## Features

- **Multi-Branch Support**: 5 branches (Mercury, Williamsburg, Warwick, Jefferson, Pembroke)
- **Multi-Tenant Security**: Branch users can only access their branch data; Admins have full access
- **Inventory Transactions**: Create and post IN/OUT transactions with atomic balance updates
- **Stock Management**: Real-time inventory balances with negative stock prevention
- **Dashboard Analytics**: Summaries with totals and top products
- **MongoDB Transactions**: ACID guarantees for posting operations using MongoDB replica sets
- **JWT Authentication**: Secure token-based authentication
- **Hexagonal Architecture**: Clean separation of Domain, Application, Infrastructure, and API layers

## Architecture

```
HenryTires.Inventory/
├── Domain/              # Entities, enums, business rules (no dependencies)
├── Application/         # Use cases, ports (interfaces), DTOs
├── Infrastructure/      # MongoDB, JWT, repositories (implements ports)
└── Api/                 # Controllers, middleware, DI composition
```

## Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Docker & Docker Compose** (for containerized deployment) OR
- **MongoDB 7.0+** configured as a replica set (for local development)

## Quick Start with Docker

### 1. Clone and Build

```bash
# Clone the repository
cd /path/to/HenryTires.Inventory

# Start MongoDB + API with docker-compose
docker-compose up -d

# Check logs
docker-compose logs -f api
```

The API will be available at **http://localhost:5000**

### 2. Access Swagger UI

Navigate to: **http://localhost:5000/swagger**

### 3. Seed Development Data

```bash
curl -X POST http://localhost:5000/api/auth/seed
```

This creates:
- 5 branches (Mercury, Williamsburg, Warwick, Jefferson, Pembroke)
- 1 admin user: `admin` / `admin123`
- 5 branch users: `mercury` / `mercury123`, `williamsburg` / `williamsburg123`, etc.

### 4. Login and Get Token

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "username": "admin",
    "role": "Admin",
    "branchId": null
  }
}
```

**Save the token** - you'll need it for authenticated requests!

---

## MongoDB Atlas Setup

Since you have a MongoDB Atlas URI, update the configuration:

### Option 1: Environment Variables (Docker)

Edit `docker-compose.yml`:

```yaml
services:
  api:
    environment:
      - MongoDb__ConnectionString=YOUR_ATLAS_URI_HERE
      - MongoDb__DatabaseName=HenryTiresInventory
```

**Important**: Your Atlas URI **must** include `?replicaSet=...` to support transactions

### Option 2: appsettings (Local Development)

Edit `src/HenryTires.Inventory.Api/appsettings.Development.json`:

```json
{
  "MongoDb": {
    "ConnectionString": "YOUR_ATLAS_URI_HERE",
    "DatabaseName": "HenryTiresInventory"
  }
}
```

---

## Local Development (Without Docker)

### 1. Set Up MongoDB Replica Set

**Option A: Use MongoDB Atlas** (Recommended)
- Already configured as a replica set
- Just use your connection URI

**Option B: Local MongoDB**

```bash
# Start MongoDB as a replica set
mongod --replSet rs0 --port 27017 --dbpath /data/db

# In another terminal, initialize replica set
mongosh
> rs.initiate()
```

### 2. Run the API

```bash
cd src/HenryTires.Inventory.Api

# Restore dependencies
dotnet restore

# Run
dotnet run

# Or with watch mode
dotnet watch run
```

API runs at: **https://localhost:5001** (HTTPS) or **http://localhost:5000** (HTTP)

---

## API Endpoints

### Authentication

#### Login
```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

#### Seed Development Data (Development Only)
```bash
POST /api/auth/seed
```

---

### Branches

#### Get All Branches
```bash
GET /api/branches
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "674f...",
      "code": "MERCURY",
      "name": "Mercury (Principal)"
    },
    ...
  ]
}
```

---

### Products

#### Create Product
```bash
POST /api/products
Authorization: Bearer <token>
Content-Type: application/json

{
  "itemCode": "P225/65R17",
  "description": "All-Season Tire 225/65R17"
}
```

#### Search Products
```bash
GET /api/products?search=225&page=1&pageSize=20
Authorization: Bearer <token>
```

#### Get Product by ID
```bash
GET /api/products/{productId}
Authorization: Bearer <token>
```

---

### Inventory Transactions

#### Create Transaction (Draft)

**As Mercury Branch User:**
```bash
POST /api/inventory/transactions
Authorization: Bearer <mercury-user-token>
Content-Type: application/json

{
  "type": "In",
  "notes": "Receiving shipment from supplier",
  "lines": [
    {
      "productId": "674f...",
      "condition": "New",
      "quantity": 50,
      "fromLocation": "Supplier A",
      "toLocation": "Warehouse"
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "674f...",
    "branchId": "674f...",
    "type": "In",
    "status": "Draft",
    "notes": "Receiving shipment from supplier",
    "postedAtUtc": null,
    "lines": [...],
    "createdAtUtc": "2025-12-20T10:30:00Z",
    "createdBy": "mercury"
  }
}
```

#### Post Transaction (Commit to Inventory)

```bash
POST /api/inventory/transactions/{transactionId}/post
Authorization: Bearer <token>
```

**This will:**
- Validate product existence
- For OUT transactions: Check sufficient stock (prevent negative inventory)
- Update `InventoryBalance` atomically via MongoDB transaction
- Mark transaction as `Posted` (immutable)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "674f...",
    "status": "Posted",
    "postedAtUtc": "2025-12-20T10:35:00Z",
    ...
  }
}
```

#### Search Transactions

```bash
GET /api/inventory/transactions?branchId=674f...&from=2025-01-01&to=2025-12-31&type=In&status=Posted&page=1&pageSize=20
Authorization: Bearer <token>
```

**Query Parameters:**
- `branchId` - Filter by branch (required for Admin, ignored for BranchUser)
- `from` - Start date (YYYY-MM-DD)
- `to` - End date (YYYY-MM-DD)
- `type` - `In` or `Out`
- `status` - `Draft`, `Posted`, or `Cancelled`
- `productId` - Filter by product
- `condition` - `New` or `Used`
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20)

#### Get Transaction by ID

```bash
GET /api/inventory/transactions/{id}
Authorization: Bearer <token>
```

---

### Stock Queries

#### Get Current Stock

```bash
GET /api/inventory/stock?branchId=674f...&search=225&condition=New&page=1&pageSize=20
Authorization: Bearer <token>
```

**Query Parameters:**
- `branchId` - Required for Admin; auto-filled for BranchUser
- `search` - Search by product code/description
- `condition` - `New` or `Used`
- `page`, `pageSize` - Pagination

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "productId": "674f...",
        "productCode": "P225/65R17",
        "productDescription": "All-Season Tire",
        "condition": "New",
        "quantityOnHand": 45,
        "updatedAtUtc": "2025-12-20T10:35:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20
  }
}
```

---

### Dashboard

#### Get Dashboard Summary

```bash
GET /api/dashboard/summary?branchId=674f...&from=2025-01-01&to=2025-12-31
Authorization: Bearer <token>
```

**Query Parameters:**
- `branchId` - Required for Admin; auto-filled for BranchUser
- `from`, `to` - Date range (optional)

**Response:**
```json
{
  "success": true,
  "data": {
    "totalInQty": 500,
    "totalOutQty": 120,
    "countInTx": 5,
    "countOutTx": 8,
    "currentStockTotalQty": 380,
    "topProductsOut": [
      {
        "productId": "674f...",
        "productCode": "P225/65R17",
        "totalQuantityOut": 50
      }
    ]
  }
}
```

---

## Sample Workflow

### Complete Flow: Create Product → Receive Stock → Sell Stock

```bash
# 1. Login as Admin
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.data.token')

# 2. Get Mercury Branch ID
MERCURY_BRANCH_ID=$(curl -s -X GET http://localhost:5000/api/branches \
  -H "Authorization: Bearer $TOKEN" \
  | jq -r '.data[] | select(.code=="MERCURY") | .id')

# 3. Create a Product
PRODUCT_ID=$(curl -s -X POST http://localhost:5000/api/products \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"itemCode":"P225/65R17","description":"All-Season Tire 225/65R17"}' \
  | jq -r '.data.id')

# 4. Login as Mercury Branch User
TOKEN_MERCURY=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"mercury","password":"mercury123"}' \
  | jq -r '.data.token')

# 5. Create IN Transaction (Draft)
TX_IN_ID=$(curl -s -X POST http://localhost:5000/api/inventory/transactions \
  -H "Authorization: Bearer $TOKEN_MERCURY" \
  -H "Content-Type: application/json" \
  -d "{\"type\":\"In\",\"notes\":\"Receiving shipment\",\"lines\":[{\"productId\":\"$PRODUCT_ID\",\"condition\":\"New\",\"quantity\":100,\"fromLocation\":\"Supplier\",\"toLocation\":\"Warehouse\"}]}" \
  | jq -r '.data.id')

# 6. Post IN Transaction (commit to inventory)
curl -X POST "http://localhost:5000/api/inventory/transactions/$TX_IN_ID/post" \
  -H "Authorization: Bearer $TOKEN_MERCURY"

# 7. Check Stock
curl -X GET "http://localhost:5000/api/inventory/stock" \
  -H "Authorization: Bearer $TOKEN_MERCURY"

# 8. Create OUT Transaction
TX_OUT_ID=$(curl -s -X POST http://localhost:5000/api/inventory/transactions \
  -H "Authorization: Bearer $TOKEN_MERCURY" \
  -H "Content-Type: application/json" \
  -d "{\"type\":\"Out\",\"notes\":\"Sale to customer\",\"lines\":[{\"productId\":\"$PRODUCT_ID\",\"condition\":\"New\",\"quantity\":10,\"fromLocation\":\"Warehouse\",\"toLocation\":\"Customer\"}]}" \
  | jq -r '.data.id')

# 9. Post OUT Transaction
curl -X POST "http://localhost:5000/api/inventory/transactions/$TX_OUT_ID/post" \
  -H "Authorization: Bearer $TOKEN_MERCURY"

# 10. Check Stock Again (should show 90 remaining)
curl -X GET "http://localhost:5000/api/inventory/stock" \
  -H "Authorization: Bearer $TOKEN_MERCURY"

# 11. Get Dashboard Summary
curl -X GET "http://localhost:5000/api/dashboard/summary" \
  -H "Authorization: Bearer $TOKEN_MERCURY"
```

---

## Business Rules

1. **Only Posted transactions affect inventory balances**
2. **Posted transactions are immutable** - cannot be modified or deleted
3. **OUT transactions cannot create negative stock** - posting will fail with a 409 Conflict if insufficient inventory
4. **Branch isolation for BranchUser role** - users can only see/modify their branch data
5. **Admin users must specify branchId** in endpoints that require it
6. **Reversals require new transactions** - create an opposite transaction instead of modifying existing ones
7. **All timestamps are UTC**
8. **MongoDB transactions ensure atomicity** - balance updates + transaction status changes happen together or not at all

---

## Error Responses

All errors follow this format:

```json
{
  "success": false,
  "errorMessage": "Insufficient stock for product 'P225/65R17' (Condition: New). Required: 200, Available: 90",
  "developerMessage": "HenryTires.Inventory.Application.Common.BusinessException: Insufficient stock..."
}
```

**HTTP Status Codes:**
- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Missing or invalid JWT token
- `403 Forbidden` - User doesn't have permission (branch access)
- `404 Not Found` - Resource not found
- `409 Conflict` - Business rule violation (e.g., negative stock)
- `500 Internal Server Error` - Unexpected errors

---

## Development Notes

### Project Structure

- **Domain Layer** (`HenryTires.Inventory.Domain`)
  - Pure business entities and enums
  - No external dependencies

- **Application Layer** (`HenryTires.Inventory.Application`)
  - Use cases (services)
  - Ports (interfaces)
  - DTOs and mappings

- **Infrastructure Layer** (`HenryTires.Inventory.Infrastructure`)
  - MongoDB repositories
  - JWT token service
  - Password hashing
  - Current user service

- **API Layer** (`HenryTires.Inventory.Api`)
  - Controllers
  - Middleware (exception handling)
  - Dependency injection setup

### Running Tests

*(Not implemented in MVP, but infrastructure is test-friendly)*

```bash
dotnet test
```

### Database Initialization

The API automatically:
1. Creates MongoDB collections and indexes on startup
2. Seeds 5 branches (if not already present)
3. Validates MongoDB is running as a replica set

---

## Production Considerations

1. **Change JWT Secret**: Update `Jwt:Key` in `appsettings.json` to a secure random value
2. **Use HTTPS**: Enable HTTPS in production
3. **MongoDB Replica Set**: Ensure MongoDB is properly configured as a replica set (Atlas does this automatically)
4. **Environment Variables**: Store sensitive config (connection strings, JWT keys) as environment variables
5. **Logging**: Configure Serilog sinks for persistent logging (e.g., Seq, Elasticsearch)
6. **Rate Limiting**: Add rate limiting middleware for public endpoints
7. **CORS**: Restrict CORS to specific origins in production

---

## Troubleshooting

### MongoDB Connection Issues

**Error**: `Server selection timeout`

**Solution**: Ensure MongoDB is running and accessible:
```bash
# Check Docker container
docker ps | grep mongo

# Check MongoDB logs
docker logs henrytires-mongodb

# Test connection
mongosh "mongodb://localhost:27017/?replicaSet=rs0"
```

### Replica Set Not Initialized

**Error**: `Transaction numbers are only allowed on a replica set member or mongos`

**Solution**: Initialize replica set manually:
```bash
docker exec -it henrytires-mongodb mongosh
> rs.initiate({_id:'rs0',members:[{_id:0,host:'mongodb:27017'}]})
> rs.status()
```

### Negative Stock Error

**Error**: `409 Conflict - Insufficient stock`

**Solution**: This is expected behavior! Create an IN transaction to add stock before posting OUT transactions.

---

## License

Proprietary - Henry's Tires Inc.

---

## Support

For issues or questions, contact the development team or open an issue in the repository.
