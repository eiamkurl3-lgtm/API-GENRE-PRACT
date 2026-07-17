---
name: entity-scaffold
description: Use when the user asks to create a new entity from scratch. Builds the full stack: entity, repository with stored procedures, DTOs, AutoMapper profile, CRUD endpoints, and DI registration. Orchestrates the automapper-coder and stored-procedure-coder patterns internally.
---

# entity-scaffold

This skill scaffolds an entire new entity from scratch for a Minimal API project using Dapper, stored procedures, and AutoMapper. It creates all layers in one shot — entity, repository, DTOs, mapper, endpoints, SQL script, and DI registration.

## Trigger Examples

- "scaffold a new entity called Movie"
- "create a full entity for Product with Name, Price decimal, IsAvailable bool"
- "build out the Director entity with FirstName, LastName, BirthDate"
- "add a new entity called Review with Rating int, Comment string, MovieId int, UserId int"

## Workflow

Follow these steps in order for the entity mentioned in the user's prompt:

### Step 1: Parse User Input

Extract from the prompt:
1. **Entity name** (e.g., `Movie`, `Product`, `Director`)
2. **Properties** with their C# types (e.g., `Title string`, `ReleaseDate DateTime`, `Price decimal`)

If properties are not provided, ask the user what properties the entity should have before proceeding.

**Important naming conventions:**
- The entity class uses singular form: `Movie`, `Product`, `Director`
- The table name uses the same or plural form (user decides): `Movies`, `Products`, `Directors`
- The SQL parameter table name in stored procedures matches the actual table name

### Step 2: Create Entity

Create `Entities/{Entity}.cs`:

```csharp
namespace MinimalApiMovies.Entities
{
    public class {Entity}
    {
        public int Id { get; set; }

        // user-provided properties with their types
        public string Title { get; set; } = null!;
        public DateTime ReleaseDate { get; set; }
        // etc.
    }
}
```

**Rules:**
- Always include `public int Id { get; set; }` as the first property
- Use `= null!;` for string properties to suppress nullable warnings
- Use the exact types the user specified

### Step 3: Create Repository Interface

Create `Repositories/I{Entity}Repository.cs`:

```csharp
using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Repositories
{
    public interface I{Entity}Repository
    {
        Task<int> Create({Entity} {entity});
        Task<List<{Entity}>> GetAll();
        Task<{Entity}?> GetById(int id);
        Task<bool> Exists(int id);
        Task Update({Entity} {entity});
        Task Delete(int id);
    }
}
```

**Rules:**
- Always include all 6 CRUD methods
- Use `{entity}` (camelCase) for parameter names

### Step 4: Create Repository Implementation

Create `Repositories/{Entity}Repository.cs` using stored procedures (following the stored-procedure-coder pattern):

```csharp
using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;
using System.Data;

namespace MinimalApiMovies.Repositories
{
    public class {Entity}Repository : I{Entity}Repository
    {
        private readonly string connectionString;

        public {Entity}Repository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> Create({Entity} {entity})
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>(
                "Create_{Entity}",
                new
                {
                    // all properties except Id
                },
                commandType: CommandType.StoredProcedure
            );

            {entity}.Id = id;
            return id;
        }

        public async Task<bool> Exists(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var exists = await connection.QuerySingleAsync<bool>(
                "{Entity}_IfExists",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return exists;
        }

        public async Task<List<{Entity}>> GetAll()
        {
            using var connection = new SqlConnection(connectionString);

            var {entities} = await connection.QueryAsync<{Entity}>(
                "{Entity}_GetAll",
                commandType: CommandType.StoredProcedure
            );

            return {entities}.ToList();
        }

        public async Task<{Entity}?> GetById(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var {entity} = await connection.QueryFirstOrDefaultAsync<{Entity}>(
                "{Entity}_GetByID",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            return {entity};
        }

        public async Task Update({Entity} {entity})
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(
                "Update_{Entity}",
                new
                {
                    {entity}.Id,
                    // all properties except Id
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(
                "{Entity}_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
```

**Rules:**
- Always use `using var connection = new SqlConnection(connectionString);`
- All methods use `commandType: CommandType.StoredProcedure`
- Create method uses `QuerySingleAsync<int>` with `SCOPE_IDENTITY()`
- GetAll uses `QueryAsync<{Entity}>`
- GetById uses `QueryFirstOrDefaultAsync<{Entity}>`
- Update and Delete use `ExecuteAsync`
- Include `using System.Data;` for `CommandType`

### Step 5: Generate SQL Script with Table and Stored Procedures

Create `{Entity}_StoredProcedures.sql` in the project root with the table creation and all stored procedures:

```sql
-- =============================================
-- Table and Stored Procedures for {Entity}
-- =============================================

-- Create Table if not exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{Entities}' AND xtype='U')
CREATE TABLE {Entities} (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Prop1 NVARCHAR(100) NOT NULL,
    Prop2 INT,
    -- other columns based on entity properties
);
GO

-- Create_{Entity}
CREATE PROCEDURE Create_{Entity}
    @Prop1 NVARCHAR(100),
    @Prop2 INT,
    -- other parameters (exclude Id - it's identity)
AS
BEGIN
    INSERT INTO {Entities} (Prop1, Prop2, ...)
    VALUES (@Prop1, @Prop2, ...);

    SELECT SCOPE_IDENTITY();
END;
GO

-- {Entity}_IfExists
CREATE PROCEDURE {Entity}_IfExists
    @Id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM {Entities} WHERE Id = @Id)
        SELECT 1;
    ELSE
        SELECT 0;
END;
GO

-- {Entity}_GetAll
CREATE PROCEDURE {Entity}_GetAll
AS
BEGIN
    SELECT * FROM {Entities} ORDER BY {PrimarySortColumn};
END;
GO

-- {Entity}_GetByID
CREATE PROCEDURE {Entity}_GetByID
    @Id INT
AS
BEGIN
    SELECT * FROM {Entities} WHERE Id = @Id;
END;
GO

-- Update_{Entity}
CREATE PROCEDURE Update_{Entity}
    @Id INT,
    @Prop1 NVARCHAR(100),
    @Prop2 INT,
    -- other parameters
AS
BEGIN
    UPDATE {Entities}
    SET
        Prop1 = @Prop1,
        Prop2 = @Prop2,
        -- other columns
    WHERE Id = @Id;
END;
GO

-- {Entity}_Delete
CREATE PROCEDURE {Entity}_Delete
    @Id INT
AS
BEGIN
    DELETE FROM {Entities} WHERE Id = @Id;
END;
GO
```

**SQL type mapping:**
| C# Type | SQL Type |
|---------|----------|
| `string` | `NVARCHAR(100)` or `NVARCHAR(500)` for longer text like URLs |
| `int` | `INT` |
| `long` | `BIGINT` |
| `decimal` | `DECIMAL(18, 2)` |
| `double` | `FLOAT` |
| `bool` | `BIT` |
| `DateTime` | `DATE` or `DATETIME` |
| `Guid` | `UNIQUEIDENTIFIER` |

**Rules:**
- Use `{Entities}` (plural or same as entity) for table name — match what the user specifies or what's consistent with existing tables
- `{PrimarySortColumn}` should be the most logical sort column (usually the first string property or Name)
- Exclude `Id` from INSERT and UPDATE parameters (it's identity for create, and WHERE clause for update)
- Always include `CREATE TABLE IF NOT EXISTS` at the top of the SQL script
- Use `IDENTITY(1,1)` for the `Id` column to auto-increment

### Step 6: Create DTOs

**A. Create Request DTO (`Create{Entity}DTO`)**

Create `DTOs/Create{Entity}DTO.cs`:

```csharp
namespace MinimalApiMovies.DTOs
{
    public class Create{Entity}DTO
    {
        // all properties except Id
        public string Title { get; set; } = null!;
        public DateTime ReleaseDate { get; set; }
        // etc.
    }
}
```

**B. Create Response DTO (`{Entity}DTO`)**

Create `DTOs/{Entity}DTO.cs`:

```csharp
namespace MinimalApiMovies.DTOs
{
    public class {Entity}DTO
    {
        public int Id { get; set; }

        // all properties (same as entity)
        public string Title { get; set; } = null!;
        public DateTime ReleaseDate { get; set; }
        // etc.
    }
}
```

**Rules:**
- `Create{Entity}DTO` — Never include `Id` (it's auto-generated), only what client sends
- `{Entity}DTO` — Include `Id` and all properties (what API returns)
- Use the same types as the entity for both DTOs
- Two DTOs give you control over what data is exposed to the client

### Step 7: Create AutoMapper Profile

Create `Mappers/{Entity}Profile.cs`:

```csharp
using AutoMapper;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Mappers
{
    public class {Entity}Profile : Profile
    {
        public {Entity}Profile()
        {
            CreateMap<Create{Entity}DTO, {Entity}>();
            CreateMap<{Entity}, {Entity}DTO>();
        }
    }
}
```

**Rules:**
- One profile class per entity
- Named `{Entity}Profile.cs` in the `Mappers/` folder
- Must include both mappings:
  - `Create{Entity}DTO -> {Entity}` (for Create/Update requests)
  - `{Entity} -> {Entity}DTO` (for responses)

### Step 8: Create Endpoints

Create `Endpoints/{Entity}Endpoints.cs` with full CRUD. Endpoints must use DTOs for both requests and responses:

```csharp
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.Endpoints
{
    public static class {Entity}Endpoints
    {
        public static RouteGroupBuilder Map{Entity}Endpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAll{Entities});
            group.MapGet("/{id:int}", Get{Entity}ById);
            group.MapPost("/", Create{Entity});
            group.MapPut("/{id:int}", Update{Entity});
            group.MapDelete("/{id:int}", Delete{Entity});
            return group;
        }

        static async Task<Ok<List<{Entity}DTO>>> GetAll{Entities}(I{Entity}Repository repository, IMapper mapper)
        {
            var {entities} = await repository.GetAll();
            var {entityDTOs} = mapper.Map<List<{Entity}DTO>>({entities});
            return TypedResults.Ok({entityDTOs});
        }

        static async Task<Results<Ok<{Entity}DTO>, NotFound>> Get{Entity}ById(int id, I{Entity}Repository repository, IMapper mapper)
        {
            var {entity} = await repository.GetById(id);
            if ({entity} == null)
            {
                return TypedResults.NotFound();
            }
            var {entity}DTO = mapper.Map<{Entity}DTO>({entity});
            return TypedResults.Ok({entity}DTO);
        }

        static async Task<Results<Created<{Entity}DTO>, NotFound>> Create{Entity}(Create{Entity}DTO create{Entity}DTO, I{Entity}Repository repository, IMapper mapper)
        {
            var {entity} = mapper.Map<{Entity}>(create{Entity}DTO);
            var id = await repository.Create({entity});
            {entity}.Id = id;
            var {entity}DTO = mapper.Map<{Entity}DTO>({entity});
            return TypedResults.Created($"/{Entity}/{id}", {entity}DTO);
        }

        static async Task<Results<Ok, NotFound>> Update{Entity}(int id, Create{Entity}DTO update{Entity}DTO, I{Entity}Repository repository, IMapper mapper)
        {
            var exists = await repository.Exists(id);
            if (!exists)
            {
                return TypedResults.NotFound();
            }

            var {entity} = mapper.Map<{Entity}>(update{Entity}DTO);
            {entity}.Id = id;

            await repository.Update({entity});
            return TypedResults.Ok();
        }

        static async Task<Results<Ok, NotFound>> Delete{Entity}(int id, I{Entity}Repository repository)
        {
            var exists = await repository.Exists(id);
            if (!exists)
            {
                return TypedResults.NotFound();
            }
            await repository.Delete(id);
            return TypedResults.Ok();
        }
    }
}
```

**Rules:**
- GetAll returns `Ok<List<{Entity}DTO>>` (not raw entity)
- GetById returns `Results<Ok<{Entity}DTO>, NotFound>` (not raw entity)
- Create returns `Created<{Entity}DTO>` with mapped response DTO
- Create and Update use `IMapper mapper` and accept `Create{Entity}DTO`
- Update and Delete check existence first
- Endpoints must return DTOs, not raw entities — this controls what data is exposed

### Step 9: Register DI and Route in Program.cs

1. Read `Program.cs`
2. Add DI registration in the Services zone (after existing registrations):
   ```csharp
   builder.Services.AddScoped<I{Entity}Repository, {Entity}Repository>();
   ```
3. Add route mapping in the Middlewares zone (after existing route groups):
   ```csharp
   app.MapGroup("/{Entity}").Map{Entity}Endpoints();
   ```

**Rules:**
- Check if `I{Entity}Repository` is already registered — if so, skip
- Preserve all existing code in Program.cs
- Only append — never modify existing registrations or routes
- Place new DI registration after the last existing `AddScoped` line
- Place new route mapping after the last existing `MapGroup` line

### Step 10: Verify

Read back all created/modified files and confirm:
- Entity has correct properties with correct types
- Repository interface has all 6 CRUD methods
- Repository implementation uses stored procedures for all methods
- SQL script includes table creation and all 6 stored procedures
- Both DTOs exist (`Create{Entity}DTO` and `{Entity}DTO`)
- Profile has both mappings (`Create{Entity}DTO -> {Entity}` and `{Entity} -> {Entity}DTO`)
- Endpoints have all 5 CRUD operations and return DTOs (not raw entities)
- Program.cs has DI registration and route mapping
- No other entities/files were modified

## Rules

1. **Scope lock** — Only create/modify files for the entity mentioned in the prompt. Never touch existing entities, repositories, endpoints, or other files.
2. **No unnecessary changes** — If AutoMapper is already registered, don't re-add it. If a file already exists, don't recreate it.
3. **Preserve existing code** — Keep all existing logic intact. Only append new code to Program.cs.
4. **DTO convention** — Two DTOs per entity:
   - `Create{Entity}DTO` — Request DTO (no `Id`, only what client sends)
   - `{Entity}DTO` — Response DTO (includes `Id`, what API returns)
5. **Profile convention** — One profile class per entity, named `{Entity}Profile.cs` in the `Mappers/` folder. Must include both mappings.
6. **Stored procedure convention** — Follow naming: `Create_{Entity}`, `{Entity}_IfExists`, `{Entity}_GetAll`, `{Entity}_GetByID`, `Update_{Entity}`, `{Entity}_Delete`.
7. **Parameter naming** — Use @ prefix for SQL parameters. Match C# property names in PascalCase for anonymous objects passed to Dapper.
8. **SQL script** — Always generate a complete SQL script that includes table creation and all stored procedures.
9. **Namespace** — All files use `MinimalApiMovies` namespace (matching the existing project).
10. **Connection string** — Always use `configuration.GetConnectionString("DefaultConnection")!` in repository constructors.
11. **Response mapping** — Endpoints must return DTOs, not raw entities. This controls what data is exposed to the client.

Base directory for this skill: C:\Users\Eiamk\projects\Skills\entity-scaffold
Relative paths in this skill (e.g., scripts/, reference/) are relative to this base directory.
