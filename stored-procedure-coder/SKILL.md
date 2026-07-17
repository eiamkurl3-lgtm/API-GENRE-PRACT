---
name: stored-procedure-coder
description: Use when the user asks to create or convert to stored procedures for specific endpoints. Only applies stored procedures to the endpoints mentioned in the prompt — never touches other endpoints.
---

# stored-procedure-coder

This skill creates stored procedures and the corresponding table for a Minimal API project using Dapper. It only modifies the repository and SQL script the user explicitly mentions — never touches other repositories or files outside scope.

## Trigger Examples

- "create stored procedures for genre endpoints"
- "add stored procedures to user endpoints"
- "implement stored procedures for the movie endpoints"
- "set up stored procedures for genre and user endpoints"

## Workflow

Follow these steps **only for the endpoints mentioned in the user's prompt**:

### Step 1: Identify Target Endpoints

Parse the user prompt to determine which endpoint(s) to modify. For example, if the user says "create stored procedures for genre endpoints", only touch `GenreEndpoints.cs` and related Genre files.

### Step 2: Read the Entity

Read the corresponding entity file in `Entities/` to understand the properties that need mapping.

### Step 3: Check and Create Repository Interface

1. Check if an `I{Entity}Repository` exists in `Repositories/` folder.
2. If missing, create `Repositories/I{Entity}Repository.cs` with all 6 CRUD methods:

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

3. If the interface already exists, verify it has all necessary methods.

### Step 4: Create or Update Repository Implementation

1. Check if a `{Entity}Repository` exists in `Repositories/` folder.
2. If missing, create `Repositories/{Entity}Repository.cs` with all 6 CRUD methods using stored procedures.
3. If the repository already exists, verify all methods use stored procedures.

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
                new { /* entity properties excluding Id */ },
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
                    Id = {entity}.Id,
                    /* other properties */
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

### Step 5: Create SQL Script with Table and Stored Procedures

Create a SQL script file `{Entity}_StoredProcedures.sql` in the project root with the table creation and all stored procedures.

#### Stored Procedure Naming Convention
| Method | Stored Procedure Name |
|--------|----------------------|
| Create | `Create_{Entity}` |
| Exists | `{Entity}_IfExists` |
| GetAll | `{Entity}_GetAll` |
| GetById | `{Entity}_GetByID` |
| Update | `Update_{Entity}` |
| Delete | `{Entity}_Delete` |

#### SQL Script Template

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

### Step 6: Verify

Read back all modified files to confirm correctness. Check that:
- Repository interface has all 6 CRUD methods
- Repository implementation uses stored procedures for all methods
- SQL script includes table creation and all 6 stored procedures
- No other endpoints were modified

## Rules

1. **Scope lock** — Only modify files related to the endpoints mentioned in the prompt. If the user says "genre endpoints", do not touch `UserEndpoints.cs`, `UserRepository.cs`, or any User-related files.
2. **No unnecessary changes** — If a method already uses stored procedures, don't re-convert it.
3. **Preserve existing code** — Keep all existing logic (existence checks, error handling, return types) intact. Only change the data access layer.
4. **Convention** — Follow the stored procedure naming convention: `Create_{Entity}`, `{Entity}_IfExists`, `{Entity}_GetAll`, `{Entity}_GetByID`, `Update_{Entity}`, `{Entity}_Delete`.
5. **Parameter naming** — Use @ prefix for SQL parameters. Match C# property names in PascalCase for the anonymous objects passed to Dapper.
6. **SQL script** — Always generate a complete SQL script that includes table creation and all stored procedures.
7. **Table creation** — Always include `CREATE TABLE IF NOT EXISTS` at the top of the SQL script.

Base directory for this skill: C:\Users\Eiamk\projects\Skills\stored-procedure-coder
Relative paths in this skill (e.g., scripts/, reference/) are relative to this base directory.
