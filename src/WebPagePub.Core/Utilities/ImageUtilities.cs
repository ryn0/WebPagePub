using System;
using System.Drawing;

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
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }

        public Bitmap RotateImage(Image image, float angle)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }

            const double pi2 = Math.PI / 2.0;

            // Why can't C# allow these to be const, or at least readonly
            // *sigh*  I'm starting to talk like Christian Graus :omg:
            double oldWidth = image.Width;
            double oldHeight = image.Height;

            // Convert degrees to radians
            double theta = angle * Math.PI / 180.0;
            double lockedTheta = theta;

            // Ensure theta is now [0, 2pi)
            while (lockedTheta < 0.0)
            {
                lockedTheta += 2 * Math.PI;
            }

            /*
             * The trig involved in calculating the new width and height
             * is fairly simple; the hard part was remembering that when
             * PI/2 <= theta <= PI and 3PI/2 <= theta < 2PI the width and
             * height are switched.
             *
             * When you rotate a rectangle, r, the bounding box surrounding r
             * contains for right-triangles of empty space.  Each of the
             * triangles hypotenuse's are a known length, either the width or
             * the height of r.  Because we know the length of the hypotenuse
             * and we have a known angle of rotation, we can use the trig
             * function identities to find the length of the other two sides.
             *
             * sine = opposite/hypotenuse
             * cosine = adjacent/hypotenuse
             *
             * solving for the unknown we get
             *
             * opposite = sine * hypotenuse
             * adjacent = cosine * hypotenuse
             *
             * Another interesting point about these triangles is that there
             * are only two different triangles. The proof for which is easy
             * to see, but its been too long since I've written a proof that
             * I can't explain it well enough to want to publish it.
             *
             * Just trust me when I say the triangles formed by the lengths
             * width are always the same (for a given theta) and the same
             * goes for the height of r.
             *
             * Rather than associate the opposite/adjacent sides with the
             * width and height of the original bitmap, I'll associate them
             * based on their position.
             *
             * adjacent/oppositeTop will refer to the triangles making up the
             * upper right and lower left corners
             *
             * adjacent/oppositeBottom will refer to the triangles making up
             * the upper left and lower right corners
             *
             * The names are based on the right side corners, because thats
             * where I did my work on paper (the right side).
             *
             * Now if you draw this out, you will see that the width of the
             * bounding box is calculated by adding together adjacentTop and
             * oppositeBottom while the height is calculate by adding
             * together adjacentBottom and oppositeTop.
             */

            double adjacentTop, oppositeTop;
            double adjacentBottom, oppositeBottom;

            // We need to calculate the sides of the triangles based
            // on how much rotation is being done to the bitmap.
            //   Refer to the first paragraph in the explaination above for
            //   reasons why.
            if ((lockedTheta >= 0.0 && lockedTheta < pi2) ||
                (lockedTheta >= Math.PI && lockedTheta < (Math.PI + pi2)))
            {
                adjacentTop = Math.Abs(Math.Cos(lockedTheta)) * oldWidth;
                oppositeTop = Math.Abs(Math.Sin(lockedTheta)) * oldWidth;

                adjacentBottom = Math.Abs(Math.Cos(lockedTheta)) * oldHeight;
                oppositeBottom = Math.Abs(Math.Sin(lockedTheta)) * oldHeight;
            }
            else
            {
                adjacentTop = Math.Abs(Math.Sin(lockedTheta)) * oldHeight;
                oppositeTop = Math.Abs(Math.Cos(lockedTheta)) * oldHeight;

                adjacentBottom = Math.Abs(Math.Sin(lockedTheta)) * oldWidth;
                oppositeBottom = Math.Abs(Math.Cos(lockedTheta)) * oldWidth;
            }

            double newWidth = adjacentTop + oppositeBottom;
            double newHeight = adjacentBottom + oppositeTop;

            var nWidth = (int)Math.Ceiling(newWidth);
            var nHeight = (int)Math.Ceiling(newHeight);

            var rotatedBmp = new Bitmap(nWidth, nHeight);

            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                // This array will be used to pass in the three points that
                // make up the rotated image
                Point[] points;

                /*
                 * The values of opposite/adjacentTop/Bottom are referring to
                 * fixed locations instead of in relation to the
                 * rotating image so I need to change which values are used
                 * based on the how much the image is rotating.
                 *
                 * For each point, one of the coordinates will always be 0,
                 * nWidth, or nHeight.  This because the Bitmap we are drawing on
                 * is the bounding box for the rotated bitmap.  If both of the
                 * corrdinates for any of the given points wasn't in the set above
                 * then the bitmap we are drawing on WOULDN'T be the bounding box
                 * as required.
                 */
                if (lockedTheta >= 0.0 && lockedTheta < pi2)
                {
                    points = new[]
                    {
                        new Point((int)oppositeBottom, 0),
                        new Point(nWidth, (int)oppositeTop),
                        new Point(0, (int)adjacentBottom)
                    };
                }
                else if (lockedTheta >= pi2 && lockedTheta < Math.PI)
                {
                    points = new[]
                    {
                        new Point(nWidth, (int)oppositeTop),
                        new Point((int)adjacentTop, nHeight),
                        new Point((int)oppositeBottom, 0)
                    };
                }
                else if (lockedTheta >= Math.PI && lockedTheta < (Math.PI + pi2))
                {
                    points = new[]
                    {
                        new Point((int)adjacentTop, nHeight),
                        new Point(0, (int)adjacentBottom),
                        new Point(nWidth, (int)oppositeTop)
                    };
                }
                else
                {
                    points = new[]
                    {
                        new Point(0, (int)adjacentBottom),
                        new Point((int)oppositeBottom, 0),
                        new Point((int)adjacentTop, nHeight)
                    };
                }

                g.DrawImage(image, points);
            }

            return rotatedBmp;
        }

        public Image ScaleImage(object value, int maxWidthPx, int maxHeightPx)
        {
            throw new NotImplementedException();
        }
    }
}
