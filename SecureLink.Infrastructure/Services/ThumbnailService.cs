using Microsoft.Extensions.Logging;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace SecureLink.Infrastructure.Services;

public class ThumbnailService() : IThumbnailService
{
    public async Task<Stream> CreateThumbnail(Stream input)
    {
        // Using `using` here would dispose the stream at the end of the method.
        // We need to return stream for uplaoding to storage
        var output = new MemoryStream();
        using var image = await Image.LoadAsync(input);
        image.Mutate(x =>
            x.Resize(
                new ResizeOptions
                {
                    Size = new Size(400, 400), // Bounding box
                    Mode = ResizeMode.Max, // Constrains the resized image to fit the bounds of its container maintaining the original aspect ratio.
                    Sampler = KnownResamplers.Lanczos3,
                }
            )
        );

        var encoder = new WebpEncoder
        {
            Quality = 75,
            FileFormat = WebpFileFormatType.Lossy,
            Method = WebpEncodingMethod.Level4,
        };

        await image.SaveAsWebpAsync(output, encoder);
        output.Position = 0;
        return output;
    }
}
