using System;

namespace HW1Armin
{
    public class Rectangle2D
    {
        public double height;
        public double width;
        protected double x;
        protected double y;
        protected double minX;
        protected double minY;
        protected double maxX;
        protected double maxY;


        public Rectangle2D(double minX, double minY, double maxX, double maxY)
        {
            double x = Math.Min(minX, maxX);
            double y = Math.Min(minY, maxY);
            double width = Math.Abs(maxX - minX);
            double height = Math.Abs(maxY - minY);

            this.x = x;
            this.y = y;
            this.minX = x;
            this.minY = y;
            this.maxX = Math.Max(minX, maxX);
            this.maxY = Math.Max(minY, maxY);
            this.width = width;
            this.height = height;
        }
        public double getMinX()
        {
            return x;
        }

        public double getMinY()
        {
            return y;
        }

        public double getMaxX()
        {
            return x + width;
        }

        public double getMaxY()
        {
            return y + height;
        }
    }
}