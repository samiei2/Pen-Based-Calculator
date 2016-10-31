using System;

namespace HW1Armin
{
    internal class PerpendicularBisector
    {
        private Line2D.Double bisector;
        private double bisectorSlope;
        private double bisectorYIntercept;
        private BoundingBox boundingBox;
        private Point2D midPoint;
        private Point2D point2D1;
        private Point2D point2D2;

        public PerpendicularBisector(Point2D p1, Point2D p2, BoundingBox bounds)
        {
            this.point2D1 = p1;
            this.point2D2 = p2;
            this.boundingBox = bounds;

            double xMid = ((p2.X - p1.X) / 2) + p1.X;
            double yMid = ((p2.Y - p1.Y) / 2) + p1.Y;
            midPoint = new Point2D(xMid, yMid);

            if ((p2.Y - p1.Y) == 0)
            {
                // line is horizontal, slope 0
                // perpendicular bisector is slope infinity
                bisectorSlope = double.NaN;
            }
            else if ((p2.X - p1.X) == 0)
            {
                // line is vertical, slope infinity
                // perpendicular bisector is slope 0
                bisectorSlope = 0;
            }
            else
            {
                bisectorSlope = -1 * (p2.X - p1.X)
                        / (p2.Y - p1.Y);
            }

            double x1, y1, x2, y2;
            if (double.IsNaN(bisectorSlope))
            {
                // vertical
                bisectorYIntercept = double.NaN;
                y1 = bounds.getMinY();
                y2 = bounds.getMaxY();
                x1 = midPoint.X;
                x2 = midPoint.X;
            }
            else if (bisectorSlope == 0)
            {
                // horizontal
                bisectorYIntercept = midPoint.Y;
                y1 = midPoint.Y;
                y2 = midPoint.Y;
                x1 = bounds.getMinX();
                x2 = bounds.getMaxX();
            }
            else
            {
                // solve for y intercept
                // y = mx + b
                double y = midPoint.Y;
                double x = midPoint.X;
                double m = bisectorSlope;
                double b = y - (m * x);
                bisectorYIntercept = b;

                x1 = bounds.getMinX();
                x2 = bounds.getMaxX();

                if (double.IsNaN(m))
                {
                    y1 = bounds.getMinY();
                    y2 = bounds.getMaxY();
                }
                else
                {
                    y1 = m * x1 + b;
                    y2 = m * x2 + b;
                }
            }
            bisector = new Line2D.Double(x1, y1, x2, y2);
        }

        internal Line2D.Double getBisector()
        {
            return bisector;
        }
    }
}