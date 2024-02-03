using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace WebPagePub.Core.Utilities
{
    public class ImageUtilities
    {
        public static Bitmap Rotate90Degrees(Image image)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            var rotatedBmp = new Bitmap(image);

            return rotatedBmp;
        }

        public static Bitmap ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.Width <= maxWidth && image.Height <= maxHeight)
            {
                return new Bitmap(image);  // If the image is already smaller than or equal to the max dimensions, return it as is.
            }

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }

        public static MemoryStream ConvertFileToMemoryStream(string filePath)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(memoryStream);
            }

            // It's important to reset the position of the MemoryStream to the beginning after copying the content
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}