# .NET 5 fuori supporto e Npgsql 5.0.10 vulnerabile

- Commit: 3d2aa56
- Posizione: AcademyModel/AcademyModel.csproj, AcademyPersistance/AcademyEfPersistance.csproj, CodeAcademyWeb/CodeAcademyWeb.csproj

La soluzione usa net5.0, fuori supporto da maggio 2022 e senza patch di sicurezza. Npgsql 5.0.10, dipendenza transitiva di Npgsql.EntityFrameworkCore.PostgreSQL 5.0.10, ha una vulnerabilità di gravità alta (GHSA-x9vc-6hfv-hg8c).

Su Linux .NET 5 richiede inoltre OpenSSL 1.1 (pacchetto AUR `openssl-1.1`), fuori supporto da settembre 2023.
