# Project Agent Instructions

## API Endpoint Error Handling

- Do not add `try`/`catch` blocks inside controller endpoints.
- Services and repositories must return an explicit application status code for expected outcomes.
- Controllers must map those status codes to HTTP responses with a `switch` expression or `switch` statement.
- Define custom application status codes in a dedicated shared file so distinct failure cases remain identifiable.
- Reserve exceptions for unexpected failures; handle them through centralized exception-handling middleware.
