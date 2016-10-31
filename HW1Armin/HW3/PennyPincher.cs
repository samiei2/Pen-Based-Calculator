using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace HW1Armin.HW3
{
    class PennyPincher
    {
        private readonly int NUMRESAMPLEPOINTS = 64;

        public NBestList Recognize(StrokeCollection strokes, int sampleCount = 1)
        {
            List<PointR> points = new List<PointR>();
            foreach (var pts in strokes)
            {
                points.AddRange(GetPointRs(pts));
            }
            PennyGesture result = new PennyGesture();
            var c = pennyPincherResample(points);
            var similarity = Double.NegativeInfinity;
            NBestList nbest = new NBestList();
            var dataSet = Dataset.Instance.GetPennyPincherSampleCount(sampleCount);
            foreach (PennyGesture item in dataSet.Values)
            {
                {
                    double d = 0;
                    var count = Math.Min(item.Points.Count,c.Count);
                    for (int i = 0; i < count - 2; i++)
                    {
                        var tp = item.Points[i];
                        var cp = c[i];

                        d = d + tp.X * cp.X + tp.Y * cp.Y;
                    }
                    if (d > similarity)
                    {
                        similarity = d;
                        nbest.AddResult(item.name, similarity, 0, 0);
                        result = item;
                    }
                }
            }
            nbest.SortDescending();
            return nbest;
        }

        private List<PointR> ResampleBetweenPoints(List<PointR> points)
        {
            var I = PathLength(points) / (NUMRESAMPLEPOINTS - 1);
            double bigD = 0;
            List<PointR> v = new List<PointR>();
            var prevP = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                var thisPoint = points[i];
                var prevPoint = points[i - 1];
                var pDist = Distance(thisPoint, prevPoint);

                if (bigD + pDist > I)
                {
                    var qx = points[i - 1].X + (points[i].X - points[i - 1].X) * (I - bigD) / pDist;
                    var qy = points[i - 1].Y + (points[i].Y - points[i - 1].Y) * (I - bigD) / pDist;
                    PointR q = new PointR(qx,qy);

                    var rx = q.X - prevP.X;
                    var ry = q.Y - prevP.Y;
                    var r = new PointR(rx,ry);
                    var rd = Distance(new PointR(0, 0), r);
                    r.X = rx / rd;
                    r.Y = ry / rd;

                    bigD = 0;
                    prevP = q;

                    v.Add(r);
                    points.Insert(i,q);
                }
                else
                {
                    bigD = bigD + pDist;
                }
            }
            return v;
        }

        public static List<PointR> pennyPincherResample(List<PointR> points, int numberOfPoints = 32)
        {
            double I = getStrokeLength(points) / (numberOfPoints - 1); // interval length
            double D = 0.0;
            List<PointR> srcPts = new List<PointR>(points);
            List<PointR> vector = new List<PointR>(numberOfPoints);

            PointR prev = srcPts[0];
            for (int i = 1; i < srcPts.Count; i++)
            {
                PointR pt1 = (PointR)srcPts[i - 1];
                PointR pt2 = (PointR)srcPts[i];

                double d = getDistanceBetween(pt1, pt2);
                if ((D + d) >= I)
                {
                    double qx = pt1.X + ((I - D) / d) * (pt2.X - pt1.X);
                    double qy = pt1.Y + ((I - D) / d) * (pt2.Y - pt1.Y);
                    PointR q = new PointR(qx, qy);
                    PointR r = new PointR(qx - prev.X, qy - prev.Y);
                    double rd = getDistanceBetween(new PointR(0, 0), r);
                    PointR rr = new PointR(r.X / rd, r.Y / rd);
                    vector.Add(rr); // append new vector 'rr'
                    srcPts.Insert(i, q); // insert 'q' at position i in points s.t. 'q' will be the next i
                    D = 0.0;
                    prev = q;
                }
                else
                {
                    D += d;
                }
            }

            return vector;
        }

        public static double getStrokeLength(List<PointR> points)
        {
            double length = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                length += getDistanceBetween(points[i], points[i + 1]);
            }
            return length;
        }

        // get distance of two points
        public static double getDistanceBetween(PointR p1, PointR p2)
        {
            return (Math.Sqrt(Math.Pow(Math.Abs(p1.X - p2.X), 2) + Math.Pow(Math.Abs(p1.Y - p2.Y), 2)));
        }

        private IEnumerable<PointR> GetPointRs(Stroke pts)
        {
            List<PointR> points = new List<PointR>();
            foreach (var item in pts.StylusPoints)
            {
                points.Add(new PointR(item.X, item.Y));
            }
            return points;
        }

        private double PathLength(List<PointR> points)
        {
            double d = 0f;
            for (int i = 1; i < points.Count - 1; i++)
            {
                d = d + Distance(points[i-1],points[i]);
            }
            return d;
        }

        private double Distance(PointR p1,PointR p2)
        {
            return Math.Sqrt(Math.Pow((p2.X - p1.X),2) + Math.Pow((p2.Y - p1.Y), 2));
        }
    }
}
