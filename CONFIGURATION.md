# Configuration Guide

This document explains how to configure the Henry's Tires Inventory API for local development and production.

## Quick Start

1. **Copy the example configuration:**
   ```bash
   cp src/HenryTires.Inventory.Api/appsettings.Example.json src/HenryTires.Inventory.Api/appsettings.Development.json
   ```

2. **Edit `appsettings.Development.json` with your settings:**
   ```json
   {
     "MongoDb": {
       "ConnectionString": "mongodb+srv://YOUR_USERNAME:YOUR_PASSWORD@YOUR_CLUSTER.mongodb.net/",
       "DatabaseName": "HenryTiresInventory"
     },
     "Jwt": {
       "Key": "your-super-secret-jwt-key-min-32-characters-long",
       "Issuer": "https://localhost:5000",
       "Audience": "https://localhost:5000",
       "ExpiryMinutes": "480"
     }
   }
   ```

3. **Run the application:**
   ```bash
   dotnet run --project src/HenryTires.Inventory.Api
   ```

## Environment Variables (Recommended for Production)

For production deployments, use environment variables instead of configuration files:

```bash
# MongoDB Configuration
export MONGO_CONNECTION_STRING="mongodb+srv://username:password@cluster.mongodb.net/"

# JWT Configuration (if needed)
export JWT_KEY="your-production-jwt-key-min-32-characters-long"
export JWT_ISSUER="https://api.henrytires.com"
export JWT_AUDIENCE="https://henrytires.com"
```

## Configuration Priority

The application loads configuration in this order (later sources override earlier ones):

1. `appsettings.json` (base configuration, committed to git)
2. `appsettings.Development.json` (local development, **NOT committed to git**)
3. `appsettings.Production.json` (production settings, **NOT committed to git**)
4. Environment variables (highest priority)

## MongoDB Connection

The application checks for the MongoDB connection string in this order:

1. `MONGO_CONNECTION_STRING` environment variable
2. `ConnectionStrings:MongoDbLocal` from configuration files
3. `MongoDb:ConnectionString` from configuration files

See: `src/HenryTires.Inventory.Infrastructure/Extensions/ServiceCollectionExtensions.cs`

## Security Notes

**IMPORTANT:** Never commit the following files to version control:
- `appsettings.Development.json` - Contains local development secrets
- `appsettings.Production.json` - Contains production secrets
- `appsettings.Local.json` - Contains local overrides

These files are already listed in `.gitignore`.

## Required Configuration Values

### MongoDB
- **ConnectionString**: MongoDB Atlas or local connection string
- **DatabaseName**: Database name (default: "HenryTiresInventory")

### JWT Authentication
- **Key**: Secret key for signing JWT tokens (minimum 32 characters)
- **Issuer**: Token issuer URL
- **Audience**: Token audience URL
- **ExpiryMinutes**: Token expiration time in minutes

## Example Configurations

### Local Development
```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "HenryTiresInventory_Dev"
  },
  "Jwt": {
    "Key": "dev-secret-key-min-32-characters-long-for-development-only",
    "Issuer": "https://localhost:5000",
    "Audience": "https://localhost:5000",
    "ExpiryMinutes": "480"
  }
}
```

### Production (via Environment Variables)
```bash
MONGO_CONNECTION_STRING="mongodb+srv://prod_user:SECURE_PASSWORD@prod-cluster.mongodb.net/?retryWrites=true&w=majority"
JWT_KEY="production-super-secure-key-min-32-characters-long"
JWT_ISSUER="https://api.henrytires.com"
JWT_AUDIENCE="https://henrytires.com"
JWT_EXPIRY_MINUTES="480"
```

## Troubleshooting

### "MongoDB connection string not configured"
- Ensure you have created `appsettings.Development.json` from the example
- Or set the `MONGO_CONNECTION_STRING` environment variable

### "Invalid JWT configuration"
- Ensure the JWT Key is at least 32 characters long
- Verify all JWT settings are present in your configuration file
