using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW1Armin.HW3
{
    public struct PointR
    {
        public static readonly PointR Empty;
        public double X, Y;
        public long T;
        public double P;

        public PointR(double x, double y) : this(x, y, 0)
        {
        }

        public PointR(double x, double y, long t)
        {
            X = x;
            Y = y;
            T = t;
            P = -1;
        }

        public PointR(double x, double y, long t, double p)
        {
            X = x;
            Y = y;
            T = t;
            P = p;
        }

        // copy constructor
        public PointR(PointR p)
        {
            X = p.X;
            Y = p.Y;
            T = p.T;
            P = p.P;
        }
        

        public static bool operator ==(PointR p1, PointR p2)
        {
            return (p1.X == p2.X && p1.Y == p2.Y);
        }

        public static bool operator !=(PointR p1, PointR p2)
        {
            return (p1.X != p2.X || p1.Y != p2.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is PointR)
            {
                PointR p = (PointR)obj;
                return (X == p.X && Y == p.Y);
            }
            return false;
        }
        
    }
}
