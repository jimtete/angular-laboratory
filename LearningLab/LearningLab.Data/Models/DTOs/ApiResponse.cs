namespace LearningLab.Data.Models.DTOs;

public class ApiResponse<T>
{
    public int StatusCode { get; init; }

    public required string Message { get; init; }

    public T? Data { get; init; }
}
