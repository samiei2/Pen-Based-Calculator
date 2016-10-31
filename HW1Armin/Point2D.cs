using System;

namespace HW1Armin
{
    public class Point2D
    {
        public double X;
        public double Y;

        public Point2D(Point2D point2D)
        {
            this.X = point2D.X;
            this.Y = point2D.Y;
        }

        public Point2D(double x1, double y1)
        {
            this.X = x1;
            this.Y = y1;
        }

        internal double Distance(Point2D point2D)
        {
            return Math.Sqrt(Math.Pow((point2D.X - this.X),2) + Math.Pow((point2D.Y - this.Y), 2));
        }
    }
}