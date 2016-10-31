using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace HW1Armin
{
    public class FeatureArea
    {
        /**
	     * Calculate the feature area of the (x,y) points to the input line
	     * 
	     * @param points
	     *            points
	     * @param line
	     *            line to find feature area to
	     * @return feature area from the (x,y) points to the input line
	     */
        public static double toLine(StylusPointCollection points, Line2D line)
        {
            double area = 0;
            double b1, b2, d, h;
            for (int i = 0; i < points.Count - 1; i++)
            {
                b1 = line.ptSegDist(points[i].X, points[i].Y);
                b2 = line.ptSegDist(points[i + 1].X, points[i + 1]
                        .Y);
                d = distance(points[i],points[i + 1]);
                h = Math.Sqrt(Math.Abs(Math.Pow(d, 2)
                        - Math.Pow(Math.Abs(b1 - b2), 2)));
                area += Math.Abs(0.5 * (b1 + b2) * h);
            }
            return area;
        }

        public static double distance(StylusPoint p1, StylusPoint p2)
        {
            double Xdist = p1.X - p2.X;
            double Ydist = p1.Y - p2.Y;

            return Math.Sqrt(Xdist * Xdist + Ydist * Ydist);
        }

        /**
	     * Calculate the feature area of the (x,y) points to the input point
	     * 
	     * @param points
	     *            points
	     * @param p
	     *            point to find feature area to
	     * @return feature area from the (x,y) points to the input point
	     */
        public static double toPoint(StylusPointCollection points, Point2D p)
        {
            double area = 0;
            double a, b, c, s;
            for (int i = 0; i < points.Count - 1; i++)
            {
                a = Distance(points[i],new Point2D(points[i + 1].X, points[i + 1].Y));
                b = Distance(points[i],new Point2D(p.X, p.Y));
                c = Distance(points[i + 1],(new Point2D(p.X, p.Y)));
                s = (a + b + c) / 2;
                area += Math.Sqrt(s * (s - a) * (s - b) * (s - c));
            }
            return area;
        }

        private static double Distance(StylusPoint p1, Point2D p2)
        {
            double Xdist = p1.X - p2.X;
            double Ydist = p1.Y - p2.Y;

            return Math.Sqrt(Xdist * Xdist + Ydist * Ydist);
        }
    }
}