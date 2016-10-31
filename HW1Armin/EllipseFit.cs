using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace HW1Armin
{
    internal class EllipseFit
    {
        private int m_fail;
        private bool m_passed = true;
        private Recognizer recognizer;
        private Line2D m_majorAxis;
        private double m_majorAxisLength;
        private double m_majorAxisAngle;
        private Point2D m_center;
        private Line2D.Double m_minorAxis;
        private double m_minorAxisLength;
        public static double M_ELLIPSE_SMALL = 33.0;// 30.0; // L
        public static double M_ELLIPSE_FEATURE_AREA = 0.35; // 0.33; // M
        private double m_err;
        private double m_perimeterStrokeLengthRatio;

        public EllipseFit(Recognizer recognizer)
        {
            this.recognizer = recognizer;
        }

        internal void Test()
        {
            // estimate best ellipse

            try
            {
                calcMajorAxis();
                calcCenter();
                calcMinorAxis();
            }
            catch (Exception e)
            {
                m_passed = false;
                m_fail = 0;
                return;
            }

            // test 1: stroke must be closed
            // if (!m_features.isClosed())
            // m_passed = false;

            // test 2: make sure NDDE is high (close to 1.0) or low (close to 0.0);
            // ignore small ellipses
            if (m_majorAxisLength > M_ELLIPSE_SMALL)
            {
                if (recognizer.getNDDE() < 0.63)
                {
                    m_passed = false;
                    m_fail = 1;
                }

                // closed test but with looser thresholds
                if (recognizer.getEndptStrokeLengthRatio() > 0.3
                        && recognizer.getNumRevolutions() < 1.0)
                {
                    m_passed = false;
                    m_fail = 2;
                }
            }
            else
            {
                if (recognizer.getNDDE() < 0.54)
                {
                    m_passed = false;
                    m_fail = 3;
                }

                // closed test but with looser thresholds
                if (recognizer.getEndptStrokeLengthRatio() > 0.26
                        && recognizer.getNumRevolutions() < 0.75)
                {
                    m_passed = false;
                    m_fail = 4;
                }
            }

            // test 3: feature area test (results used for error)
            //if (!recognizer.isOvertraced())
            //{
            //    m_err = calcFeatureArea();
            //    if (m_majorAxisLength > M_ELLIPSE_SMALL)
            //    {
            //        if (m_err > M_ELLIPSE_FEATURE_AREA)
            //        {
            //            m_passed = false;
            //            m_fail = 5;
            //        }
            //    }
            //    else
            //    {
            //        if (m_err > M_ELLIPSE_FEATURE_AREA
            //                && (m_err > 0.7 || !recognizer.dirWindowPassed()))
            //        {
            //            m_passed = false;
            //            m_fail = 6;
            //        }
            //    }
            //}

            ////// overtraced so we need to compute feature area slightly differently
            //else
            //{
            //    List<StylusPointCollection> subStrokes = recognizer.getRevSegmenter()
            //            .getSegmentations().get(0).getSegmentedStrokes();
            //    double subErr = 0.0;
            //    double centerDis = 0.0, axisDiff = 0.0;
            //    for (int i = 0; i < subStrokes.size() - 1; i++)
            //    {
            //        OrigPaleoSketchRecognizer paleo = new OrigPaleoSketchRecognizer(
            //                subStrokes.get(i), new PaleoConfig());
            //        EllipseFit ef = paleo.getEllipseFit();
            //        if (ef.getFailCode() != 0)
            //        {
            //            centerDis += m_center.distance(ef.getCenter());
            //            axisDiff += Math.Abs(m_majorAxisLength
            //                    - ef.getMajorAxisLength());
            //            subErr += ef.getError();
            //        }
            //    }
            //    m_err = subErr / (subStrokes.size() - 1);
            //    if (Double.IsNaN(m_err))
            //        m_err = calcFeatureArea();
            //    if (m_majorAxisLength > M_ELLIPSE_SMALL)
            //    {
            //        if (m_err > M_ELLIPSE_FEATURE_AREA)
            //        {
            //            m_passed = false;
            //            m_fail = 7;
            //        }
            //    }
            //    else
            //    {
            //        if (m_err > M_ELLIPSE_FEATURE_AREA
            //                && (m_err > 0.65 || !recognizer.dirWindowPassed()))
            //        {
            //            m_passed = false;
            //            m_fail = 8;
            //        }
            //    }
            //}

            double perimeter = recognizer.getBounds().getPerimeter();
            m_perimeterStrokeLengthRatio = recognizer.getStrokeLength() / perimeter;
            if (m_perimeterStrokeLengthRatio < 0.55)
            {
                m_passed = false;
                m_fail = 9;
            }

            if (recognizer.getPctDirWindowPassed() <= 0.25
                    && recognizer.getNumRevolutions() < 0.4)
            {
                m_passed = false;
                m_fail = 10;
            }

            if (recognizer.getAvgCornerStrokeDistance() < 0.1)
            {
                m_passed = false;
                m_fail = 11;
            }

            if (recognizer.getStrokeLengthPerimRatio() > 1.15)
            {
                m_passed = false;
                m_fail = 12;
            }

            // ANOTHER NEW TEST: moving window for direction graph
            // if (!m_features.dirWindowPassed())
            // m_passed = false;

            // create shape/beautified object
            //m_shape = new Ellipse2D.Double(m_center.getX() - m_majorAxisLength / 2,
            //        m_center.getY() - m_minorAxisLength / 2, m_majorAxisLength,
            //        m_minorAxisLength);
            //m_shapePainter = new EllipsePainter(getShape(), getMajorAxisAngle(),
            //        getCenter());
            //try
            //{
            //    computeBeautified();
            //    m_beautified.setAttribute(IsAConstants.CLOSED, "true");
            //}
            //catch (Exception e)
            //{
            //    log.error("Could not create shape object: " + e.getMessage());
            //}
        }

        /**
	     * Get the length of the minor axis for this ellipse estimation
	     * 
	     * @return length of the minor axis
	     */
        public double getMinorAxisLength()
        {
            return m_minorAxisLength;
        }

        /**
	     * Get the length of the major axis for this ellipse estimation
	     * 
	     * @return length of the major axis
	     */
        public double getMajorAxisLength()
        {
            return m_majorAxisLength;
        }

        /**
	     * Get the center point of the ellipse
	     * 
	     * @return center point of the ellipse
	     */
        public Point2D getCenter()
        {
            return m_center;
        }

        internal bool passed()
        {
            return m_passed;
        }

        /**
	     * Estimate the major axis of the ellipse (this is done by finding the two
	     * points that are farthest apart and joining them with a line)
	    */
        protected void calcMajorAxis()
        {
            m_majorAxis = recognizer.getMajorAxis();
            m_majorAxisLength = recognizer.getMajorAxisLength();
            m_majorAxisAngle = recognizer.getMajorAxisAngle();
        }

        /**
	     * Estimate the center point of the ellipse (average of all stroke points)
	     */
        protected void calcCenter()
        {
            /*
             * double avgX = 0, avgY = 0; for (int i = 0; i <
             * m_features.getNumPoints(); i++) { avgX +=
             * m_features.getPoints().get(i).getX(); avgY +=
             * m_features.getPoints().get(i).getY(); } avgX /=
             * m_features.getNumPoints(); avgY /= m_features.getNumPoints();
             * m_center = new Point2D.Double(avgX, avgY);
             */

            m_center = new Point2D(recognizer.getBounds().getCenterX(),
                    recognizer.getBounds().getCenterY());
        }

        /**
         * Estimate the minor axis of the ellipse (perpendicular bisector of the
         * major axis; clipped where it intersects the stroke)
         */
        protected void calcMinorAxis()
        {
            PerpendicularBisector bisect = new PerpendicularBisector(
                    m_majorAxis.GetP1(), m_majorAxis.GetP2(),
                    recognizer.getBounds());
            List<Point2D> intersectPts = recognizer.getIntersection(bisect
                    .getBisector());
            if (intersectPts.Count < 2)
            {
                m_minorAxis = null;
                m_minorAxisLength = 0.0;
            }
            else
            {
                double d1, d2;
                d1 = m_center.Distance(intersectPts[0]);
                d2 = m_center.Distance(intersectPts[1]);
                m_minorAxis = new Line2D.Double(intersectPts[0].X,
                        intersectPts[0].Y, intersectPts[1].X,
                        intersectPts[1].Y);
                m_minorAxisLength = d1 + d2;
            }
        }

        /**
	     * Calculate the feature area error of the ellipse
	     * 
	     * @return feature area error
	     */
        protected double calcFeatureArea()
        {
            double err1 = FeatureArea.toPoint(recognizer.getPoints(), m_center);
            err1 /= (Math.PI * (m_minorAxisLength / 2.0) * (m_majorAxisLength / 2.0));
            err1 = Math.Abs(1.0 - err1);
            if (Double.IsInfinity(err1) || Double.IsNaN(err1))
                err1 = M_ELLIPSE_FEATURE_AREA * 10.0;
            return err1;
        }
    }
}