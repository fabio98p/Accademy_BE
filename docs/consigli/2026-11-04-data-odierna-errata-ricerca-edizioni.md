# Data odierna errata nella ricerca edizioni

- Commit: caf09b7
- Posizione: AcademyPersistance/Repository/EFEditionRepository.cs:33

`LocalDate.FromDateTime(new DateTime())` restituisce 0001-01-01 invece della data odierna, quindi i filtri `InTheFuture`/`InThePast` di `Search` non funzionano come previsto.
