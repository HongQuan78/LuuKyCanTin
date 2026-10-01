# Seed data

There are two kinds of seed data, and they take different paths.

## Reference data: in migrations, always

Roles, permissions, document counters, units of measure and signer configuration are needed on every install. Seed them **inside an EF migration**:

- `HasData(...)` in the entity's configuration, when the rows are static and keyed by a fixed `Id`. EF then generates the inserts and later updates in migrations.
- `migrationBuilder.Sql(...)` in the migration, when the data depends on existing rows or needs `IDENTITY_INSERT` or a `MERGE`.

A fresh install and an upgrade then produce identical data. **Never** seed reference data from startup code. Workstations don't write at startup, and the least-privilege login can't.

## Demo data: `--seed-demo`, never in production

`Demo/DemoDataSeeder.cs` fills a database with sample detainees, vouchers and goods for development and training. It runs only from the admin command:

```powershell
LuuKyCanTin.WinForms.exe --migrate --seed-demo                    # Development environment
LuuKyCanTin.WinForms.exe --seed-demo --force=<DatabaseName>       # anywhere else, confirming the target database by name
```

Outside the `Development` environment the seeder refuses unless `--force=` names the database it is connected to (see `DemoSeedPolicy`).
