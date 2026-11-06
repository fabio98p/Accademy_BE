# Edition update does not check that the edition exists

- Commit: 5ee7be0
- Posizione: AcademyPersistance/Services/EFEditionsService.cs:54

In `EditCourseEdition` the `CheckCourseEdition(e.Id)` call is commented out, so the service never throws `EntityNotFoundException` for a missing edition. A `PUT /api/Edition` with a non-existent id reaches `editionRepo.Update` and `SaveChanges`, and the 404 branch in `EditionController.Edit` is never reached.
