# HdbApi Copilot Instructions

## Project Overview
This is the HDB (Hydrologic Database) Data Services API - a RESTful Web API providing access to hydrological data from Oracle databases. The API serves as the main interface for HDB data services used by the US Bureau of Reclamation.

**Key Architecture:**
- ASP.NET Web API 2 (.NET Framework 4.7) with OWIN hosting
- Repository pattern with Dapper ORM for data access
- Oracle database backend with stored procedures
- Authentication via HTTP headers (`api_hdb`, `api_user`, `api_pass`)
- Swagger/OpenAPI documentation

## Core Components
- **Controllers/**: REST endpoints for Sites, Series, DataTypes, ModelRuns, etc.
- **DataAccessLayer/**: Repository classes with Dapper queries
- **Models/**: Data transfer objects (DTOs) for API responses
- **App_Code/**: Database connection logic and stored procedure wrappers
- **Tests/**: NUnit integration tests against live database

## Critical Patterns & Conventions

### Database Connection
All API endpoints require authentication headers:
```
api_hdb: [HDB instance, e.g., "LCHDB2"]
api_user: [username]
api_pass: [password]
```

Connection established in controllers via `HdbController.Connect(this.Request.Headers)`.

### Repository Pattern
- Controllers instantiate repositories directly (no DI injection)
- Repositories use `IDbConnection` parameter passed from controller
- Raw SQL strings with Dapper's `Query<T>()` method
- Example: `db.Query<SiteModel.HdbSite>(sqlString)`

### Data Modification
Uses stored procedures for inserts/updates:
- `MODIFY_R_BASE_RAW` for time series data
- `MODIFY_M_TABLE_RAW` for model data
- `DELETE_FROM_HDB` for deletions

### Error Handling
- Controllers catch exceptions but often swallow them
- Database connections manually closed/disposed in finally blocks
- No global exception handling middleware

### Testing
- NUnit tests connect to "lchdb" test database
- Tests validate specific data values (not mocks)
- Repository methods tested directly with hardcoded assertions

## Development Workflows

### Building
```bash
# .NET Framework build
msbuild HdbApi.csproj /p:Configuration=Debug
# Or via Visual Studio
```

### Running Locally
- IIS Express hosting (configured in .csproj)
- Swagger UI available at `/HdbApi/index`
- Requires Oracle client libraries for database access

### Testing
```bash
# Run NUnit tests
nunit3-console Tests/ApiReadTests.dll
# Tests require database connectivity
```

### Database Setup
- Multiple HDB instances: LCHDB2, UCHDB2, UCHDBT, etc.
- Special users: `app_user` gets connection pooling
- DBA users get enhanced pooling settings

## Key Files to Reference
- `Controllers/HdbsController.cs`: Connection logic and HDB instance listing
- `DataAccessLayer/SiteRepository.cs`: Example repository implementation
- `App_Code/HdbStoredProcs.cs`: Stored procedure wrappers
- `Startup.cs`: OWIN configuration
- `Tests/ApiReadTests.cs`: Testing patterns

## Migration Context
This codebase is currently migrating from .NET Framework to .NET Core (see `migrate-to-core` branch). New code should consider .NET Core compatibility.

## Common Gotchas
- Database connections not pooled by default (except for `app_user`)
- Raw SQL injection risks in some queries
- No async/await patterns (synchronous database calls)
- Controllers mix business logic with HTTP concerns
- CSV catalog files (`hydromet*.csv`) used for metadata caching</content>
<parameter name="filePath">/home/agilmore/workspace/HdbApi/.github/copilot-instructions.md