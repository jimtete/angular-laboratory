namespace LearningLab.Data.Models;

public sealed record ServiceResult<T>(
    ApplicationStatusCode StatusCode,
    T? Data = default);
