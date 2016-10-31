using System;

namespace HW1Armin
{
    public class BoundingBox : Rectangle2D
    {
        public BoundingBox(double minX, double minY, double maxX, double maxY) : base(minX, minY, maxX, maxY)
        {
            
        }

        internal double getPerimeter()
        {
            return 2 * width + 2 * height;
        }
        
        public double getTop()
        {
            return getMinY();
        }
        
        public double getBottom()
        {
            return getMaxY();
        }
        
        public double getLeft()
        {
            return getMinX();
        }
        
        public double getRight()
        {
            return getMaxX();
        }

        public Point2D getTopRightPoint()
        {
            return new Point2D(getRight(), getTop());
        }

        public Point2D getTopLeftPoint()
        {
            return new Point2D(getLeft(), getTop());
        }

        public Point2D getBottomLeftPoint()
        {
            return new Point2D(getLeft(), getBottom());
        }

        public Point2D getBottomRightPoint()
        {
            return new Point2D(getRight(), getBottom());
        }

        internal double getCenterX()
        {
            return (getLeft() + width / 2);
        }

        internal double getCenterY()
        {
            return (getTop() + height / 2);
        }
    }
}