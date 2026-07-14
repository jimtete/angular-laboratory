using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Parsers;

public static class MediaParser
{
    public static async Task<byte[]> ReadProfilePictureBytesAsync(
        IFormFile? profilePicture,
        CancellationToken cancellationToken)
    {
        return await ReadFormFileBytesAsync(profilePicture, cancellationToken);
    }

    public static async Task<byte[]> ReadCampaignPictureBytesAsync(
        IFormFile? campaignPicture,
        CancellationToken cancellationToken)
    {
        return await ReadFormFileBytesAsync(campaignPicture, cancellationToken);
    }

    private static async Task<byte[]> ReadFormFileBytesAsync(
        IFormFile? formFile,
        CancellationToken cancellationToken)
    {
        if (formFile is null)
        {
            return [];
        }
        
        using var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream, cancellationToken);
        
        return memoryStream.ToArray();
    }
}
