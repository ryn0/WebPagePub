using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebPagePub.Core.Utilities;
using Xunit;

namespace WebPagePub.Core.UnitTests.UtilitiesTests
{
    public class ImageUtilitiesTests
    {
        [Fact]
        public void Rotate90Degrees_ShouldRotateImageCorrectly()
        {
            // Arrange — build a 2x3 image with a distinct color at each pixel.
            // Layout (x, y):
            //   (0,0)=Red    (1,0)=Green
            //   (0,1)=Blue   (1,1)=Yellow
            //   (0,2)=White  (1,2)=Black
            var red = Color.Red.ToPixel<Rgba32>();
            var green = Color.Green.ToPixel<Rgba32>();
            var blue = Color.Blue.ToPixel<Rgba32>();
            var yellow = Color.Yellow.ToPixel<Rgba32>();
            var white = Color.White.ToPixel<Rgba32>();
            var black = Color.Black.ToPixel<Rgba32>();

            using var original = new Image<Rgba32>(2, 3);
            original[0, 0] = red;
            original[1, 0] = green;
            original[0, 1] = blue;
            original[1, 1] = yellow;
            original[0, 2] = white;
            original[1, 2] = black;

            using var input = new MemoryStream();
            original.SaveAsPng(input);

            // Act
            using var resultStream = ImageUtilities.Rotate90Degrees(input);
            using var result = Image.Load<Rgba32>(resultStream);

            // Assert — after 90° clockwise rotation, the 2x3 becomes 3x2:
            //   (0,0)=White  (1,0)=Blue    (2,0)=Red
            //   (0,1)=Black  (1,1)=Yellow  (2,1)=Green
            Assert.Equal(3, result.Width);
            Assert.Equal(2, result.Height);
            Assert.Equal(white, result[0, 0]);
            Assert.Equal(blue, result[1, 0]);
            Assert.Equal(red, result[2, 0]);
            Assert.Equal(black, result[0, 1]);
            Assert.Equal(yellow, result[1, 1]);
            Assert.Equal(green, result[2, 1]);
        }

        [Fact]
        public void Rotate90Degrees_WithNullStream_ShouldThrowArgumentNullException()
        {
            Stream? nullStream = null;
            Assert.Throws<ArgumentNullException>(() => ImageUtilities.Rotate90Degrees(nullStream!));
        }

        [Fact]
        public void ScaleImage_WithImageSmallerThanMaxDimensions_ShouldReturnOriginalSize()
        {
            // Arrange
            using var original = new Image<Rgba32>(50, 50);
            using var input = new MemoryStream();
            original.SaveAsPng(input);

            // Act
            using var resultStream = ImageUtilities.ScaleImage(input, 100, 100);
            using var result = Image.Load(resultStream);

            // Assert
            Assert.Equal(50, result.Width);
            Assert.Equal(50, result.Height);
        }

        [Fact]
        public void ScaleImage_WithWidthAsLimitingFactor_ShouldScaleProportionally()
        {
            // Arrange — width is the binding constraint (100 → 50 means ratio 0.5)
            using var original = new Image<Rgba32>(100, 50);
            using var input = new MemoryStream();
            original.SaveAsPng(input);

            // Act
            using var resultStream = ImageUtilities.ScaleImage(input, 50, 100);
            using var result = Image.Load(resultStream);

            // Assert
            Assert.Equal(50, result.Width);
            Assert.Equal(25, result.Height);
        }

        [Fact]
        public void ScaleImage_WithHeightAsLimitingFactor_ShouldScaleProportionally()
        {
            // Arrange — height is the binding constraint (100 → 50 means ratio 0.5)
            using var original = new Image<Rgba32>(50, 100);
            using var input = new MemoryStream();
            original.SaveAsPng(input);

            // Act
            using var resultStream = ImageUtilities.ScaleImage(input, 100, 50);
            using var result = Image.Load(resultStream);

            // Assert
            Assert.Equal(25, result.Width);
            Assert.Equal(50, result.Height);
        }

        [Fact]
        public void ScaleImage_WithNullStream_ShouldThrowArgumentNullException()
        {
            Stream? nullStream = null;
            Assert.Throws<ArgumentNullException>(() => ImageUtilities.ScaleImage(nullStream!, 100, 100));
        }
    }
}