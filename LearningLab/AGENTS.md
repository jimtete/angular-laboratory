# Project Agent Instructions

## API Endpoint Error Handling

- Do not add `try`/`catch` blocks inside controller endpoints.
- Services and repositories must return an explicit application status code for expected outcomes.
- Controllers must map those status codes to HTTP responses with a `switch` expression or `switch` statement.
- Define custom application status codes in a dedicated shared file so distinct failure cases remain identifiable.
- Reserve exceptions for unexpected failures; handle them through centralized exception-handling middleware.

## Project Boundaries

- Put asset-specific backend concerns under the `LearningLab.Assets` project, including asset storage, upload handling, file validation, asset metadata, asset repositories, asset services, and asset DTOs.
- Put gameplay/domain behavior under the `LearningLab.Services` project, including campaign gameplay flows, session behavior, story/combat interactions, visibility rules, placement rules, and gameplay use of maps or other assets.
- Keep EF persistence models and DbContext mapping in `LearningLab.Data`.
- Keep API controllers in the `LearningLab` project unless the user explicitly asks to split the API surface differently.

## Entity Framework Database Changes

- Do not run `dotnet ef database update`.
- Do not create or scaffold migrations unless the user explicitly asks for a migration.
- When a model change appears to need a migration, mention it to the user and let them decide when to create and apply it.
- Treat every existing migration file as already applied and immutable.
- Never edit an existing migration file to fix a migration problem.
- If a migration needs to be corrected, create a new migration that reverses or adjusts the previous migration's effects.
