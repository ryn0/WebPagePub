using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WebPagePub.Core.Utilities
{
    public class ImageUtilities
    {
        public Bitmap ScaleImage(Image image, int maxWidth, int maxHeight)
        {
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

        public static Bitmap Rotate90Degrees(Image image)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            var rotatedBmp = new Bitmap(image);

            return rotatedBmp;
        }
    }
}
