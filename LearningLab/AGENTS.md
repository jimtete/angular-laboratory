# Project Agent Instructions

## API Endpoint Error Handling

- Do not add `try`/`catch` blocks inside controller endpoints.
- Services and repositories must return an explicit application status code for expected outcomes.
- Controllers must map those status codes to HTTP responses with a `switch` expression or `switch` statement.
- Define custom application status codes in a dedicated shared file so distinct failure cases remain identifiable.
- Reserve exceptions for unexpected failures; handle them through centralized exception-handling middleware.

## Entity Framework Database Changes

- Do not run `dotnet ef database update`.
- Do not create or scaffold migrations unless the user explicitly asks for a migration.
- When a model change appears to need a migration, mention it to the user and let them decide when to create and apply it.
