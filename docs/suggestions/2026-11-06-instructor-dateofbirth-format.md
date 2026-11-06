# Instructor date of birth in culture-dependent format

- Commit: 1be95e2
- Posizione: CodeAcademyWeb/Profiles/InstructorProfile.cs

`InstructorProfile` has no explicit mapping for `DateOfBirth` (`LocalDate` → `string`), so AutoMapper uses `LocalDate.ToString()`, which returns the culture's long date (e.g. `Saturday, March 15, 1980`) instead of `yyyy-MM-dd` like the other DTOs. The reverse map `InstructorDTO` → `Instructor` has no conversion from `string` to `LocalDate` either.
