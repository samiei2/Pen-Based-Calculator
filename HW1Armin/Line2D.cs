using System;

namespace HW1Armin
{
    public class Line2D
    {
        protected double x1;
        protected double x2;
        protected double y1;
        protected double y2;
        protected double slope;

        Point2D p1;
        Point2D p2;

        // starndard equation A,B,C : AX+BY+C = 0
        protected double A;
        protected double B;
        protected double C;

        public Line2D()
        {
        }

        public Line2D(double x1, double y1, double x2, double y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.p1 = new Point2D(x1, y1);
            this.p2 = new Point2D(x2, y2);

            this.slope = (y2 - y1) / (x2 - x1);
            this.A = -slope;
            this.B = 1;
            this.C = (slope * x1) - y1;
        }

        public class Double : Line2D
        {
            public Double() : base()
            {
            }

            public Double(double x1, double y1, double x2, double y2) : base(x1, y1, x2, y2)
            {
            }
            
        }

        internal double ptSegDist(double x, double y)
        {
            return Math.Abs((A * x + B * y + C) / (Math.Sqrt(Math.Pow(A, 2) + Math.Pow(B, 2))));
        }

        internal Point2D GetP1()
        {
            return p1;
        }

        internal Point2D GetP2()
        {
            return p2;
        }

        internal double GetY1()
        {
            return y1;
        }

        internal double GetY2()
        {
            return y2;
        }

        internal double GetX1()
        {
            return x1;
        }

        internal double GetX2()
        {
            return x2;
        }

        internal double ptSegDist2(double x, double y)
        {
            return LineToPointDistance2D(
                new double[] { this.p1.X,this.p1.Y}, 
                new double[] { this.p2.X, this.p2.Y }, 
                new double[] { x,y},
                true);
        }

        //Compute the dot product AB . AC
        private double DotProduct(double[] pointA, double[] pointB, double[] pointC)
        {
            double[] AB = new double[2];
            double[] BC = new double[2];
            AB[0] = pointB[0] - pointA[0];
            AB[1] = pointB[1] - pointA[1];
            BC[0] = pointC[0] - pointB[0];
            BC[1] = pointC[1] - pointB[1];
            double dot = AB[0] * BC[0] + AB[1] * BC[1];

            return dot;
        }

        //Compute the cross product AB x AC
        private double CrossProduct(double[] pointA, double[] pointB, double[] pointC)
        {
            double[] AB = new double[2];
            double[] AC = new double[2];
            AB[0] = pointB[0] - pointA[0];
            AB[1] = pointB[1] - pointA[1];
            AC[0] = pointC[0] - pointA[0];
            AC[1] = pointC[1] - pointA[1];
            double cross = AB[0] * AC[1] - AB[1] * AC[0];

            return cross;
        }

        //Compute the distance from A to B
        double Distance(double[] pointA, double[] pointB)
        {
            double d1 = pointA[0] - pointB[0];
            double d2 = pointA[1] - pointB[1];

            return Math.Sqrt(d1 * d1 + d2 * d2);
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        double LineToPointDistance2D(double[] pointA, double[] pointB, double[] pointC,
            bool isSegment)
        {
            double dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                double dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                double dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }
            return Math.Abs(dist);
        }

        public bool intersectsLine(double x1, double y1, double x2, double y2)
        {
            LineSegmentIntersection.Vector outres = new LineSegmentIntersection.Vector();
            return LineSegmentIntersection.LineSegment.LineSegementsIntersect(
                new LineSegmentIntersection.Vector(x1, y1),
                new LineSegmentIntersection.Vector(x2, y2),
                new LineSegmentIntersection.Vector(this.x1, this.y1),
                new LineSegmentIntersection.Vector(this.x2, this.y2),
                out outres
                );
        }
    }
}