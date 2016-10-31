using System;
using System.Windows.Input;

namespace HW1Armin
{
    internal class CircleFit
    {
        private EllipseFit m_ellipseFit;
        private int m_fail;
        private bool m_passed = true;
        private Recognizer recognizer;
        private double m_radius;
        public static double M_CIRCLE_SMALL = 16.0; // N
        public static double M_NDDE_HIGH = 0.79; // K
        public static double M_AXIS_RATIO_TO_BE_CIRCLE = 0.5;// 0.6;//0.425; O
        private double m_axisRatio;

        public CircleFit(Recognizer recognizer)
        {
            this.recognizer = recognizer;
        }

        public CircleFit(Recognizer recognizer, EllipseFit ellipseFit) : this(recognizer)
        {
            this.m_ellipseFit = ellipseFit;
        }

        internal void Test()
        {

            // estimate the radius of the circle
            calcRadius();

            // test 1: stroke must be closed
            if (!recognizer.isClosed())
            {
                m_passed = false;
                m_fail = 0;
            }

            // test 2: make sure NDDE is high (close to 1.0) or low (close to 0.0);
            // ignore small circles
            if (recognizer.getNDDE() < M_NDDE_HIGH && m_radius > M_CIRCLE_SMALL)
            {
                m_passed = false;
                m_fail = 1;
            }

            // test 3: check major axis to minor axis ratio
            m_axisRatio = Math.Abs(1.0 - m_ellipseFit.getMajorAxisLength()
                    / m_ellipseFit.getMinorAxisLength());
            if (Double.IsInfinity(m_axisRatio) || Double.IsNaN(m_axisRatio))
                m_axisRatio = 1.0;
            if (m_axisRatio > M_AXIS_RATIO_TO_BE_CIRCLE)
            {
                m_passed = false;
                m_fail = 2;
            }

            // test 4: feature area test (results used for error)
            //if (!recognizer.isOvertraced())
            //{
            //    m_err = calcFeatureArea();
            //    if (m_err > M_CIRCLE_FEATURE_AREA)
            //    {
            //        m_passed = false;
            //        m_fail = 3;
            //    }
            //}

            //// overtraced so we need to compute feature area slightly differently
            //else
            //{
            //    List<Stroke> subStrokes = recognizer.getRevSegmenter()
            //            .getSegmentations().get(0).getSegmentedStrokes();
            //    double subErr = 0.0;
            //    double centerDis = 0.0, radDiff = 0.0;
            //    for (int i = 0; i < subStrokes.size(); i++)
            //    {
            //        if (i != subStrokes.size() - 1)
            //        {
            //            OrigPaleoSketchRecognizer paleo = new OrigPaleoSketchRecognizer(
            //                    subStrokes.get(i), new PaleoConfig());
            //            CircleFit cf = paleo.getCircleFit();
            //            centerDis += m_ellipseFit.getCenter().distance(
            //                    cf.getCenter());
            //            radDiff += Math.abs(m_radius - cf.getRadius());
            //            subErr += cf.getError();
            //        }
            //        else
            //        {
            //            // fit last substroke to an arc (part of a circle)
            //            OrigPaleoSketchRecognizer paleo = new OrigPaleoSketchRecognizer(
            //                    subStrokes.get(i), new PaleoConfig());
            //            ArcFit af = paleo.getArcFit();
            //            centerDis += m_ellipseFit.getCenter().distance(
            //                    af.getCenter());
            //            radDiff += Math.abs(m_radius - af.getRadius());
            //        }
            //    }
            //    if (subStrokes.size() > 1)
            //        m_err = subErr / (subStrokes.size() - 1);
            //    if (m_err > M_CIRCLE_FEATURE_AREA)
            //    {
            //        m_passed = false;
            //        m_fail = 4;
            //    }
            //}
        }

        internal bool passed()
        {
            return m_passed;
        }

        /**
	     * Estimate the best radius for the circle fit
	     */
        protected void calcRadius()
        {
            // calc average radius (average of all points)
            m_radius = 0.0;
            if (m_ellipseFit.getCenter() != null)
            {
                for (int i = 0; i < recognizer.getNumPoints(); i++)
                    m_radius += distance(recognizer
                            .getPoints()[i],new StylusPoint(m_ellipseFit.getCenter().X,
                                    m_ellipseFit.getCenter().Y));
                m_radius /= recognizer.getNumPoints();
            }
        }

        public double distance(StylusPoint p1, StylusPoint p2)
        {
            double Xdist = p1.X - p2.X;
            double Ydist = p1.Y - p2.Y;

            return Math.Sqrt(Xdist * Xdist + Ydist * Ydist);
        }
    }
}