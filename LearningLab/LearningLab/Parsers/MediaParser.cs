namespace LearningLab.Parsers;

public static class MediaParser
{
    public static async Task<byte[]> ReadProfilePictureBytesAsync(
        IFormFile? profilePicture,
        CancellationToken cancellationToken)
    {
        if (profilePicture is null)
        {
            return [];
        }
        
        using var memoryStream = new MemoryStream();
        await profilePicture.CopyToAsync(memoryStream, cancellationToken);
        
        return memoryStream.ToArray();
    }
}