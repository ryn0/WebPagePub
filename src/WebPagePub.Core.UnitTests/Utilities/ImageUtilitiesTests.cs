using System.Drawing;
using WebPagePub.Core.Utilities;
using Xunit;

namespace WebPagePub.Core.UnitTests.Utilities
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

        private void SetPixelsForExpectedRotatedImage(Bitmap image)
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
    }
}
