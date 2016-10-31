using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HW1Armin
{
    class ScratchFit
    {
        /**
         * Least squares error
         */
        protected double m_lsqe;

        /**
         * Endpoint to stroke length ratio
         */
        protected double m_ratio;

        /**
	     * Error of the fit
	     */
        protected double m_err;

        /**
         * Flag denoting whether fit tests passed or not
         */
        protected bool m_passed = true;

        /**
	     * Fail code
	     */
        protected int m_fail = -1;

        private Line2D m_shape;

        public static double M_LINE_LS_ERROR_FROM_ENDPTS = 1.5;

        public static double M_LINE_FEATURE_AREA = 10.25; // H
        public double lineLength;

        public void FitTest(Recognizer m_features, List<Point> corners)
        {
            m_passed = false;
            if(corners.Count > 5)
            {
                PolyLineFit2 polyfittest = new PolyLineFit2(m_features, corners);
                polyfittest.Test();
                if(polyfittest.passed() && polyfittest.GetSubStrokes().Count > 4)
                {
                    bool allAnglesAcute = true;
                    for (int i = 1; i < corners.Count - 2; i++)
                    {
                        double angles = getAngelOfThreePoints(corners[i - 1], corners[i], corners[i + 1]);
                        if (angles > 20)
                            allAnglesAcute = false;
                    }
                    if (allAnglesAcute)
                        m_passed = true;
                }
            }
        }

        public double getAngelOfThreePoints(Point p1, Point center, Point p2)
        {
            double dp12 = getDistanceBetween(p1, center);
            double dp23 = getDistanceBetween(center, p2);
            double dp13 = getDistanceBetween(p1, p2);
            double up = Math.Pow(dp12, 2) + Math.Pow(dp23, 2) - Math.Pow(dp13, 2);
            double down = 2 * dp12 * dp23;
            return Math.Acos(up / down) * (180 / Math.PI);
        }

        private double getDistanceBetween(Point p1, Point p2)
        {
            return (Math.Sqrt(Math.Pow(Math.Abs(p1.X - p2.X), 2) + Math.Pow(Math.Abs(p1.Y - p2.Y), 2)));
        }

        public bool passed()
        {
            return m_passed;
        }

        internal double getLSQE()
        {
            return m_lsqe;
        }
        
        internal double getError()
        {
            return m_err;
        }
    }
}
