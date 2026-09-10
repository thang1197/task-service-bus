using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using TaskManagementServiceBusApi.File.DTO;


namespace TaskManagementServiceBusApi.File.Service
{
    public class ImageProcessor(ILogger logger) : IFileProcessor
    {
        private readonly ILogger _logger = logger;
        private readonly int _maxWidth = 400;
        public async Task<FileHandleResponse> HandleFile(Stream fileStream, string fileName, string fileType)
        {
            try
            {
                using var memory = new MemoryStream();
                await fileStream.CopyToAsync(memory);
                _logger.LogInformation("Length: {Length}", memory.Length);

                memory.Position = 0;

                using var bitmap = SKBitmap.Decode(memory)
                    ?? throw new InvalidOperationException("Unable to decode image.");

                SKBitmap outputBitmap = bitmap;

                if (bitmap.Width > _maxWidth)
                {
                    var newHeight = bitmap.Height * _maxWidth / bitmap.Width;

                    outputBitmap = bitmap.Resize(new SKImageInfo(_maxWidth, newHeight),SKSamplingOptions.Default)?? throw new InvalidOperationException("Failed to resize image.");
                }

                try
                {
                    using var image = SKImage.FromBitmap(outputBitmap);
                    using var data = image.Encode(SKEncodedImageFormat.Webp, 80);

                    var stream = new MemoryStream();
                    data.SaveTo(stream);
                    stream.Position = 0;

                    var newFileName = Guid.NewGuid() + Path.GetExtension(fileName);
                    return new FileHandleResponse
                    {
                        FileName = newFileName,
                        FileType = fileType,
                        FileStream = stream
                    };
                }
                finally
                {
                    if (!ReferenceEquals(outputBitmap, bitmap))
                    {
                        outputBitmap.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while processing file: {ex.Message}");
                throw;
            }
        }
    }
}