using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PngLib
{
    public static class ImageManipulations
    {
        public static Bitmap CreateThumbnail(this Image inputImg, int thumbnailSquareSize)
        {
            // if thumbnail size is invalid, set to 100x100
            thumbnailSquareSize = thumbnailSquareSize <= 0 ? 100 : thumbnailSquareSize;

            int newWidth = inputImg.Width >= inputImg.Height
                               ? thumbnailSquareSize
                               : (int)(((double)inputImg.Width / inputImg.Height) * thumbnailSquareSize);
            int newHeight = inputImg.Height >= inputImg.Width
                                ? thumbnailSquareSize
                                : (int)(((double)inputImg.Height / inputImg.Width) * thumbnailSquareSize);

            var thumbnailImage = new Bitmap(thumbnailSquareSize, thumbnailSquareSize);

            using (Graphics graphics = Graphics.FromImage(thumbnailImage))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(inputImg,
                                   new Rectangle((thumbnailSquareSize - newWidth) / 2,
                                                 (thumbnailSquareSize - newHeight) / 2,
                                                 newWidth, newHeight));
            }

            inputImg.Dispose();
            return thumbnailImage;
        }

        public static Bitmap CropImage(this Image inputImg, int top, int right, int bottom, int left)
        {
            int newWidth = inputImg.Width - left - right,
                newHeight = inputImg.Height - top - bottom;

            // Don't allow cropping to nothing
            if (newWidth <= 0 || newHeight <= 0)
            {
                return (Bitmap)inputImg;
            }

            var croppedImage = new Bitmap(newWidth, newHeight);

            using (Graphics graphics = Graphics.FromImage(croppedImage))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(inputImg, 0, 0, new RectangleF(left, top, newWidth, newHeight), GraphicsUnit.Pixel);
            }

            inputImg.Dispose();
            return croppedImage;
        }

        public static Bitmap ScaleImage(this Image inputImg, double scaleX, double scaleY)
        {
            var outImage = new Bitmap((int)(inputImg.Width * scaleX), (int)(inputImg.Height * scaleY));

            using (Graphics g = Graphics.FromImage(outImage))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //Transformation matrix
                var m = new Matrix();
                m.Scale((float)scaleX, (float)scaleY);
                //m.Translate((float)scaleX, (float)scaleY);

                g.Transform = m;
                g.DrawImage(inputImg, 0, 0);
            }
            return outImage;
        }

        public static Bitmap RotateImage(this Image inputImg, double degreeAngle)
        {
            //Corners of the image
            PointF[] rotationPoints = { new PointF(0, 0),
                                        new PointF(inputImg.Width, 0),
                                        new PointF(0, inputImg.Height),
                                        new PointF(inputImg.Width, inputImg.Height)};

            //Rotate the corners
            PointMath.RotatePoints(rotationPoints, new PointF(inputImg.Width / 2.0f, inputImg.Height / 2.0f), degreeAngle);

            //Get the new bounds given from the rotation of the corners
            //(avoid clipping of the image)
            Rectangle bounds = PointMath.GetBounds(rotationPoints);

            //An empy bitmap to draw the rotated image
            var rotatedBitmap = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(rotatedBitmap))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //Transformation matrix
                var m = new Matrix();
                m.RotateAt((float)degreeAngle, new PointF(inputImg.Width / 2.0f, inputImg.Height / 2.0f));
                m.Translate(-bounds.Left, -bounds.Top, MatrixOrder.Append); //shift to compensate for the rotation

                g.Transform = m;
                g.DrawImage(inputImg, 0, 0);
            }
            return rotatedBitmap;
        }
        public static class PointMath
        {
            private static double DegreeToRadian(double angle)
            {
                return Math.PI * angle / 180.0;
            }

            public static PointF RotatePoint(PointF pnt, double degreeAngle)
            {
                return RotatePoint(pnt, new PointF(0, 0), degreeAngle);
            }

            public static PointF RotatePoint(PointF pnt, PointF origin, double degreeAngle)
            {
                double radAngle = DegreeToRadian(degreeAngle);

                var newPoint = new PointF();

                double deltaX = pnt.X - origin.X;
                double deltaY = pnt.Y - origin.Y;

                newPoint.X = (float)(origin.X + (Math.Cos(radAngle) * deltaX - Math.Sin(radAngle) * deltaY));
                newPoint.Y = (float)(origin.Y + (Math.Sin(radAngle) * deltaX + Math.Cos(radAngle) * deltaY));

                return newPoint;
            }

            public static void RotatePoints(PointF[] pnts, double degreeAngle)
            {
                for (int i = 0; i < pnts.Length; i++)
                {
                    pnts[i] = RotatePoint(pnts[i], degreeAngle);
                }
            }

            public static void RotatePoints(PointF[] pnts, PointF origin, double degreeAngle)
            {
                for (int i = 0; i < pnts.Length; i++)
                {
                    pnts[i] = RotatePoint(pnts[i], origin, degreeAngle);
                }
            }

            public static Rectangle GetBounds(PointF[] pnts)
            {
                RectangleF boundsF = GetBoundsF(pnts);
                return new Rectangle((int)Math.Round(boundsF.Left),
                                     (int)Math.Round(boundsF.Top),
                                     (int)Math.Round(boundsF.Width),
                                     (int)Math.Round(boundsF.Height));
            }

            public static RectangleF GetBoundsF(PointF[] pnts)
            {
                float left = pnts[0].X;
                float right = pnts[0].X;
                float top = pnts[0].Y;
                float bottom = pnts[0].Y;

                for (int i = 1; i < pnts.Length; i++)
                {
                    if (pnts[i].X < left)
                        left = pnts[i].X;
                    else if (pnts[i].X > right)
                        right = pnts[i].X;

                    if (pnts[i].Y < top)
                        top = pnts[i].Y;
                    else if (pnts[i].Y > bottom)
                        bottom = pnts[i].Y;
                }

                return new RectangleF(left,
                                      top,
                                     Math.Abs(right - left),
                                     Math.Abs(bottom - top));
            }
        }
    }
}
