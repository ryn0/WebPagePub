using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace WebPagePub.Core.Utilities
{
    public static class ImageUtilities
    {
        /// <summary>
        /// Rotates an image 90 degrees clockwise. Output is encoded in the
        /// same format as the input (JPEG stays JPEG, PNG stays PNG, etc.).
        /// </summary>
        /// <param name="input">A stream containing encoded image data.</param>
        /// <returns>A new MemoryStream positioned at 0 containing the rotated image.</returns>
        public static MemoryStream Rotate90Degrees(Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.CanSeek)
            {
                input.Position = 0;
            }

            using var image = Image.Load(input);
            image.Mutate(x => x.Rotate(RotateMode.Rotate90));

            var output = new MemoryStream();
            image.Save(output, image.Metadata.DecodedImageFormat
                ?? throw new InvalidOperationException("Unable to determine source image format."));
            output.Position = 0;
            return output;
        }

        /// <summary>
        /// Scales an image proportionally to fit within max width and height.
        /// If the image is already within bounds, the original bytes are returned
        /// unchanged. Output is encoded in the same format as the input.
        /// </summary>
        /// <param name="input">A stream containing encoded image data.</param>
        /// <param name="maxWidth">Maximum width in pixels.</param>
        /// <param name="maxHeight">Maximum height in pixels.</param>
        /// <returns>A new MemoryStream positioned at 0 containing the scaled image.</returns>
        public static MemoryStream ScaleImage(Stream input, int maxWidth, int maxHeight)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.CanSeek)
            {
                input.Position = 0;
            }

            using var image = Image.Load(input);

            // If already within bounds, copy the original bytes through unchanged.
            // Re-encoding would cause unnecessary quality loss for JPEGs.
            if (image.Width <= maxWidth && image.Height <= maxHeight)
            {
                if (input.CanSeek)
                {
                    input.Position = 0;
                }

                var copy = new MemoryStream();
                input.CopyTo(copy);
                copy.Position = 0;
                return copy;
            }

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(newWidth, newHeight),
                Sampler = KnownResamplers.Bicubic,
                Mode = ResizeMode.Stretch,
            }));

            var output = new MemoryStream();
            image.Save(output, image.Metadata.DecodedImageFormat
                ?? throw new InvalidOperationException("Unable to determine source image format."));
            output.Position = 0;
            return output;
        }

        /// <summary>
        /// Reads a file fully into a MemoryStream and resets the position to 0.
        /// </summary>
        public static MemoryStream ConvertFileToMemoryStream(string filePath)
        {
            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(memoryStream);
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}