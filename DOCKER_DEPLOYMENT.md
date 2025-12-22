# Docker Deployment Guide

## ✅ Fixed Dockerfile

The Dockerfile has been updated to properly handle environment variables for production deployments.

## How to Run with Docker

### Option 1: Using Docker with MongoDB Atlas (Recommended)

When you deploy to Railway, Render, or any cloud platform:

```bash
# Build the image
docker build -t henrys-tires-api .

# Run with environment variables
docker run -p 8080:8080 \
  -e MONGO_CONNECTION_STRING="mongodb+srv://username:password@cluster.mongodb.net/" \
  -e Jwt__Key="your-production-jwt-key" \
  henrys-tires-api
```

### Option 2: Using Docker Compose (Local Development)

Your existing `docker-compose.yml` will work for local development:

```bash
# Start both MongoDB and API
docker-compose up

# API will be available at: http://localhost:5000
```

This uses a local MongoDB container with replica set support.

## Environment Variables Required for Docker

The Dockerfile requires these environment variables:

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `MONGO_CONNECTION_STRING` | **YES** | MongoDB connection string | `mongodb+srv://user:pass@cluster.mongodb.net/` |
| `Jwt__Key` | **RECOMMENDED** | JWT secret key (min 32 chars) - Override the default! | `your-secret-key-min-32-characters` |
| `Jwt__Issuer` | Optional | JWT issuer URL (has default) | `https://api.yourcompany.com` |
| `Jwt__Audience` | Optional | JWT audience URL (has default) | `https://yourcompany.com` |
| `Jwt__ExpiryMinutes` | Optional | Token expiration time (has default) | `480` |

**Important**: `appsettings.json` contains a default JWT key for development purposes. For production, you **MUST** override it with a secure random key using the `Jwt__Key` environment variable.

## Deployment Platforms

### Railway

1. Push your code to GitHub
2. Create new project in Railway
3. Railway auto-detects Dockerfile
4. Add environment variables in Railway dashboard:
   - `MONGO_CONNECTION_STRING`
   - `Jwt__Key`
   - `Jwt__Issuer`
   - `Jwt__Audience`

### Render

1. Create new Web Service
2. Connect your GitHub repository
3. Render auto-detects Dockerfile
4. Add environment variables:
   - `MONGO_CONNECTION_STRING`
   - All JWT variables

### AWS/Azure/GCP

Use their container services (ECS, Container Apps, Cloud Run) with the same environment variables.

## Security Notes

**The Dockerfile does NOT include**:
- ❌ `appsettings.Development.json` (excluded in `.dockerignore`)
- ❌ `appsettings.Production.json` (excluded in `.dockerignore`)
- ❌ Any hardcoded secrets

**Secrets are provided via**:
- ✅ Environment variables at runtime
- ✅ Platform-specific secret management (Railway Secrets, Render Environment Variables, etc.)

## Testing the Docker Image Locally

```bash
# Build the image
docker build -t henrys-tires-api .

# Run with your MongoDB Atlas connection
docker run -p 8080:8080 \
  -e MONGO_CONNECTION_STRING="mongodb+srv://YOUR_USER:YOUR_PASS@YOUR_CLUSTER.mongodb.net/" \
  -e Jwt__Key="dev-key-min-32-characters-long-testing" \
  -e Jwt__Issuer="http://localhost:8080" \
  -e Jwt__Audience="http://localhost:8080" \
  henrys-tires-api

# Test the API
curl http://localhost:8080/api/branches
```

## Troubleshooting

### "MongoDB connection string not configured"

This error means the `MONGO_CONNECTION_STRING` environment variable is not set.

**Solution**:
- For local Docker: Add `-e MONGO_CONNECTION_STRING="..."` to your `docker run` command
- For Docker Compose: Check the environment section in `docker-compose.yml`
- For cloud platforms: Add the environment variable in their dashboard

### "Port already in use"

Change the host port:
```bash
docker run -p 9090:8080 ...  # Use port 9090 instead of 8080
```

## What's Different from Before

**Before** (Insecure):
- `appsettings.json` had MongoDB connection string with password
- Secrets would be committed to GitHub

**Now** (Secure):
- Secrets are excluded from Docker image
- Secrets are provided via environment variables
- Safe to push to GitHub
- Safe to deploy anywhere
