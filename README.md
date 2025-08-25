View working website in the About section
View the frontend at https://github.com/sachen-pather/water-quality-sa

# WaterQualityAPI Backend

A .NET Core Web API for managing beach water quality data and community discussions. This API serves beach water quality readings, community posts, and provides PDF parsing capabilities for water quality reports.

## Overview

The WaterQualityAPI provides endpoints for:
- Beach water quality monitoring
- Community discussions and posts
- PDF parsing for water quality reports
- Health monitoring and diagnostics

## Live API

**Production URL:** (https://papaya-yeot-5ff93c.netlify.app/)

### Quick Test Endpoints:
- Health Check: `/health`
- Beach Data: `/beach`
- API Documentation: `/swagger` (development only)
- Debug Info: `/debug/connection`

## Technology Stack

- **.NET 8.0** - Web API Framework
- **Entity Framework Core** - ORM with PostgreSQL provider
- **Supabase PostgreSQL** - Database hosting
- **Azure App Service** - Hosting platform
- **iText** - PDF parsing library
- **Dapper** - Micro ORM for optimized queries
- **Swagger/OpenAPI** - API documentation

## Database Schema

### Core Tables:
- **`beaches`** - Beach information and locations
- **`water_quality_readings`** - Enterococcus count measurements
- **`community_posts`** - User-generated beach reports
- **`community_discussions`** - General discussions
- **`community_comments`** - Discussion responses

## API Endpoints

### Beach Management
```http
GET    /beach                    # Get all beaches with latest readings
GET    /beach/{code}            # Get specific beach by code
POST   /beach                   # Create new beach
PUT    /beach/{code}            # Update beach information
DELETE /beach/{code}            # Delete beach

GET    /beach/{code}/readings   # Get all readings for a beach
```

### Water Quality
```http
GET    /api/waterquality                        # Get all readings
GET    /api/waterquality/beach/{beachCode}      # Get readings by beach
GET    /api/waterquality/beach/{beachCode}/latest # Get latest reading
POST   /api/waterquality                        # Add new reading
PUT    /api/waterquality/{id}                   # Update reading
DELETE /api/waterquality/{id}                   # Delete reading
```

### Community Features
```http
GET    /api/Community?beachCode={code}          # Get approved posts for beach
POST   /api/Community                           # Submit new post
GET    /api/Community/pending                   # Get posts awaiting moderation
PUT    /api/Community/{id}/approve              # Approve post
PUT    /api/Community/{id}/reject               # Reject post
```

### PDF Upload
```http
POST   /upload                                  # Upload and parse water quality PDF
```

### Health & Diagnostics
```http
GET    /health                                  # Health check with database status
GET    /debug/connection                        # Database connection info
```

## Data Models

### Beach
```json
{
  "id": 1,
  "code": "XCN08",
  "name": "Camps Bay Main Beach",
  "location": "Camps Bay, Cape Town",
  "latitude": -33.9555,
  "longitude": 18.3783,
  "createdAt": "2025-08-11T21:26:36.539Z",
  "waterQualityReadings": [...]
}
```

### Water Quality Reading
```json
{
  "id": 1,
  "beachCode": "XCN08",
  "samplingDate": "2025-08-08T00:00:00Z",
  "enterococcusCount": 78.0,
  "samplingFrequency": "Weekly",
  "isWithinSafetyThreshold": true
}
```

### Community Post
```json
{
  "id": 1,
  "beachCode": "XCN08",
  "content": "Great water quality today!",
  "status": "approved",
  "createdAt": "2025-08-11T12:00:00Z",
  "moderatedAt": "2025-08-11T14:00:00Z"
}
```

## Security & CORS

The API is configured with CORS to allow requests from:
- `http://localhost:5173` (Local development)
- `https://*.netlify.app` (Netlify deployments)
- `https://papaya-yeot-5ff93c.netlify.app` (Production frontend)

## Environment Variables

Required environment variables for deployment:

```env
SUPABASE_CONNECTION=Host=aws-0-eu-west-2.pooler.supabase.com;Database=postgres;Username=postgres.xxx;Password=xxx;Port=5432;SSL Mode=Require;Trust Server Certificate=true
ASPNETCORE_ENVIRONMENT=Production
```

## Deployment

### Current Deployment
- **Platform**: Azure App Service (Free F1 tier)
- **URL**: https://waterqualityapi20250812142739.azurewebsites.net
- **Auto-deploy**: Configured via Visual Studio publish profile

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/sachen-pather/WaterQualityAPI.git
   cd WaterQualityAPI
   ```

2. **Set up environment variables**
   ```bash
   # Update appsettings.Development.json with your Supabase connection
   ```

3. **Run the application**
   ```bash
   dotnet restore
   dotnet run
   ```

4. **Access the API**
   - API: http://localhost:5048
   - Swagger: http://localhost:5048/swagger

## Features

### PDF Processing
- Automated parsing of water quality reports
- Extracts beach codes, dates, and enterococcus counts
- Supports multiple date formats and beach code patterns
- Automatic safety threshold calculation (≤100 cfu/100ml)

### Data Management
- Automatic beach creation from uploaded data
- Duplicate prevention for readings
- Real-time safety status calculation
- Historical data tracking

### Community Features
- Post moderation system
- Beach-specific discussions
- Admin approval workflow
- Content filtering and management

### Health Monitoring
- Database connectivity checks
- Performance metrics
- Error logging and tracking
- Production diagnostics

## API Response Examples

### Get All Beaches
```json
[
  {
    "id": 1,
    "code": "XCN08",
    "name": "Camps Bay Main Beach",
    "location": "Camps Bay, Cape Town",
    "latitude": -33.9555,
    "longitude": 18.3783,
    "latestReading": {
      "samplingDate": "2025-08-08T00:00:00Z",
      "enterococcusCount": 78.0,
      "isWithinSafetyThreshold": true
    }
  }
]
```

### Health Check Response
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1584123",
  "entries": {
    "supabase": {
      "status": "Healthy",
      "description": "Successfully connected to Supabase PostgreSQL.",
      "data": {
        "PostgreSQLVersion": "PostgreSQL 15.8 on aarch64-unknown-linux-gnu"
      }
    }
  }
}
```

## Error Handling

The API returns consistent error responses:

```json
{
  "message": "An error occurred while processing the request",
  "error": "Detailed error information",
  "timestamp": "2025-08-12T12:34:56Z"
}
```

Common HTTP status codes:
- `200` - Success
- `400` - Bad Request (validation errors)
- `404` - Resource not found
- `500` - Internal server error

## Performance

- **Response time**: < 200ms for most endpoints
- **Database queries**: Optimized with indexes and eager loading
- **File uploads**: Streaming processing for large PDFs
- **Health checks**: Real-time database connectivity monitoring

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License.

