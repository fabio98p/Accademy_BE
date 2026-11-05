# Versioni EF Core disallineate tra i progetti

- Commit: f13e521
- Posizione: AcademyPersistance/AcademyEfPersistance.csproj, CodeAcademyWeb/CodeAcademyWeb.csproj

`AcademyPersistance` riceve EF Core 10.0.4 come dipendenza transitiva di Npgsql 10.0.3, mentre `CodeAcademyWeb` usa la 10.0.14 tramite `Microsoft.EntityFrameworkCore.Design` (`PrivateAssets=all`). Chi referenzia `CodeAcademyWeb` riceve la 10.0.4 e fallisce con `FileNotFoundException` (warning MSB3277 in build). Il progetto di test lo aggira con un riferimento esplicito a `Microsoft.EntityFrameworkCore.Relational` 10.0.14.
