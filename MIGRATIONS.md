# Database Migrations Guide

## Overview

This project uses **Entity Framework Core Migrations** to manage SQLite database schema. Migrations allow controlled schema evolution without data loss during application updates.

## Prerequisites

- .NET SDK 10.0 or higher
- EF Core Tools (automatically included via `Microsoft.EntityFrameworkCore.Design`)

## Install EF Core Tools (if not installed)

```bash
dotnet tool install --global dotnet-ef
```

Verify installation:
```bash
dotnet ef --version
```

## Initial Setup

### 1. Remove Old Database (if exists)

**Windows:**
```powershell
Remove-Item "$env:LOCALAPPDATA\User Name\com.companyname.youspent\Data\youspent.db3" -Force
```

**Android (via adb):**
```bash
adb shell run-as com.companyname.youspent rm /data/data/com.companyname.youspent/files/youspent.db3
```

### 2. Create Initial Migration

```bash
cd C:\Users\Admin\source\repos\YouSpent
dotnet ef migrations add InitialCreate --project YouSpent.csproj
```

This creates three files in `Migrations/`:
- `YYYYMMDDHHMMSS_InitialCreate.cs` - migration code
- `YYYYMMDDHHMMSS_InitialCreate.Designer.cs` - metadata
- `AppDbContextModelSnapshot.cs` - current schema snapshot

## Applying Migrations

### Automatic (on app startup)

Migrations are applied automatically in `MauiProgram.cs`:
```csharp
await dbService.InitializeAsync(); // calls MigrateAsync()
```

### Manual via CLI

```bash
dotnet ef database update --project YouSpent.csproj
```

## Creating New Migrations

When you modify models in `Models/`:

### 1. Update or Add Model Properties

Example - adding a field to `ExpenseType`:

```csharp
public class ExpenseType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? Description { get; set; } // NEW FIELD
}
```

### 2. Generate Migration

```bash
dotnet ef migrations add AddDescriptionToExpenseType --project YouSpent.csproj
```

**Migration Naming Conventions:**
- Use PascalCase
- Start with action verb: `Add`, `Update`, `Remove`, `Rename`
- Be descriptive: `AddDescriptionToExpenseType` not just `UpdateModel`

### 3. Review Generated Migration

Check `Migrations/YYYYMMDDHHMMSS_AddDescriptionToExpenseType.cs`:

```csharp
public partial class AddDescriptionToExpenseType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "ExpenseTypes",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Description",
            table: "ExpenseTypes");
    }
}
```

### 4. Apply Migration

The application automatically applies migrations on next startup.

Manual application:
```bash
dotnet ef database update --project YouSpent.csproj
```

## Rolling Back Migrations

### Rollback to Specific Migration

```bash
dotnet ef database update PreviousMigrationName --project YouSpent.csproj
```

### Rollback All Migrations

```bash
dotnet ef database update 0 --project YouSpent.csproj
```

### Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove --project YouSpent.csproj
```

## Migration Information and Diagnostics

### List All Migrations

```bash
dotnet ef migrations list --project YouSpent.csproj
```

### Check Applied Migrations Programmatically

```csharp
var dbService = serviceProvider.GetRequiredService<DatabaseService>();
var applied = await dbService.GetAppliedMigrationsAsync();
foreach (var migration in applied)
{
    Console.WriteLine($"Applied: {migration}");
}
```

### Check Pending Migrations

```csharp
var pending = await dbService.GetPendingMigrationsAsync();
foreach (var migration in pending)
{
    Console.WriteLine($"Pending: {migration}");
}
```

## Generate SQL Scripts

### Generate SQL for All Migrations

```bash
dotnet ef migrations script --project YouSpent.csproj --output migration.sql
```

### Generate SQL for Specific Range

```bash
dotnet ef migrations script FromMigration ToMigration --project YouSpent.csproj
```

## Database Reset (Development)

### Option 1: Clear Data, Keep Schema

```csharp
var dbService = serviceProvider.GetRequiredService<DatabaseService>();
await dbService.ClearAllDataAsync(); // clears data, keeps tables
```

### Option 2: Delete Database File

Delete `youspent.db3` from application data directory.

### Option 3: Drop and Recreate via EF

```bash
dotnet ef database drop --project YouSpent.csproj --force
dotnet ef database update --project YouSpent.csproj
```

## Common Migration Scenarios

### Adding New Field

```bash
# 1. Add property to model
# 2. Generate migration
dotnet ef migrations add AddFieldToModel --project YouSpent.csproj

# 3. Restart app (migration applies automatically)
```

### Renaming Field

```bash
# 1. Generate migration
dotnet ef migrations add RenameField --project YouSpent.csproj

# 2. Edit migration to use RenameColumn instead of Drop+Add
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "OldName",
        table: "TableName",
        newName: "NewName");
}
```

### Adding New Table

```bash
# 1. Create new model in Models/
# 2. Add DbSet to AppDbContext
# 3. Generate migration
dotnet ef migrations add AddNewTable --project YouSpent.csproj
```

### Removing Table

```bash
# 1. Remove model and DbSet
# 2. Generate migration
dotnet ef migrations add RemoveOldTable --project YouSpent.csproj
```

## Configuring AppDbContext

File `Data/AppDbContext.cs` contains table configurations:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configure ExpenseType entity
    modelBuilder.Entity<ExpenseType>(entity =>
    {
        entity.HasKey(et => et.Id);
        entity.Property(et => et.Name).IsRequired().HasMaxLength(100);
        entity.Property(et => et.IsActive).IsRequired();
        entity.Property(et => et.CreatedAt).IsRequired();
        entity.HasIndex(et => et.Name).IsUnique(); // Example: unique names
    });
}
```

## Seed Data

Default data seeding is implemented in `DatabaseService.SeedDefaultExpenseTypesAsync()`:

```csharp
private async Task SeedDefaultExpenseTypesAsync()
{
    if (!await _context.ExpenseTypes.AnyAsync())
    {
        var defaultTypes = new[]
        {
            new ExpenseType { Name = "Food", IsActive = true },
            // ... other types
        };
        
        await _context.ExpenseTypes.AddRangeAsync(defaultTypes);
        await _context.SaveChangesAsync();
    }
}
```

## Troubleshooting

### Error: "No migrations configuration type was found"

**Solution:**
```bash
dotnet restore
dotnet build
```

### Error: "SQLite Error: no such table"

**Cause:** Database was created using old method (`EnsureCreated`).

**Solution:**
1. Delete database file
2. Generate initial migration
3. Restart application

### Migration Fails to Apply

**Enable verbose logging:**
```bash
dotnet ef database update --project YouSpent.csproj --verbose
```

### Start Fresh

**Clean slate approach:**
```bash
# Delete Migrations/ folder
# Delete database file
# Create new initial migration
dotnet ef migrations add InitialCreate --project YouSpent.csproj
```

## Production Deployment

### Generate Idempotent SQL Script

```bash
dotnet ef migrations script --idempotent --project YouSpent.csproj --output deploy.sql
```

Flag `--idempotent` makes script safe for multiple runs.

### Auto-Migration in Production

Code in `DatabaseService.InitializeAsync()` automatically applies migrations:

```csharp
await _context.Database.MigrateAsync(); // safe for production
```

## Best Practices

1. ? **Always create migration** when changing schema
2. ? **Test migrations** before deployment
3. ? **Use descriptive names** for migrations
4. ? **Implement rollback** (`Down` method) carefully
5. ? **Commit migrations** to Git along with code changes
6. ? **Never edit applied migrations** - create new one instead
7. ? **Never use `EnsureCreated`** in production
8. ? **Don't delete migrations** from history

## Additional Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [EF Core CLI Reference](https://learn.microsoft.com/ef/core/cli/dotnet)
- [SQLite with EF Core](https://learn.microsoft.com/ef/core/providers/sqlite/)

## Quick Reference Commands

```bash
# List migrations
dotnet ef migrations list --project YouSpent.csproj

# Add migration
dotnet ef migrations add MigrationName --project YouSpent.csproj

# Apply migrations
dotnet ef database update --project YouSpent.csproj

# Rollback to specific migration
dotnet ef database update MigrationName --project YouSpent.csproj

# Remove last migration (if not applied)
dotnet ef migrations remove --project YouSpent.csproj

# Generate SQL script
dotnet ef migrations script --project YouSpent.csproj --output script.sql

# Drop database
dotnet ef database drop --project YouSpent.csproj --force
```
