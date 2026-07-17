---
name: automapper-coder
description: Use when the user asks to create or implement AutoMapper for specific endpoints. Only applies AutoMapper to the endpoints mentioned in the prompt — never touches other endpoints.
---

# automapper-coder

This skill implements AutoMapper for specific Minimal API endpoints. It only modifies the endpoints the user explicitly mentions — never touches other endpoints or files outside scope.

## Trigger Examples

- "create mapper for genre endpoints"
- "add automapper to user endpoints"
- "implement automapper for the movie endpoints"
- "set up mapping for genre and user endpoints"

## Workflow

Follow these steps **only for the endpoints mentioned in the user's prompt**:

### Step 1: Identify Target Endpoints

Parse the user prompt to determine which endpoint(s) to modify. For example, if the user says "create mapper for genre endpoints", only touch `GenreEndpoints.cs` and related Genre files.

### Step 2: Read the Entity

Read the corresponding entity file in `Entities/` to understand the properties that need mapping.

### Step 3: Check and Create DTOs

**A. Create Request DTO (`Create{Entity}DTO`)**

1. Check if a `Create{Entity}DTO` exists in `DTOs/` folder.
2. If missing, create `DTOs/Create{Entity}DTO.cs` with only the properties the client should send (exclude `Id` and any auto-generated fields).
3. If the DTO already exists, verify it has all necessary properties.

**B. Create Response DTO (`{Entity}DTO`)**

1. Check if a `{Entity}DTO` exists in `DTOs/` folder.
2. If missing, create `DTOs/{Entity}DTO.cs` with `Id` and all properties that should be returned to the client.
3. If the DTO already exists, verify it has all necessary properties.

**Purpose of having two DTOs:**
- `Create{Entity}DTO` — Used for Create/Update requests (what the client sends, no `Id`)
- `{Entity}DTO` — Used for responses (what the API returns, includes `Id`)

This gives you control over what data is exposed to the client and allows adding computed fields or excluding sensitive data later.

### Step 4: Create or Update the Profile

1. Check if a profile exists in `Mappers/` folder (e.g., `Mappers/GenreProfile.cs`).
2. If missing, create `Mappers/{Entity}Profile.cs`:

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

3. If the profile already exists, add any new mappings needed.

**Two mappings are required:**
- `Create{Entity}DTO -> {Entity}` — For Create/Update (request to entity)
- `{Entity} -> {Entity}DTO` — For Responses (entity to response DTO)

### Step 5: Register AutoMapper in Program.cs

Check `Program.cs` for `builder.Services.AddAutoMapper(typeof(Program));`. If missing, add it in the Services zone. If it already exists, skip this step.

### Step 6: Update Endpoint Methods

Only modify the target endpoint file:

1. Add `using AutoMapper;` to the file imports if not present.
2. Add `using MinimalApiMovies.DTOs;` to the file imports if not present.

#### Update Create Method:
- Replace entity parameter with `Create{Entity}DTO` parameter
- Add `IMapper mapper` parameter
- Replace manual mapping with `mapper.Map<{Entity}>(dto)`
- Set `Id` separately if needed (for response)
- Map entity to response DTO for return: `mapper.Map<{Entity}DTO>(entity)`

```csharp
static async Task<Results<Created<{Entity}DTO>, NotFound>> Create{Entity}(
    Create{Entity}DTO create{Entity}DTO, I{Entity}Repository repository, IMapper mapper)
{
    var {entity} = mapper.Map<{Entity}>(create{Entity}DTO);
    var id = await repository.Create({entity});
    {entity}.Id = id;
    var {entity}DTO = mapper.Map<{Entity}DTO>({entity});
    return TypedResults.Created($"/{Entity}/{id}", {entity}DTO);
}
```

#### Update Update Method:
- Replace entity parameter with `Create{Entity}DTO` parameter
- Add `IMapper mapper` parameter
- Replace manual mapping with `mapper.Map<{Entity}>(dto)`
- Set `Id` from route parameter

#### Update GetAll Method:
- Change return type from `Ok<List<{Entity}>>` to `Ok<List<{Entity}DTO>>`
- Map entities to DTOs: `mapper.Map<List<{Entity}DTO>>({entities})`

```csharp
static async Task<Ok<List<{Entity}DTO>>> GetAll{Entities}(I{Entity}Repository repository, IMapper mapper)
{
    var {entities} = await repository.GetAll();
    var {entityDTOs} = mapper.Map<List<{Entity}DTO>>({entities});
    return TypedResults.Ok({entityDTOs});
}
```

#### Update GetById Method:
- Change return type from `Ok<{Entity}>` to `Ok<{Entity}DTO>`
- Map entity to DTO: `mapper.Map<{Entity}DTO>({entity})`

```csharp
static async Task<Results<Ok<{Entity}DTO>, NotFound>> Get{Entity}ById(
    int id, I{Entity}Repository repository, IMapper mapper)
{
    var {entity} = await repository.GetById(id);
    if ({entity} == null)
    {
        return TypedResults.NotFound();
    }
    var {entity}DTO = mapper.Map<{Entity}DTO>({entity});
    return TypedResults.Ok({entity}DTO);
}
```

### Step 7: Verify

Read back all modified files to confirm correctness. Check that:
- Both DTOs exist (`Create{Entity}DTO` and `{Entity}DTO`)
- Profile has both mappings (`Create{Entity}DTO -> {Entity}` and `{Entity} -> {Entity}DTO`)
- Endpoint methods use `IMapper` and accept/return DTOs
- GetAll and GetById return `{Entity}DTO`/`List<{Entity}DTO>` (not raw entity)
- Create returns `Created<{Entity}DTO>`
- AutoMapper is registered in `Program.cs`
- No other endpoints were modified

## Rules

1. **Scope lock** — Only modify files related to the endpoints mentioned in the prompt. If the user says "genre endpoints", do not touch `UserEndpoints.cs`, `UserRepository.cs`, or any User-related files.
2. **No unnecessary changes** — If AutoMapper is already registered, don't re-add it. If a profile already exists, update it rather than recreating.
3. **Preserve existing code** — Keep all existing logic (existence checks, error handling, return types) intact. Only change the mapping-related parts.
4. **DTO convention** — Two DTOs per entity:
   - `Create{Entity}DTO` — Request DTO (no `Id`, only what client sends)
   - `{Entity}DTO` — Response DTO (includes `Id`, what API returns)
5. **Profile convention** — One profile class per entity, named `{Entity}Profile.cs` in the `Mappers/` folder. Must include both mappings.
6. **Response mapping** — Endpoints must return DTOs, not raw entities. This controls what data is exposed to the client.

Base directory for this skill: C:\Users\Eiamk\projects\Skills\automapper-coder
Relative paths in this skill (e.g., scripts/, reference/) are relative to this base directory.
