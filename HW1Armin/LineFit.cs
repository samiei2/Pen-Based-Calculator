using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HW1Armin
{
    class LineFit
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

        public void FitTest(Recognizer m_features,bool useEndpoints = false)
        {
            // if only two points then we have a line with no error
            if (m_features.getNumPoints() <= 2)
            {
                //log.debug("LineFit: stroke contains " + m_features.getNumPoints()
                //        + " points");
                m_err = 0.0;
                m_lsqe = 0.0;
                m_passed = true;
                return;
            }

            // ideal line (just connect the endpoints)
            if (useEndpoints)
                m_shape = new Line2D.Double(m_features.getFirstOrigPoint().X,
                        m_features.getFirstOrigPoint().Y, m_features
                                .getLastOrigPoint().X, m_features
                                .getLastOrigPoint().Y);
            else
                m_shape = m_features.getMajorAxis();

            // test 1: least squares error between the stroke points and the line
            // formed by the endpoints
            m_lsqe = LeastSquares.error(m_features.getPoints(), (Line2D)m_shape);
            m_ratio = m_features.getEndptStrokeLengthRatio();
            if (m_features.getStrokeLength() > 25.0)
            {
                if (m_lsqe / m_features.getStrokeLength() > 1.4)
                {
                    m_passed = false;
                    m_fail = 0;

                    // if line test was close and endpt ratio is high then go ahead
                    // and pass
                    if (m_lsqe / m_features.getStrokeLength() < M_LINE_LS_ERROR_FROM_ENDPTS + 0.1
                            && m_ratio > 0.98)
                        m_passed = true;
                }
                if (m_lsqe / m_features.getStrokeLength() > 1.25 && m_ratio < 0.75
                        && m_features.getNumRevolutions() <= 0.5)
                {
                    m_passed = false;
                    m_fail = 1;
                }
            }
            else
            {
                if (m_lsqe / m_features.getStrokeLength() > 1.5 && m_ratio < 0.732)
                {
                    m_passed = false;
                    m_fail = 2;
                }
                if (m_lsqe / m_features.getStrokeLength() > 1.25 && m_ratio < 0.7
                        && m_features.getNumRevolutions() < 0.15)
                {
                    m_passed = false;
                    m_fail = 3;
                }
            }

            // test 2: verify that stroke is not overtraced
            if (m_features.isOvertraced())
            {
                m_passed = false;
                m_fail = 4;
            }

            // test 3: test feature area (use as error for fit)
            m_err = FeatureArea.toLine(m_features.getPoints(), (Line2D)m_shape)
                    / m_features.getStrokeLength();
            if (m_err > M_LINE_FEATURE_AREA)
            {
                m_passed = false;
                m_fail = 5;
            }

            // test 4: ratio must be near 1.0
            //if (m_ratio > 0.99) // definitely a line
            //    m_passed = true;
            if (m_ratio < 0.3)
            { // definitely not a line
                m_passed = false;
                m_fail = 6;
            }

            lineLength = m_features.getStrokeLength();

            // compute beautified shape
            try
            {
                //computeBeautified();
            }
            catch (Exception e)
            {
                //log.error("Could not create shape object: " + e.getMessage());
            }

            //log.debug("LineFit: passed = " + m_passed + "(" + m_fail
            //        + ") least sq error = "
            //        + (m_lsqe / m_features.getStrokeLength()) + "  overtraced = "
            //        + m_features.isOvertraced() + "  feature area error = " + m_err
            //        + "  endpts = (" + ((Line2D)m_shape).getX1() + ","
            //        + ((Line2D)m_shape).getY1() + ") ("
            //        + ((Line2D)m_shape).getX2() + "," + ((Line2D)m_shape).getY2()
            //        +
            //        /* ") corners = " + m_features.numFinalCorners() + */
            //        "  best fit = (" + m_features.getBestFitLine().getX1() + ","
            //        + m_features.getBestFitLine().getY1() + ") ("
            //        + m_features.getBestFitLine().getX2() + ","
            //        + m_features.getBestFitLine().getY2() + ")  is closed = "
            //        + m_features.isClosed() + " ratio = " + m_ratio + " length = "
            //        + m_features.getStrokeLength() + " revs = "
            //        + m_features.getNumRevolutions());
        }

        internal void getFit(StylusPointCollection p)
        {
            var recognizer = new Recognizer(p);
            recognizer.PreProcess();
            FitTest(recognizer);
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
