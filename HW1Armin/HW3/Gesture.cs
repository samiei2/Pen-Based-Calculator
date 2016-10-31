using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW1Armin.HW3
{
    public class Gesture : IComparable
    {
        public string Name;
        public List<PointR> RawPoints; 
        public List<PointR> Points;
        public bool Is1D; 
        public PointR StartUnitVector; 
        public List<Double> VectorVersion; 
        
        public Gesture()
        {
            this.Name = String.Empty;
            this.RawPoints = null;
            this.Points = null;
            this.Is1D = true; 
        }

        public Gesture(string name, List<PointR> points) : this(points)
        {
            this.Name = name;
        }

        public Gesture(List<PointR> points)
        {
            this.Name = String.Empty;
            this.RawPoints = new List<PointR>(points);

            Points = RawPoints;
            
            Points = Utils.Resample(Points, 64);
            
            double radians = Utils.AngleInRadians(Utils.Centroid(Points), (PointR)Points[0], false);
            Points = Utils.RotateByRadians(Points, -radians);
            
            this.Is1D = Utils.Is1DGesture(RawPoints);
            
            Points = Utils.Scale(Points, 0.3f, new SizeR(250, 250));
            
            // next, if NOT rotation-invariant, rotate back
            if (!NDollarParameters.Instance.RotationInvariant)
            {
                Points = Utils.RotateByRadians(Points, +radians); // undo angle
            }
            
            Points = Utils.TranslateCentroidTo(Points, new PointR(0,0));
            
            this.StartUnitVector = Utils.CalcStartUnitVector(Points, 8);
            
            this.VectorVersion = Vectorize(this.Points);
        }

        public long Duration
        {
            get
            {
                if (RawPoints.Count >= 2)
                {
                    PointR p0 = (PointR)RawPoints[0];
                    PointR pn = (PointR)RawPoints[RawPoints.Count - 1];
                    return pn.T - p0.T;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double AveragePressure
        {
            get
            {
                if (RawPoints.Count >= 2)
                {
                    double temp = 0;
                    foreach (PointR p in RawPoints)
                    {
                        temp += p.P;
                    }
                    return temp / RawPoints.Count;
                }
                else
                {
                    return 0;
                }
            }
        }
        
        public int CompareTo(object obj)
        {
            if (obj is Gesture)
            {
                Gesture g = (Gesture)obj;
                return Name.CompareTo(g.Name);
            }
            else throw new ArgumentException("object is not a Gesture");
        }
        
        public static string ParseName(string filename)
        {
            int start = filename.LastIndexOf('\\');
            int end = filename.LastIndexOf('.');
            return filename.Substring(start + 1, end - start - 1);
        }
        
        public static string ParseUser(string name)
        {
            int start = name.IndexOf("_");
            int end = name.IndexOf("_", start + 1);
            return name.Substring(start + 1, end - start - 1);
        }
        
        private List<Double> Vectorize(List<PointR> pts)
        {
            // skip the resampling, translation because $N already did this in pre-processing
            // re-do the rotation though
            // (note: doing rotation  on the pre-processed points is ok because $N rotates it back to the
            // original orientation if !RotationInvariant, e.g., it is rotation sensitive)

            // extract indicative angle (delta)
            double indicativeAngle = Math.Atan2(pts[0].Y, pts[0].X);
            double delta;
            if (!NDollarParameters.Instance.RotationInvariant) // rotation sensitive
            {
                double baseOrientation = (Math.PI / 4) * Math.Floor((indicativeAngle + Math.PI / 8) / (Math.PI / 4));
                delta = baseOrientation - indicativeAngle;
            }
            else
            {
                delta = -indicativeAngle;
            }

            // find the match
            double sum = 0;
            List<Double> vector = new List<Double>();
            foreach (PointR p in pts)
            {
                double newX = p.X * Math.Cos(delta) - p.Y * Math.Sin(delta);
                double newY = p.Y * Math.Cos(delta) + p.X * Math.Sin(delta);
                vector.Add(newX);
                vector.Add(newY);
                sum += newX * newX + newY * newY;
            }
            double magnitude = Math.Sqrt(sum);
            for (int i = 0; i < vector.Count; i++) //foreach (Double d in vector)
            {
                vector[i] /= magnitude;
            }
            return vector;
        }

    }
}
