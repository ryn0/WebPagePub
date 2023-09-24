using System;
using System.Drawing;
using System.Runtime.InteropServices;
using WebPagePub.Core.Utilities;
using Xunit;

namespace WebPagePub.Core.UnitTests.UtilitiesTests
{
    public class ImageUtilitiesTests
    {
        [Fact]
        public void Rotate90Degrees_ShouldRotateImageCorrectly()
        {
            // Arrange
            var originalImage = new Bitmap(2, 3);
            this.SetPixelsForOriginalImage(originalImage);  // This method would set the specific pixels for the image

            var expectedRotatedImage = new Bitmap(3, 2);
            this.SetPixelsForExpectedRotatedImage(expectedRotatedImage);  // This method would set the pixels for the expected rotated image

            // Act
            var resultImage = ImageUtilities.Rotate90Degrees(originalImage);

            // Assert
            Assert.True(this.BitmapAreEqual(expectedRotatedImage, resultImage));
        }

        [Fact]
        public void ScaleImage_WithImageSmallerThanMaxDimensions_ShouldReturnOriginalSize()
        {
            // Arrange
            var originalImage = new Bitmap(50, 50);  // Example image size
            var maxWidth = 100;
            var maxHeight = 100;

            // Act
            var scaledImage = ImageUtilities.ScaleImage(originalImage, maxWidth, maxHeight);

            // Assert
            Assert.Equal(originalImage.Width, scaledImage.Width);
            Assert.Equal(originalImage.Height, scaledImage.Height);
        }

        [Fact]
        public void ScaleImage_WithWidthAsLimitingFactor_ShouldScaleProportionally()
        {
            // Arrange
            var originalImage = new Bitmap(100, 50);  // Width is larger proportionally
            var maxWidth = 50;
            var maxHeight = 100;

            // Act
            var scaledImage = ImageUtilities.ScaleImage(originalImage, maxWidth, maxHeight);

            // Assert
            Assert.Equal(50, scaledImage.Width);
            Assert.Equal(25, scaledImage.Height);  // Height should be scaled down by the same ratio
        }

        [Fact]
        public void ScaleImage_WithHeightAsLimitingFactor_ShouldScaleProportionally()
        {
            // Arrange
            var originalImage = new Bitmap(50, 100);  // Height is larger proportionally
            var maxWidth = 100;
            var maxHeight = 50;

            // Act
            var scaledImage = ImageUtilities.ScaleImage(originalImage, maxWidth, maxHeight);

            // Assert
            Assert.Equal(25, scaledImage.Width);  // Width should be scaled down by the same ratio
            Assert.Equal(50, scaledImage.Height);
        }

        [Fact]
        public void ScaleImage_WithNullImage_ShouldThrowArgumentNullException()
        {
            // Arrange
            Image nullImage = null;
            var maxWidth = 100;
            var maxHeight = 100;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ImageUtilities.ScaleImage(nullImage, maxWidth, maxHeight));
        }

        private bool BitmapAreEqual(Bitmap bmp1, Bitmap bmp2)
        {
            for (int i = 0; i < bmp1.Width; i++)
            {
                for (int j = 0; j < bmp1.Height; j++)
                {
                    if (bmp1.GetPixel(i, j) != bmp2.GetPixel(i, j))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void SetPixelsForOriginalImage(Bitmap image)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Let's imagine the original 2x3 image has colors as follows:
                // Red, Green
                // Blue, Yellow
                // White, Black
                image.SetPixel(0, 0, Color.Red);
                image.SetPixel(1, 0, Color.Green);
                image.SetPixel(0, 1, Color.Blue);
                image.SetPixel(1, 1, Color.Yellow);
                image.SetPixel(0, 2, Color.White);
                image.SetPixel(1, 2, Color.Black);
            }
            else
            {
                throw new Exception("Not implemented for non-Windows platforms");
            }
        }

        private void SetPixelsForExpectedRotatedImage(Bitmap image)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // After rotating the above 2x3 image by 90 degrees clockwise, we should get a 3x2 image:
                // White, Blue, Red
                // Black, Yellow, Green
                image.SetPixel(0, 0, Color.White);
                image.SetPixel(1, 0, Color.Blue);
                image.SetPixel(2, 0, Color.Red);
                image.SetPixel(0, 1, Color.Black);
                image.SetPixel(1, 1, Color.Yellow);
                image.SetPixel(2, 1, Color.Green);
            }
            else
            {
                throw new Exception("Not implemented for non-Windows platforms");
            }
        }
    }
}
