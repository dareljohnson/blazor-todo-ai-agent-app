using OpenAI.Images;

namespace BlazorAiAgentTodo.Services;

public interface IImageService
{
    Task<string> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default);
}

public class ImageService : IImageService
{
    private readonly ImageClient _imageClient;
    private readonly ILogger<ImageService> _logger;

    public ImageService(ImageClient imageClient, ILogger<ImageService> logger)
    {
        _imageClient = imageClient ?? throw new ArgumentNullException(nameof(imageClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateImageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
        }

        if (prompt.Length > 1000)
        {
            throw new ArgumentException("Prompt cannot exceed 1000 characters.", nameof(prompt));
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            _logger.LogInformation("Generating image with prompt: {Prompt}", prompt);

            var imageGeneration = await _imageClient.GenerateImageAsync(
                prompt,
                new ImageGenerationOptions
                {
                    Quality = GeneratedImageQuality.Standard,
                    Size = GeneratedImageSize.W1024xH1024,
                    Style = GeneratedImageStyle.Vivid,
                    ResponseFormat = GeneratedImageFormat.Bytes
                },
                cancellationToken);

            var imageBytes = imageGeneration.Value.ImageBytes.ToArray();
            var base64Image = Convert.ToBase64String(imageBytes);

            _logger.LogInformation("Image generated successfully. Size: {Size} bytes", imageBytes.Length);

            return $"data:image/png;base64,{base64Image}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate image with prompt: {Prompt}", prompt);
            throw;
        }
    }

}
